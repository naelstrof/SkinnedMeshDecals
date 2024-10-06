using UnityEngine.Rendering;

namespace SkinnedMeshDecals {

internal struct DecalCommand {
    public int age;
    public bool valid;
    public MonoBehaviourHider.DecalableRenderer decalableRenderer;
    public DecalProjector projector;
    public DecalProjection projection;
    public DecalSettings? decalSettings;

    public bool TryApply(CommandBuffer cmd) {
        age++;
        try {
            cmd.SetViewProjectionMatrices(projection.view, projection.projection);
            return decalableRenderer.TryApply(cmd, projector, decalSettings ?? PaintDecal.GetSkinnedMeshDecalSettings().defaultDecalSettings);
        } catch {
            valid = false;
            throw;
        }
    }
}

}