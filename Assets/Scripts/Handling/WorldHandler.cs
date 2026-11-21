using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Cty.World;

public class WorldHandler : MonoBehaviour
{
    static public Dictionary<string, Prefecture> prefectures = new Dictionary<string, Prefecture>();
    static public Dictionary<string, Province> provinces = new Dictionary<string, Province>();
    static public Dictionary<int, County> counties;
    static public bool logged = false;

    void Start()
    {
        // Already loaded (a second WorldHandler in the scene, or domain reload is disabled).
        if(logged) return;

        TextAsset file = Resources.Load<TextAsset>("province_data");
        if(file == null){
            Debug.LogError("Resources/province_data not found");
            return;
        }
        counties = JsonConvert.DeserializeObject<Dictionary<int, County>>(file.text);
        if(counties == null){
            Debug.LogError("province_data.json deserialized to null");
            return;
        }

        provinces.Clear();
        prefectures.Clear();
        int minId = int.MaxValue;
        int maxId = int.MinValue;
        int unnamed = 0;

        foreach(var item in counties){
            County county = item.Value;
            county.id = item.Key;
            county.calcColors();
            if(item.Key < minId) minId = item.Key;
            if(item.Key > maxId) maxId = item.Key;
            if(string.IsNullOrEmpty(county.name)) unnamed++;

            string provinceName = county.ParentProvinceName ?? "(none)";
            string prefectureName = county.ParentPrefectureName ?? "(none)";

            if(!provinces.TryGetValue(provinceName, out Province province)){
                province = new Province(provinceName);
                provinces.Add(province.name, province);
            }
            string prefecturekey = province.name + "/" + prefectureName;
            if(!prefectures.TryGetValue(prefecturekey, out Prefecture prefecture)){
                prefecture = new Prefecture(prefectureName, province);
                prefectures.Add(prefecturekey, prefecture);
                province.prefectures.Add(prefecture);
            }
            county.ParentProvince = province;
            county.ParentPrefecture = prefecture;
            prefecture.counties.Add(county);
        }
        logged = true;

        Debug.Log($"Completed JSON loading: {counties.Count} counties, ids {minId}..{maxId}, {unnamed} without a name");
        if(minId <= 0) Debug.LogWarning("A county has id 0 or lower. Id 0 is black in the raster, which is also the background.");
        if(maxId > 0xFFFFFF) Debug.LogWarning("A county id is above 16777215 and can't be stored in an RGB color.");
        if(unnamed == counties.Count) Debug.LogError("No county has a name, so the JSON isn't filling County.name. Use the County.cs with the parameterless constructor.");

        int shown = 0;
        foreach(var item in counties){
            Debug.Log($"  sample: id {item.Key} = {item.Value.name} | {item.Value.ParentPrefectureName} | {item.Value.ParentProvinceName}");
            if(++shown >= 3) break;
        }
    }
}
