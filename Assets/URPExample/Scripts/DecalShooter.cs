using SkinnedMeshDecals;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public LayerMask hitMask;
    public Color color;
    [Range(0f,5f)]
    public float size;
    void Update() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            PaintDecal.QueueDecal(hit.collider, new DecalProjector(DecalProjectorType.TextureAlpha, color, true), new DecalProjection(hit.point, transform.forward, size));
        }
    }
}
