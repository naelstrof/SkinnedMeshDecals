using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace SkinnedMeshDecals {
public class PaintDecal : MonoBehaviour {
    public static PaintDecal instance = null;
    private CommandBuffer commandBuffer;
    private void Awake() {
        if (instance == null) {
            //if not, set instance to this
            instance = this;
            commandBuffer = new CommandBuffer();
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
    }
    [Range(32,1024)]
    [Tooltip("Memory usage in megabytes before old textures get removed.")]
    public float memoryBudget = 512;
    private float memoryInUsage = 0f;
    [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
    public int texelsPerMeter = 256;
    private List<DecalableInfo> rendererCache = new List<DecalableInfo>();
    private WaitForSeconds waitForASec = new WaitForSeconds(8f);

    public IEnumerator CleanUpOccasionally() {
        while(true) {
            yield return waitForASec;
            while (memoryInUsage > memoryBudget && memoryInUsage > 0 && rendererCache.Count > 0) {
                RemoveOldest();
            }
        }
    }

    private void RemoveOldest() {
        DecalableInfo oldestInfo = null;
        float oldestTime = float.MaxValue;
        foreach( var info in rendererCache) {
            if (info.lastUse < oldestTime) {
                oldestInfo = info;
                oldestTime = info.lastUse;
            }
        }
        if (oldestInfo != null) {
            Destroy(oldestInfo);
        }
    }
    public void Start() {
        StartCoroutine(CleanUpOccasionally());
    }

    public static bool IsDecalable(Material m) {
        return m.HasProperty("_DecalColorMap");
    }
    public static void ClearRenderTexture(RenderTexture target) {
        var rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = target;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;
    }
    public void AddDecalableInfo(DecalableInfo info) {
        memoryInUsage += info.memorySizeMB;
        rendererCache.Add(info);
    }
    public void RemoveDecalableInfo(DecalableInfo info) {
        memoryInUsage -= info.memorySizeMB;
        rendererCache.Remove(info);
    }

    public void RenderDecal(Renderer r, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f) {
        DecalableInfo info = r.GetComponent<DecalableInfo>();
        if (info == null) {
            info = r.gameObject.AddComponent<DecalableInfo>();
        }
        if (!info.IsValid()) {
            return;
        }

        // Could use a Matrix4x4.Perspective instead! depends on use case.
        Matrix4x4 projection = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, 0f, depth);
        Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1)));

        commandBuffer.Clear();
        // Model matrix comes from CommandBuffer.DrawRenderer
        commandBuffer.SetViewProjectionMatrices(view, projection);
        info.Render(commandBuffer, projector);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }

    public void ClearDecalMaps() {
        while(rendererCache.Count > 0) {
            Destroy(rendererCache[0]);
        }
    }
    // This clears the decals and frees memory for the specified renderer.
    // If you wanted to "clean" renderers in a more believable way, you could draw decals in a subtractive mode on the renderer.
    public void ClearDecalsForRenderer(Renderer r) {
        DecalableInfo info = GetComponent<DecalableInfo>();
        if (info != null) {
            Destroy(info);
        }
    }
}

}