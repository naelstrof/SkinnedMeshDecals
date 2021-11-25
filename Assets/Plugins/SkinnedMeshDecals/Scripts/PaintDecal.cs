using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace SkinnedMeshDecals {
public static class PaintDecal {
    private const string defaultTextureName = "_DecalColorMap";
    private static CommandBuffer commandBuffer = new CommandBuffer();
    [Range(32,1024)]
    [Tooltip("Memory usage in megabytes before old textures get removed.")]
    public static float memoryBudget = 512;
    private static float memoryInUsage = 0f;
    [Range(32,4096)]
    [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
    public static int texelsPerMeter = 512;
    private static List<DecalableInfo> rendererCache = new List<DecalableInfo>();
    /*public IEnumerator CleanUpOccasionally() {
        while(true) {
            yield return waitForASec;
            while (memoryInUsage > memoryBudget && memoryInUsage > 0 && rendererCache.Count > 0) {
                RemoveOldest();
            }
        }
    }*/
    private static void RemoveOldest() {
        DecalableInfo oldestInfo = null;
        float oldestTime = float.MaxValue;
        foreach( var info in rendererCache) {
            if (info.lastUse < oldestTime) {
                oldestInfo = info;
                oldestTime = info.lastUse;
            }
        }
        if (oldestInfo != null) {
            GameObject.Destroy(oldestInfo);
        }
    }
    public static bool IsDecalable(Material m, string textureTarget = defaultTextureName) {
        return m.HasProperty(textureTarget);
    }
    public static void AddDecalableInfo(DecalableInfo info) {
        memoryInUsage += info.memorySizeMB;
        rendererCache.Add(info);
        while (memoryInUsage>memoryBudget) {
            RemoveOldest();
        }
    }
    public static void RemoveDecalableInfo(DecalableInfo info) {
        memoryInUsage -= info.memorySizeMB;
        rendererCache.Remove(info);
    }

    public static void RenderDecal(Renderer r, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f, string textureName = defaultTextureName) {
        DecalableInfo info = r.GetComponent<DecalableInfo>();
        if (info == null) {
            info = r.gameObject.AddComponent<DecalableInfo>();
        }
        // Could use a Matrix4x4.Perspective instead! depends on use case.
        Matrix4x4 projection = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, 0f, depth);
        Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1)));
        commandBuffer.Clear();
        // Model matrix comes from CommandBuffer.DrawRenderer
        commandBuffer.SetViewProjectionMatrices(view, projection);
        info.Render(commandBuffer, projector, textureName);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }

    public static void ClearDecalMaps() {
        while(rendererCache.Count > 0) {
            GameObject.Destroy(rendererCache[0]);
        }
    }
    // This clears the decals and frees memory for the specified renderer.
    // If you wanted to "clean" renderers in a more believable way, you could draw decals in a subtractive mode on the renderer.
    public static void ClearDecalsForRenderer(Renderer r) {
        DecalableInfo info = r.GetComponent<DecalableInfo>();
        if (info != null) {
            GameObject.Destroy(info);
        }
    }
}

}