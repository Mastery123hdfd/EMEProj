using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cty.DoubleClick{
    // Runs before other scripts so DragCamera sees this frame's double click in its own Update.
    [DefaultExecutionOrder(-100)]
    public class DoubleClickHandler : MonoBehaviour
    {
        [SerializeField] public bool doubleClickDetect = true;
        // Max seconds between the two presses. 0.5 is the Windows default double-click time.
        // (Renamed from doubleClickTime so this default replaces the 0.3 saved in your scene.)
        [SerializeField] private float doubleClickWindow = 0.5f;
        // Max screen pixels the mouse may drift between the two presses.
        [SerializeField] private float maxDriftPixels = 12f;
        // Logs why each press was or wasn't counted.
        [SerializeField] private bool debugLog = true;

        private float lastClickTime;
        private Vector2 lastClickPos;
        private bool hasLastClick;

        // Counts double clicks. Anything that wants to react can compare against its last seen value.
        public static int pulse { get; private set; }
        // Fired once per double click with the screen position of the second press.
        public static event Action<Vector2> DoubleClicked;

        void Start(){
            pulse = 0;
        }

        void Update()
        {
            Mouse mouse = Mouse.current;
            if(mouse == null || !doubleClickDetect) return;
            if(!mouse.leftButton.wasPressedThisFrame) return;

            Vector2 pos = mouse.position.ReadValue();
            float now = Time.unscaledTime;
            float gap = now - lastClickTime;
            float drift = (pos - lastClickPos).magnitude;

            // Compare against the PREVIOUS press before recording this one.
            bool isSecondClick = hasLastClick && gap <= doubleClickWindow && drift <= maxDriftPixels;

            if(isSecondClick){
                // Consume it, so a third quick click starts a new pair instead of counting again.
                hasLastClick = false;
                pulse++;
                Debug.Log($"Double Click Detected (gap {gap:F2}s, drift {drift:F0}px)");
                DoubleClicked?.Invoke(pos);
            } else {
                if(debugLog){
                    if(hasLastClick){
                        Debug.Log($"Not a double click: gap {gap:F2}s (max {doubleClickWindow:F2}s), drift {drift:F0}px (max {maxDriftPixels:F0}px)");
                    } else {
                        Debug.Log("First click recorded");
                    }
                }
                hasLastClick = true;
                lastClickTime = now;
                lastClickPos = pos;
            }
        }
    }
}
