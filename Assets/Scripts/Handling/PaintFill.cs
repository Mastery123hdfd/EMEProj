using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paint-bucket (flood fill) tool.
///
/// Receives a coordinate from another script (e.g. your mouse script) and fills the
/// matching region of the texture named "Empty", which sits on a material named "Empty",
/// which sits on a quad named "Empty".
///
/// USAGE FROM YOUR PREVIOUS (MOUSE) SCRIPT - pick whichever coordinate type you have:
///
///     public PaintFill paintFill;   // drag this component in via the Inspector
///
///     if (Input.GetMouseButtonDown(0))
///         paintFill.FillAtScreenPoint(Input.mousePosition);   // screen-space mouse position
///
///     // or, if your script already has a world point on the quad:
///     paintFill.FillAtWorldPoint(hit.point);
///
///     // or, if it already has UV (0-1) or texture pixel coordinates:
///     paintFill.FillAtUV(hit.textureCoord);
///     paintFill.FillAtPixel(px, py);
/// </summary>
/// 
namespace Cty.Paint { 
public class PaintFill : MonoBehaviour
{
    [Header("Target (all named 'Empty' by default)")]
    [Tooltip("Optional. If left empty, the script finds a GameObject with the name below.")]
    public GameObject quad;
    public string quadName = "Empty";
    public string materialName = "Empty";
    public string textureName = "Empty";

    [Tooltip("Camera used to convert a screen-space mouse position. Defaults to Camera.main.")]
    public Camera cam;

    [Header("Texture (only used if a readable 'Empty' texture isn't already on the material)")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color baseColor = Color.white;

    [Header("Fill")]
    public Color fillColor = Color.red;
    [Tooltip("0 = only exact color matches. Higher values also fill similar colors (helps with anti-aliased edges).")]
    [Range(0, 255)] public int tolerance = 0;

    [Header("Display / Debug")]
    [Tooltip("Edit the quad's shared material asset instead of a per-object copy. Try this if another script holds a reference to the original 'Empty' material.")]
    public bool useSharedMaterial = false;
    [Tooltip("Set the material's tint (_BaseColor / _Color) to white so it can't hide or darken the texture.")]
    public bool resetTint = true;
    [Tooltip("Log setup details and every fill to the Console.")]
    public bool debugLog = true;

    Texture2D tex;
    Material mat;
    Collider quadCollider;
    string texProp;
    Renderer quadRenderer;

    void Awake()
    {
        Resolve();
    }

    // ------------------------------------------------------------------
    // Setup: find quad -> material -> texture, and create them if needed
    // ------------------------------------------------------------------
    void Resolve()
    {
        if (quad == null) quad = GameObject.Find(quadName);
        if (quad == null)
        {
            Debug.LogError($"PaintFill: no GameObject named '{quadName}' found.", this);
            enabled = false;
            return;
        }

        Renderer rend = quad.GetComponent<Renderer>();
        quadRenderer = rend;
        if (rend == null)
        {
            Debug.LogError("PaintFill: the quad has no Renderer.", this);
            enabled = false;
            return;
        }

        // A Quad primitive ships with a MeshCollider, but add one if it was removed.
        quadCollider = quad.GetComponent<Collider>();
        if (quadCollider == null) quadCollider = quad.AddComponent<MeshCollider>();

        if (debugLog)
        {
            Debug.Log($"PaintFill: using GameObject '{quad.name}' at world position {quad.transform.position}, " +
                      $"renderer enabled = {rend.enabled}, material slots = {rend.sharedMaterials.Length}. " +
                      "Click this log line to highlight the object in the Hierarchy.", quad);
        }

        // By default use a per-object material instance so a shared asset isn't edited.
        mat = useSharedMaterial ? rend.sharedMaterial : rend.material;
        if (mat == null)
        {
            Debug.LogError("PaintFill: the quad's Renderer has no material.", this);
            enabled = false;
            return;
        }
        if (!useSharedMaterial) mat.name = materialName;

        // Reuse an existing readable texture (e.g. one a paint script is already drawing to).
        Texture2D existing = mat.mainTexture as Texture2D;
        bool reusable = existing != null && existing.isReadable &&
                        (existing.format == TextureFormat.RGBA32 ||
                         existing.format == TextureFormat.ARGB32 ||
                         existing.format == TextureFormat.RGB24);

        if (reusable)
        {
            tex = existing;
        }
        else
        {
            tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color32 c = baseColor;
            Color32[] pixels = new Color32[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels32(pixels);
            tex.Apply();
        }

        tex.name = textureName;
        ApplyTextureToMaterial();
    }

    // Shaders name their main texture slot differently (_MainTex, _BaseMap, ...),
    // so set every standard one that exists, and fall back to the shader's first texture slot.
    void ApplyTextureToMaterial()
    {
        texProp = null;

        foreach (string prop in new[] { "_BaseMap", "_MainTex", "_BaseColorMap" })
        {
            if (!mat.HasProperty(prop)) continue;
            mat.SetTexture(prop, tex);
            if (texProp == null) texProp = prop;
        }

        if (texProp == null)
        {
            string[] names = mat.GetTexturePropertyNames();
            if (names.Length > 0)
            {
                texProp = names[0];
                mat.SetTexture(texProp, tex);
            }
        }

        if (resetTint)
        {
            foreach (string prop in new[] { "_BaseColor", "_Color" })
            {
                if (mat.HasProperty(prop)) mat.SetColor(prop, Color.white);
            }
        }

        if (debugLog)
        {
            Debug.Log($"PaintFill: texture '{tex.name}' ({tex.width}x{tex.height}) -> material '{mat.name}', " +
                      $"shader '{mat.shader.name}', texture slot '{(texProp ?? "NONE FOUND")}'.", this);
        }
    }

    // ------------------------------------------------------------------
    // Public entry points - call these from your mouse script
    // ------------------------------------------------------------------

    /// <summary>Screen-space position (e.g. Input.mousePosition). Returns true if it hit the quad.</summary>
    public bool FillAtScreenPoint(Vector2 screenPoint)
    {
        if (tex == null) Resolve();
        if (tex == null) return false;

        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("PaintFill: no camera assigned and no Camera.main found.", this);
            return false;
        }

        Ray ray = cam.ScreenPointToRay(screenPoint);

        // Raycast against the quad's collider only, so other objects can't block it.
        if (quadCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            FillAtUV(hit.textureCoord);
            return true;
        }
        return false;
    }

    /// <summary>
    /// World-space point on the quad's plane. Returns false (and fills nothing) if the point
    /// falls outside the quad.
    /// </summary>
    public bool FillAtWorldPoint(Vector3 worldPoint)
    {
        if (tex == null) Resolve();
        if (tex == null) return false;

        // A Unity Quad spans -0.5..0.5 in local X/Y, with UVs 0..1.
        Vector3 local = quad.transform.InverseTransformPoint(worldPoint);
        float u = local.x + 0.5f;
        float v = local.y + 0.5f;
        if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

        FillAtUV(new Vector2(u, v));
        return true;
    }

    /// <summary>UV coordinate (0-1 on both axes).</summary>
    public void FillAtUV(Vector2 uv)
    {
        if (tex == null) Resolve();
        if (tex == null) return;

        int x = Mathf.Clamp(Mathf.FloorToInt(uv.x * tex.width), 0, tex.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(uv.y * tex.height), 0, tex.height - 1);
        FillAtPixel(x, y);
    }

    /// <summary>Texture pixel coordinate (0,0 = bottom-left).</summary>
    public void FillAtPixel(int x, int y)
    {
        if (tex == null) Resolve();
        if (tex == null) return;

        int w = tex.width;
        int h = tex.height;
        if (x < 0 || y < 0 || x >= w || y >= h) return;

        Color32[] px = tex.GetPixels32();
        Color32 target = px[y * w + x];
        Color32 fill = fillColor;

        // Clicking a pixel that's already the fill color: nothing to do.
        if (SameColor(target, fill)) return;

        bool[] visited = new bool[w * h];
        int filled = 0;
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));

