using SkinnedMeshDecals;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public LayerMask hitMask;
    public Color color;
    [Range(0f,5f)]
    public float size;
    void Update() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
                return;
            }
            foreach (var decalableRenderer in decalableCollider.GetDecalableRenderers()) {
                PaintDecal.RenderDecal(decalableRenderer, new DecalProjector(DecalProjectorType.TextureAlpha, color), new DecalProjection(hit.point, transform.forward, size));
            }
        }
    }
}
