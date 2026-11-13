using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private float camangle;
    private float targetHeight;
    private Plane map;
    private Vector3 dragOrigin;
    private int timeCounter;
    private float camx;
    private float camy;
    private bool up;
    private bool right;
    private bool down;
    private bool left;
    void Start(){
        map = new Plane(Vector3.up, Vector3.zero);
        targetHeight = maxH;
        camangle = Camera.main.fieldOfView * Mathf.Deg2Rad /2f;
    }
    void Update(){
        Vector3 pos = transform.position;
        camy = Mathf.Tan(camangle) * pos.y;
        camx = camy * Camera.main.aspect;
        left = Mathf.Abs(pos.x -camx) > maxX;
        right = Mathf.Abs(pos.x + camx) > maxX;
        up = Mathf.Abs(pos.z+camy) > maxY;
        down = Mathf.Abs(pos.z-camy) > maxY;
        if(timeCounter >= 0){
            timeCounter -=1;
        }
        float scroll = Mouse.current.scroll.ReadValue().y;
        targetHeight -= scroll *zoomSensitivity;
        targetHeight = Mathf.Clamp(targetHeight, minH, maxH);
        
        pos.y = Mathf.Lerp(pos.y,targetHeight,12f*Time.deltaTime);
        transform.position = pos;
        if(Mouse.current.leftButton.wasPressedThisFrame){
            dragOrigin = getClickPoint();
            if(timeCounter == 0){
                timeCounter = 25;
            } else {

            }
        }
        if(Mouse.current.leftButton.isPressed){
            Vector3 current = getClickPoint();
            Vector3 dragMove = dragOrigin-current;
            if(right && dragMove.x > 0){dragMove.x = 0;}
            if(left && dragMove.x < 0) dragMove.x = 0;
            if(up && dragMove.z > 0) dragMove.z = 0;
            if(down && dragMove.z < 0) dragMove.z = 0;
            transform.position += dragMove*dragSensitivity;
            dragOrigin = current;
        }
        Debug.Log("Left: " + left+" Right: " + right+" Up: " + up+" Down: " + down);
        Vector3 move = Vector3.zero;
        Vector3 mouse = Mouse.current.position.ReadValue();
        if(mouse.x < edgeSize && !down) move.x-=1;
        if(mouse.x > Screen.width-edgeSize && !right) move.x+=1;
        if(mouse.y < edgeSize && !left) move.z-=1;
        if(mouse.y > Screen.height-edgeSize && !up) move.z+=1;
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
}
