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
        projector.color = color;
    }
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            Renderer r = hit.collider.transform.root.GetComponentInChildren<Renderer>();
            if (r != null) {
                SkinnedMeshDecals.PaintDecal.RenderDecal(r, projector, hit.point + hit.normal * 0.15f, Quaternion.FromToRotation(Vector3.forward,-hit.normal), Vector2.one * size, 0.3f);
            }
        }
    }
}
