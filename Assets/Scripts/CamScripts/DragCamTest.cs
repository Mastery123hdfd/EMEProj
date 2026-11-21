using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragCamTest : MonoBehaviour
{
    private DragCamera dc;
    [SerializeField] public bool clickLocation;
    [SerializeField] public bool doubleClickDetect;
    [SerializeField] public int doubleClickLen;
    private int time;
    // Start is called before the first frame update
    void Start()
    {
        dc = GetComponent<DragCamera>();
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(time>0){time--;}
        if(Mouse.current.leftButton.wasPressedThisFrame && clickLocation){
            Debug.Log("Pressed");
            Debug.Log("Coords:" + dc.getClickPoint());
        }
        if(Mouse.current.leftButton.wasPressedThisFrame){
            time = doubleClickLen;
        }
        if(Mouse.current.leftButton.wasPressedThisFrame && time > 0 && doubleClickDetect){
            Debug.Log("Double Click Detected");
        }
        
    }
}
