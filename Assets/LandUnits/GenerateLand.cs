using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catfood.Shapefile;

public class GenerateLand : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        using (var shapefile = new Shapefile("gadm41_CHN_3.shp")){
            foreach(Shape s in shapefile){
                switch(s.Type){
                    case ShapeType.Polygon{
                        ShapePolygon polygon = (ShapePolygon)s;
                        break;
                    }
                    case ShapeType.Point{
                        ShapePoint point = (ShapePoint)s;
                        break;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
