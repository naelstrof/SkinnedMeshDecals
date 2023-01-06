using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

public class DecalSphereSplat : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    private static Collider[] staticColliders = new Collider[32];
    void Start() {
        projector = Instantiate(projector);
    }
    void Update() {
        projector.color = color;
        float radius = transform.localScale.x*0.25f;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, radius*2f, staticColliders, hitMask);
        for (int i = 0; i < hits; i++) {
            if (staticColliders[i].TryGetComponent(out DecalableCollider decalableCollider)) {
                foreach (var decalableRenderer in decalableCollider.GetDecalableRenderers()) {
                    PaintDecal.RenderDecal(decalableRenderer, projector, transform.position-Vector3.forward*radius*2f,
                        Quaternion.identity, Vector2.one * (radius*2f), radius*4f);
                }
            }
        }
    }
}
