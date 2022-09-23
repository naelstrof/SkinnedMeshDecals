using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSphereSplat : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    void Start() {
        projector = Material.Instantiate(projector);
    }
    void Update() {
        projector.color = color;
        SkinnedMeshDecals.PaintDecal.RenderDecalInSphere(transform.position, transform.localScale.x*0.5f, projector, transform.rotation, hitMask);
    }
}
