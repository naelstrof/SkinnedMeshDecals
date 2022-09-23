using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    [Range(0f,5f)]
    public float size;
    void Start() {
        projector = Material.Instantiate(projector);
    }
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            projector.color = color;
            SkinnedMeshDecals.PaintDecal.RenderDecalForCollision(hit.collider, projector, hit.point, hit.normal, UnityEngine.Random.Range(0f,360f), Vector2.one * size, 0.6f);
        }
    }
}
