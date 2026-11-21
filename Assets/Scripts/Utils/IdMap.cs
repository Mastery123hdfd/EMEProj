using System;
using UnityEngine;

namespace Cty.World{
    // Run-length county id map written by make_idmap.py. It holds county ids directly (no colors),
    // so there is no texture import, no compression and no color-space conversion involved.
    // Row 0 is the BOTTOM row, matching Texture2D and RaycastHit.textureCoord.
    //
    // Layout (little-endian int32): magic 'IMD1' | width | height | rowStart[height + 1] | (startX, id) per run.
    public class IdMap{
        private const int Magic = 0x31444D49; // bytes 'I','M','D','1'

        public int Width { get; private set; }
        public int Height { get; private set; }
        private int[] data;
        private int runsBase;

        public static IdMap Load(string resourceName){
            TextAsset file = Resources.Load<TextAsset>(resourceName);
            if(file == null){
                Debug.LogError($"Resources/{resourceName}.bytes not found. Run make_idmap.py and put idmap.bytes in Assets/Resources.");
                return null;
            }
            byte[] bytes = file.bytes;
            Resources.UnloadAsset(file);

            if(bytes.Length < 12 || bytes.Length % 4 != 0){
                Debug.LogError($"{resourceName}.bytes is {bytes.Length} bytes, which isn't a valid id map");
                return null;
            }
            int[] ints = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);

            if(ints[0] != Magic){
                Debug.LogError($"{resourceName}.bytes isn't an id map written by make_idmap.py (bad header)");
                return null;
            }
            int width = ints[1];
            int height = ints[2];
            int rowsEnd = 3 + height + 1;
            if(width <= 0 || height <= 0 || ints.Length < rowsEnd){
                Debug.LogError($"{resourceName}.bytes has a bad size header ({width}x{height})");
                return null;
            }
            int runCount = ints[3 + height];
            if(ints.Length != rowsEnd + runCount * 2){
                Debug.LogError($"{resourceName}.bytes is truncated or corrupt (expected {rowsEnd + runCount * 2} ints, found {ints.Length}). Was it copied as text?");
                return null;
            }

            IdMap map = new IdMap();
            map.Width = width;
            map.Height = height;
            map.data = ints;
            map.runsBase = rowsEnd;
            Debug.Log($"IdMap loaded: {width}x{height}, {runCount} runs, {bytes.Length / 1048576f:F1} MB");
            return map;
        }

        // County id at pixel (x, y), or 0 (background) outside the map.
        public int GetId(int x, int y){
            if(x < 0 || y < 0 || x >= Width || y >= Height) return 0;
            int lo = data[3 + y];          // first run of this row
            int hi = data[4 + y] - 1;      // last run of this row
            if(lo > hi) return 0;
            // Last run whose startX <= x.
            while(lo < hi){
                int mid = (lo + hi + 1) >> 1;
                if(data[runsBase + mid * 2] <= x) lo = mid;
                else hi = mid - 1;
            }
            return data[runsBase + lo * 2 + 1];
        }
    }
}
