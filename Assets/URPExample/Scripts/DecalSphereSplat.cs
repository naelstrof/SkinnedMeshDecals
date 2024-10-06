using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

public class DecalSphereSplat : MonoBehaviour {
    public LayerMask hitMask;
    public bool subtract;
    public Color color;
    private static Collider[] staticColliders = new Collider[32];
    void Update() {
        float radius = transform.localScale.x*0.5f;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, radius*2f, staticColliders, hitMask);
        for (int i = 0; i < hits; i++) {
            PaintDecal.QueueDecal(staticColliders[i], new DecalProjector(subtract ? DecalProjectorType.SphereSubtractive : DecalProjectorType.SphereAlpha, color), new DecalProjection(transform.position, radius));
        }
    }
}
