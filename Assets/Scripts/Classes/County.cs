using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Cty.World
{
    public class County{
        [JsonIgnore]private byte r;
        [JsonIgnore]private byte g;
        [JsonIgnore]private byte b;
        [JsonProperty("County")]
        public string name {get;}
        [JsonProperty("Type")]
        private string type {get; set;}


        [JsonIgnore]public int id {get; set;}

        [JsonIgnore]public Prefecture ParentPrefecture {get; set;}
        [JsonIgnore]public Province ParentProvince {get; set;}
        //LOADING PURPOSES ONLY
        [JsonProperty("District")]
        public string ParentPrefectureName {get; set;}
        [JsonProperty("Province")]
        public string ParentProvinceName {get; set;}

        public County(Prefecture ParentPrefecture, Province ParentProvince, string name, int id){
            this.ParentPrefecture=ParentPrefecture;
            this.ParentProvince = ParentProvince;
            this.id = id;
            this.name = name;
            this.calcColors();
        }
        public void calcColors(){
            this.r = (byte)((id >> 8) & 0xFF);
            this.g = (byte)(id & 0xFF);
            this.b = (byte)((id >> 16) & 0xFF);
        }

    }
    public class Prefecture{
        public List<County> counties {get;} = new List<County>();
        public string name {get;}
        public Province ParentProvince {get;}
        public Prefecture(string name, Province Parent){
            this.name = name;
            this.ParentProvince = Parent;
        }
    }
    public class Province{
        public string name {get;}
        public List<Prefecture> prefectures {get;} = new List<Prefecture>();
        public Province(string name){
            this.name = name;
        }
    }
}
