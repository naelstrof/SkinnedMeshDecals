using System.Collections;
using System.Collections.Generic;
using System.Net;
using SkinnedMeshDecals;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    [Range(0f,5f)]
    public float size;
    void Start() {
        projector = Instantiate(projector);
    }
    void Update() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            projector.color = color;
            if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
                return;
            }
            foreach (var decalableRenderer in decalableCollider.GetDecalableRenderers()) {
                PaintDecal.RenderDecal(decalableRenderer, projector, hit.point-transform.forward*0.25f,
                    Quaternion.FromToRotation(Vector3.forward, transform.forward), Vector2.one * size, 0.6f);
            }
        }
    }
}
