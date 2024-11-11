using UnityEngine.Rendering;

namespace SkinnedMeshDecals {

internal struct DecalCommand {
    public bool valid;
    public MonoBehaviourHider.DecalableRenderer decalableRenderer;
    public DecalProjector projector;
    public DecalProjection projection;
    public DecalSettings? decalSettings;

    public bool TryApply(CommandBuffer cmd) {
        valid = false;
        return decalableRenderer.TryApply(cmd, projector, projection, decalSettings ?? SkinnedMeshDecalsSettings.DefaultDecalSettings);
    }

    public void Invalidate() {
        valid = false;
    }
}

}