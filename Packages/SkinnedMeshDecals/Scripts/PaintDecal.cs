using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace SkinnedMeshDecals {
public static class PaintDecal {
    private static Collider[] colliders = new Collider[32];
    private static List<Renderer> staticRenderers = new List<Renderer>();
    private static List<Renderer> staticTempRenderers = new List<Renderer>();
    private static void GetComponentsInChildrenNoAlloc<T>(this Transform t, List<T> temp, List<T> result) {
        t.GetComponents<T>(temp);
        result.AddRange(temp);
        for(int i=0;i<t.childCount;i++) {
            GetComponentsInChildrenNoAlloc<T>(t.GetChild(i), temp, result);
		}
	}
    private const string defaultTextureName = "_DecalColorMap";
    private static CommandBuffer commandBuffer = new CommandBuffer();
    // TODO: Probably should be in graphics settings somewhere for defaults.
    // Maybe generate a scriptable object asset for settings.
    // TODO: Actually should be a PaintDecalConfiguration monobehavior singleton.
    [Range(32,2048)]
    [Tooltip("Memory usage in megabytes before old textures get removed.")]
    public static float memoryBudget = 512;
    public static float memoryInUsage {
        get {
            float memoryInUse = 0f;
            foreach(var info in rendererCache) {
                memoryInUse += info.memorySizeMB;
            }
            return memoryInUse;
        }
    }

    [Range(32,4096)]
    [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
    public static int texelsPerMeter = 512;
    private static List<MonobehaviourHider.DecalableInfo> rendererCache = new List<MonobehaviourHider.DecalableInfo>();
    private static void RemoveOldest() {
        MonobehaviourHider.DecalableInfo oldestInfo = null;
        float oldestTime = float.MaxValue;
        foreach( var info in rendererCache) {
            if (info.lastUse < oldestTime) {
                oldestInfo = info;
                oldestTime = info.lastUse;
            }
        }
        if (oldestInfo != null) {
            RemoveDecalableInfo(oldestInfo);
            GameObject.Destroy(oldestInfo);
        }
    }
    public static bool IsDecalable(Material m, string textureTarget = defaultTextureName) {
        return m.HasProperty(textureTarget);
    }
    public static void OnMemoryChanged() {
        while (memoryInUsage>memoryBudget) {
            RemoveOldest();
        }
    }
    public static void AddDecalableInfo(MonobehaviourHider.DecalableInfo info) {
        rendererCache.Add(info);
    }
    public static void RemoveDecalableInfo(MonobehaviourHider.DecalableInfo info) {
        rendererCache.Remove(info);
    }

    public static void RenderDecalForCollider(Collider c, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f, string textureName = defaultTextureName) {
        LODGroup group = c.GetComponentInParent<LODGroup>();
        if (group != null) {
            staticRenderers.Clear();
            group.transform.GetComponentsInChildrenNoAlloc<Renderer>(staticTempRenderers, staticRenderers);
            foreach(Renderer renderer in staticRenderers) {
                SkinnedMeshDecals.PaintDecal.RenderDecal(renderer, projector, position, rotation, size, depth, textureName);
            }
            return;
        }
        Renderer parentRenderer = c.GetComponentInParent<Renderer>();
        if (parentRenderer != null) {
            SkinnedMeshDecals.PaintDecal.RenderDecal(parentRenderer, projector, position, rotation, size, depth, textureName);
        }

        staticRenderers.Clear();
        c.transform.GetComponentsInChildrenNoAlloc<Renderer>(staticTempRenderers, staticRenderers);
        foreach(Renderer renderer in staticRenderers) {
            SkinnedMeshDecals.PaintDecal.RenderDecal(renderer, projector, position, rotation, size, depth, textureName);
        }
    }
    public static void RenderDecalInSphere(Vector3 position, float radius, Material projector, Quaternion rotation, LayerMask hitMask, string textureName = defaultTextureName) {
        int hits = Physics.OverlapSphereNonAlloc(position, radius, colliders, hitMask, QueryTriggerInteraction.UseGlobal);
        for(int i=0;i<hits;i++) {
            Collider c = colliders[i];
            SkinnedMeshDecals.PaintDecal.RenderDecalForCollider(c, projector, position-rotation*Vector3.forward*radius, rotation, Vector2.one*radius, radius*2f, textureName);
        }
    }
    public static void RenderDecalInBox(Vector3 boxHalfExtents, Vector3 position, Material projector, Quaternion boxOrientation, LayerMask hitMask, string textureName = defaultTextureName) {
        int hits = Physics.OverlapBoxNonAlloc(position, boxHalfExtents, colliders, boxOrientation, hitMask, QueryTriggerInteraction.UseGlobal);
        for(int i=0;i<hits;i++) {
            Collider c = colliders[i];
            SkinnedMeshDecals.PaintDecal.RenderDecalForCollider(c, projector, position-boxOrientation*Vector3.forward*boxHalfExtents.z, boxOrientation, new Vector2(boxHalfExtents.x, boxHalfExtents.y)*2f, boxHalfExtents.z*2f, textureName);
        }
    }
    public static void RenderDecalForCollision(Collider c, Material projector, Vector3 position, Vector3 normal, float rotationAboutNormal, Vector2 size, float halfDepth = 0.5f, string textureName = defaultTextureName) {
        SkinnedMeshDecals.PaintDecal.RenderDecalForCollider(c, projector, position+normal*halfDepth, Quaternion.AngleAxis(rotationAboutNormal, normal)*Quaternion.FromToRotation(Vector3.forward, -normal), size, halfDepth*2f, textureName);
    }
    public static void RenderDecal(Renderer r, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f, string textureName = defaultTextureName) {
        // Only can draw on meshes.
        if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
            return;
        }
        MonobehaviourHider.DecalableInfo info = r.GetComponent<MonobehaviourHider.DecalableInfo>();
        if (info == null) {
            info = r.gameObject.AddComponent<MonobehaviourHider.DecalableInfo>();
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
        MonobehaviourHider.DecalableInfo info = r.GetComponent<MonobehaviourHider.DecalableInfo>();
        if (info != null) {
            GameObject.Destroy(info);
        }
    }
}

}