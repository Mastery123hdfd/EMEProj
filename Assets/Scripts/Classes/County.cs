using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cty.World
{
    public class County{
        private byte r;
        private byte g;
        private byte b;
        private float x;
        private float y;
        private string name;
        public int id;
        private Prefecture ParentPrefecture;
        private Province ParentProvince;
    }
    public class Prefecture{
        private County[] counties;
        private float x;
        private float y;
        public string name;
        public Province ParentProvince;
    }
    public class Province{
        public string name;
        private float x;
        private float y;
        private Prefecture[] prefectures;
    }
}
