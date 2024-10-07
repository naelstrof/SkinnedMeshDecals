using System.Collections.Generic;
using UnityEngine;

namespace SkinnedMeshDecals {
public class DecalableCollider : MonoBehaviour {
    [SerializeField]
    private List<Renderer> renderers;

    private bool initialized => decalableRenderers != null;
    private HashSet<MonoBehaviourHider.DecalableRenderer> decalableRenderers;

    void Initialize() {
        if (initialized) {
            return;
        }
        decalableRenderers = new HashSet<MonoBehaviourHider.DecalableRenderer>();
        foreach (var r in renderers) {
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer decalableRenderer)) {
                decalableRenderer = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
            }
            decalableRenderers.Add(decalableRenderer);
        }
    }
    public void QueueDecal(DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null, PaintDecal.QueueType queueType = PaintDecal.QueueType.Deferred) {
        Initialize();
        foreach (var decalableRenderer in decalableRenderers) {
            PaintDecal.QueueDecal(decalableRenderer, projector, projection, decalSettings, queueType);
        }
    }
}

}
