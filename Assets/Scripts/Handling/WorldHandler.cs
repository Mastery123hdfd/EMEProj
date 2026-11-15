using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Cty.World;
using Cty.Click;

public class WorldHandler : MonoBehaviour
{
    static public Dictionary<string, Prefecture> prefectures = new Dictionary<string, Prefecture>();
    static public Dictionary<string, Province> provinces = new Dictionary<string, Province>();
    static public Dictionary<int, County> counties;
    // Start is called before the first frame update
    void Start()
    {
        
        TextAsset file = Resources.Load<TextAsset>("province_data");
        if(file == null){
            Debug.LogError("Resources/province_Data not found");
            return;
        }
        string json = file.text;
        counties = JsonConvert.DeserializeObject<Dictionary<int, County>>(json);
        foreach(var item in counties){
            County county = item.Value;
            county.id = item.Key;
            county.calcColors();

            if(!provinces.TryGetValue(county.ParentProvinceName, out Province province)){
                province = new Province(county.ParentProvinceName);
                provinces.Add(province.name, province);
            }
            string prefecturename = province.name + "/" + county.ParentPrefectureName;
            if(!prefectures.TryGetValue(prefecturename, out Prefecture prefecture)){
                prefecture = new Prefecture(county.ParentPrefectureName,province);
                prefectures.Add(prefecturename,prefecture);
                province.prefectures.Add(prefecture);
            }
            county.ParentProvince = province;
            county.ParentPrefecture = prefecture;
            prefecture.counties.Add(county);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
