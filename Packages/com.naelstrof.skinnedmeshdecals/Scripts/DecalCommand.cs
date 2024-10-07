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
        cmd.SetViewProjectionMatrices(projection.view, projection.projection);
        return decalableRenderer.TryApply(cmd, projector, decalSettings ?? SkinnedMeshDecalsSettings.DefaultDecalSettings);
    }
}

}