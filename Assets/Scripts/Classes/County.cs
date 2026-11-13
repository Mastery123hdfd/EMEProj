using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cty.World
{
    public class County{
        private byte r;
        private byte g;
        private byte b;
        [JsonProperty("County")]
        private string name {get; set;};
        [JsonProperty("Type")]
        private string type {get; set;}


        public int id {get; set;}

        private Prefecture ParentPrefecture {get; set;}
        private Province ParentProvince {get; set;}
        //LOADING PURPOSES ONLY
        [JsonProperty("District")]
        private string ParentPrefectureName;
        [JsonProperty("Province")]
        private string ParentProvinceName;

        public County(Prefecture ParentPrefecture, Province ParentProvince, string name, int id){
            this.ParentPrefecture=ParentPrefecture;
            this.ParentProvince = ParentProvince;
            this.id = id;
            this.name = name;
            this.calcColors();
        }
        public void calcColors(){
            this.r = id / 256;
            this.g = id / 65536;
            this.b = id % 65536;
        }

    }
    public class Prefecture{
        public List<County> counties {get; set;}
        private string name;
        private Province ParentProvince;
        public Province(string name, Province Parent){
            this.name = name;
            this.ParentProvince = Parent;
        }
    }
    public class Province{
        private string name;
        public List<Prefecture> prefectures {get; set;}
        public Prefecture(string name){
            this.name = name;
        }
    }
}
