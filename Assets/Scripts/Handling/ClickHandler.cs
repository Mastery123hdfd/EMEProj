using UnityEngine;
using Cty.World;

namespace Cty.Click{
    public class ClickHandler
    {
        public County processColor(Color32 c){
            if(WorldHandler.counties == null){
                Debug.LogError("WorldHandler.counties isn't loaded yet");
                return null;
            }
            int id = c.g | (c.r << 8) | (c.b << 16);
            if(!WorldHandler.counties.TryGetValue(id, out County county)){
                Debug.Log($"No county for color ({c.r},{c.g},{c.b}) -> id {id}");
                return null;
            }
            Debug.Log($"id {id} County: {county.name}");
            return county;
        }
    }
}