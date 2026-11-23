using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cty.DoubleClick;
using Cty.World;
using Cty.Paint;

public class DragCamera:MonoBehaviour
{
    [SerializeField] private float dSpeed = 1f;
    [SerializeField] private float edgeSize = 20f;
    [SerializeField] private float mSpeed = 60f;
    [SerializeField] private float zSpeed = 20f;
    [SerializeField] private float minH = 10f;
    [SerializeField] private float maxH = 230f;
    [SerializeField] private float zoomSensitivity = 0.02f;
    [SerializeField] private float dragSensitivity = 0.5f;
    [SerializeField] private float maxX = 16320f;
    [SerializeField] private float maxY = 10050f;
    // Name of the id map file in Resources (without .bytes), made by make_idmap.py.
    [SerializeField] private string idMapResource = "idmap";
    // Pixels around the clicked one that vote on the county (2 = 5x5, 0 = clicked pixel only).
    // Antialiased seams in the source raster are blends that can decode to a wrong id; the vote outvotes them.
    [SerializeField] private int sampleRadius = 2;
    [SerializeField] private bool debugClicks = true;

    private GameObject mapObject;
    private GameObject textureObject;
    private float camangle;
    private float targetHeight;
    private Plane map;
    private Vector3 dragOrigin;
    private float camx;
    private float camy;
    private bool up;
    private bool right;
    private bool down;
    private bool left;
    public float rayDistance;
    private IdMap idMap;
    private int pulse = 0;
    private readonly Dictionary<int,int> votes = new Dictionary<int,int>();
    private PaintFill paintFill;
    public County SelectedCounty { get; private set; }

    void Start(){
        map = new Plane(Vector3.up, Vector3.zero);
        targetHeight = maxH;
        camangle = Camera.main.fieldOfView * Mathf.Deg2Rad /2f;
        mapObject = GameObject.Find("Map");
        textureObject = GameObject.Find("VisualMap");
        idMap = IdMap.Load(idMapResource);
        ValidateSetup();
        paintFill = GameObject.Find("Initializer").GetComponent<PaintFill>();
    }

    // Logs the setup problems that make clicks land on the wrong pixel.
    private void ValidateSetup(){
        if(mapObject == null){
            Debug.LogWarning("No GameObject named 'Map' was found, so the map collider and texture weren't checked");
            return;
        }
        if(!(mapObject.GetComponent<Collider>() is MeshCollider)){
            Debug.LogWarning("'Map' has no MeshCollider. Without one, hit.textureCoord is always (0,0).");
        }
        Renderer r = mapObject.GetComponent<Renderer>();
        if(idMap != null && r != null && r.sharedMaterial != null && r.sharedMaterial.mainTexture != null){
            Texture shown = r.sharedMaterial.mainTexture;
            Debug.Log($"Map shows '{shown.name}' {shown.width}x{shown.height}; id map is {idMap.Width}x{idMap.Height}");

            


            float shownAspect = (float)shown.width / shown.height;
            float idAspect = (float)idMap.Width / idMap.Height;
            if(Mathf.Abs(shownAspect - idAspect) / idAspect > 0.01f){
                Debug.LogWarning("The texture shown on 'Map' and the id map have different aspect ratios, so clicks will land on the wrong pixel unless the id map covers the same extent. " + idAspect);
            }
        }
    }

