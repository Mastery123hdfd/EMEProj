using NetTopologySuite.Triangulate.QuadEdge;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceUtils : MonoBehaviour
{
    [SerializeField] public Transform quad;
    [SerializeField] public SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr.transform.localScale = quad.localScale;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
