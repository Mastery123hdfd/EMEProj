using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragCamTest : MonoBehaviour
{
    private DragCamera dc;
    // Start is called before the first frame update
    void Start()
    {
        dc = GetComponent<DragCamera>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.leftButton.isPressed){
            Debug.Log("Pressed");
            Debug.Log("Coords:" + dc.getClickPoint());
        }
    }
}