    void Update(){
        Mouse mouse = Mouse.current;
        if(mouse == null) return;

        Vector3 pos = transform.position;
        camy = Mathf.Tan(camangle) * pos.y;
        camx = camy * Camera.main.aspect;
        left = Mathf.Abs(pos.x -camx) > maxX;
        right = Mathf.Abs(pos.x + camx) > maxX;
        up = Mathf.Abs(pos.z+camy) > maxY;
        down = Mathf.Abs(pos.z-camy) > maxY;

        // Take any new double click now so one that arrives on a non-press frame can't go stale.
        int handlerPulse = DoubleClickHandler.pulse;
        bool doubleClicked = handlerPulse > pulse;
        pulse = handlerPulse;

        float scroll = mouse.scroll.ReadValue().y;
        targetHeight -= scroll *zoomSensitivity;
        targetHeight = Mathf.Clamp(targetHeight, minH, maxH);

        pos.y = Mathf.Lerp(pos.y,targetHeight,12f*Time.deltaTime);
        transform.position = pos;
        if(mouse.leftButton.wasPressedThisFrame){
            dragOrigin = getClickPoint();
            if(doubleClicked){
                SelectCountyAtMouse();
                paintFill.FillAtWorldPoint(getClickPoint());
            }
        }
        if(mouse.leftButton.isPressed){
            Vector3 current = getClickPoint();
            Vector3 dragMove = dragOrigin-current;
            if(right && dragMove.x > 0){dragMove.x = 0;}
            if(left && dragMove.x < 0) dragMove.x = 0;
            if(up && dragMove.z > 0) dragMove.z = 0;
            if(down && dragMove.z < 0) dragMove.z = 0;
            transform.position += dragMove*dragSensitivity;
            dragOrigin = current;
        }
        //Debug.Log("Left: " + left+" Right: " + right+" Up: " + up+" Down: " + down);
        Vector3 move = Vector3.zero;
        Vector3 mousePos = mouse.position.ReadValue();
        if(mousePos.x < edgeSize && !down) move.x-=1;
        if(mousePos.x > Screen.width-edgeSize && !right) move.x+=1;
        if(mousePos.y < edgeSize && !left) move.z-=1;
        if(mousePos.y > Screen.height-edgeSize && !up) move.z+=1;
        if(right && move.x > 0){move.x = 0;}
        if(left && move.x < 0) move.x = 0;
        if(up && move.z > 0) move.z = 0;
        if(down && move.z < 0) move.z = 0;
        transform.position += move.normalized*mSpeed*Time.deltaTime;

    }
    public Vector3 getClickPoint(){
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(map.Raycast(ray, out float distance)){
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void SelectCountyAtMouse(){
        if(WorldHandler.counties == null){
            Debug.LogError("WorldHandler.counties isn't loaded (is a WorldHandler on a GameObject in the scene?)");
            return;
        }
        if(idMap == null){
            Debug.LogError("The id map isn't loaded (see the error logged at startup)");
            return;
        }
        if(!TryGetClickPixel(out int px, out int py)) return;

        int centerId = idMap.GetId(px, py);

        // Every real county id in the window gets a vote; the most common one wins.
        votes.Clear();
        int rad = Mathf.Max(0, sampleRadius);
        for(int dy = -rad; dy <= rad; dy++){
            for(int dx = -rad; dx <= rad; dx++){
                int id = idMap.GetId(px + dx, py + dy);
                if(id != 0 && WorldHandler.counties.ContainsKey(id)){
                    votes.TryGetValue(id, out int n);
                    votes[id] = n + 1;
                }
            }
        }

        int bestId = -1;
        int bestVotes = 0;
        foreach(KeyValuePair<int,int> kv in votes){
            if(kv.Value > bestVotes){
                bestId = kv.Key;
                bestVotes = kv.Value;
            }
        }
        // On a tie, trust the pixel that was actually clicked.
        if(votes.TryGetValue(centerId, out int centerVotes) && centerVotes == bestVotes){
            bestId = centerId;
        }

        if(bestVotes == 0){
            SelectedCounty = null;
            Debug.Log($"No county at pixel ({px},{py}): id there is {centerId}");
            return;
        }
        SelectedCounty = WorldHandler.counties[bestId];
        int sampled = (2 * rad + 1) * (2 * rad + 1);
        Debug.Log($"Selected id {bestId}: {SelectedCounty.name} ({SelectedCounty.ParentPrefectureName}, {SelectedCounty.ParentProvinceName}). Clicked pixel ({px},{py}) has id {centerId}; {bestVotes} of {sampled} sampled pixels agree");
    }

    // Raycasts under the mouse and converts the hit's UV to a pixel of the id map.
    private bool TryGetClickPixel(out int px, out int py){
        px = 0;
        py = 0;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float dist = rayDistance > 0f ? rayDistance : Mathf.Infinity;
        if(!Physics.Raycast(ray, out RaycastHit hit, dist)){
            Debug.Log("Click missed the map (no collider hit; check rayDistance and that the map has a collider)");
            return false;
        }
        if(!(hit.collider is MeshCollider)){
            Debug.LogWarning("Hit '" + hit.collider.name + "' which has no MeshCollider; textureCoord is always (0,0) without one");
            return false;
        }

        Vector2 uv = hit.textureCoord;
        Renderer r = hit.collider.GetComponent<Renderer>();
        if(r != null && r.sharedMaterial != null){
            Material m = r.sharedMaterial;
            uv = Vector2.Scale(uv, m.mainTextureScale) + m.mainTextureOffset;
        }

        px = Mathf.Clamp(Mathf.FloorToInt(uv.x * idMap.Width),  0, idMap.Width  - 1);
        py = Mathf.Clamp(Mathf.FloorToInt(uv.y * idMap.Height), 0, idMap.Height - 1);
        if(debugClicks){
            Debug.Log($"Hit '{hit.collider.name}': uv ({uv.x:F5},{uv.y:F5}) -> pixel ({px},{py}) of {idMap.Width}x{idMap.Height}");
        }
        return true;
    }

}