        // Scanline flood fill: fills a whole horizontal run at a time, then seeds the rows above/below.
        while (stack.Count > 0)
        {
            Vector2Int p = stack.Pop();
            int row = p.y * w;

            if (visited[row + p.x] || !Matches(px[row + p.x], target)) continue;

            int left = p.x;
            while (left > 0 && !visited[row + left - 1] && Matches(px[row + left - 1], target)) left--;

            int right = p.x;
            while (right < w - 1 && !visited[row + right + 1] && Matches(px[row + right + 1], target)) right++;

            for (int i = left; i <= right; i++)
            {
                visited[row + i] = true;
                px[row + i] = fill;
                filled++;
            }

            for (int dy = -1; dy <= 1; dy += 2)
            {
                int ny = p.y + dy;
                if (ny < 0 || ny >= h) continue;

                int nrow = ny * w;
                bool inRun = false;
                for (int i = left; i <= right; i++)
                {
                    bool ok = !visited[nrow + i] && Matches(px[nrow + i], target);
                    if (ok && !inRun)
                    {
                        stack.Push(new Vector2Int(i, ny));
                        inRun = true;
                    }
                    else if (!ok)
                    {
                        inRun = false;
                    }
                }
            }
        }

        tex.SetPixels32(px);
        tex.Apply();

        // Another script may have swapped the quad's material or texture since setup.
        // Re-read what the renderer is using right now and make sure our texture is on it.
        Material current = useSharedMaterial ? quadRenderer.sharedMaterial : quadRenderer.material;
        if (current != mat)
        {
            if (debugLog)
                Debug.LogWarning($"PaintFill: the quad's renderer is now using a different material ('{current.name}') than the one set up earlier. Re-applying the texture to it.", this);
            mat = current;
            ApplyTextureToMaterial();
        }
        else if (texProp != null && mat.GetTexture(texProp) != tex)
        {
            ApplyTextureToMaterial();
        }

        if (debugLog)
        {
            Debug.Log($"PaintFill: filled {filled} of {w * h} pixels starting at ({x},{y}) with {fillColor}.", this);
            if (fillColor.a < 1f)
                Debug.LogWarning("PaintFill: fillColor alpha is below 1. On a transparent material this makes the fill invisible.", this);
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------
    static bool SameColor(Color32 a, Color32 b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    bool Matches(Color32 a, Color32 b)
    {
        return Mathf.Abs(a.r - b.r) <= tolerance &&
               Mathf.Abs(a.g - b.g) <= tolerance &&
               Mathf.Abs(a.b - b.b) <= tolerance &&
               Mathf.Abs(a.a - b.a) <= tolerance;
    }
}
}