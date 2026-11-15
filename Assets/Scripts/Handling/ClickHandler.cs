using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cty.World;

namespace Cty.Click{
    public class ClickHandler
    {
        public void processColor(Color c){
            
            float id = c.r + 256 * c.g + 65536 * c.b;
            Debug.Log(id);
            County county = WorldHandler.counties[(int)id];
            Debug.Log(id+" County: " + county.name);
        }
    }
}
