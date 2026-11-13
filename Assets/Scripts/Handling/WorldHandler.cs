using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class WorldHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TextAsset file = Resources.Load<TextAsset>("province_data");
        string json = file.text;
        Dictionary<int, County> counties = JsonConvert.DeserializeObject<Dictionary<int, County>>(json);
        foreach(var item in counties){
            item.Value.id = item.Key;
            if(!provinces.ContainsKey(item.Value.ParentProvinceName)){
                provincences.Add(item.Value.ParentProvinceName, new Province(item.Value.ParentProvinceName));
            }
            if(!prefectures.ContainsKey(item.Value.ParentPrefectureName)){
                prefectures.Add(item.Value.ParentPrefectureName, new Prefecture(item.Value.ParentPrefectureName, provinces[item.Value.ParentProvinceName]));
            }
            item.Value.calcColors();
        }
        Dictionary<string, Prefecture> prefectures = new Dictionary<string, Prefecture>();
        Dictionary<string, Province> provinces = new Dictionary<string, Province>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
