using NetTopologySuite.Triangulate.QuadEdge;
using UnityEngine;

public class SizeMatcher : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Transform Transform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform SpriteTransform = sr.transform;
        Vector3 quadSize = Transform.localScale;
        Vector2 spriteSize = sr.sprite.bounds.size;

        SpriteTransform.localScale = new Vector3(
            quadSize.x / spriteSize.x,
            quadSize.y / spriteSize.y, 1f
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
