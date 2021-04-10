using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public Texture2D decal;
    public LayerMask hitMask;
    public Color color;
    [Range(0f,5f)]
    public float size;
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            Renderer r = hit.collider.transform.root.GetComponentInChildren<Renderer>();
            if (r != null) {
                PaintDecal.instance.RenderDecal(r, decal, hit.point + hit.normal * 0.25f, Quaternion.FromToRotation(Vector3.forward,-hit.normal), color, Vector2.one * size, 0.5f, r.lightmapIndex == -1?true:false);
            }
        }
    }
}
