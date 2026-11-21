using UnityEngine;
using Cty.World;

namespace Cty.Click{
    public class ClickHandler
    {
        // Raster layout (middle / low / high): R = middle byte, G = low byte, B = high byte.
        public static int DecodeId(Color32 c){
            return c.r | (c.g << 8) | (c.b << 16);
        }

        // Quiet lookup, used when sampling several pixels around the click.
        public bool TryGetCounty(Color32 c, out County county){
            county = null;
            if(WorldHandler.counties == null) return false;
            return WorldHandler.counties.TryGetValue(DecodeId(c), out county);
        }

        public County processColor(Color32 c){
            if(WorldHandler.counties == null){
                Debug.LogError("WorldHandler.counties isn't loaded yet");
                return null;
            }
            int id = DecodeId(c);
            if(!WorldHandler.counties.TryGetValue(id, out County county)){
                Debug.Log($"No county for color ({c.r},{c.g},{c.b}) -> id {id}");
                return null;
            }
            Debug.Log($"id {id} County: {county.name}");
            return county;
        }
    }
}
