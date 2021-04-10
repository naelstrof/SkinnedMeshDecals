using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

public class PaintDecal : MonoBehaviour {
    public static PaintDecal instance = null;
    private void Awake() {
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
    }
    private class PackedRenderer {
        private int CeilPowerOfTwo(int v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }
        public PackedRenderer(Renderer r, List<Material> mats, float texelsPerMeter) {
            int worldScale = Mathf.RoundToInt(r.bounds.extents.magnitude*texelsPerMeter);
            int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldScale), 16, 4096);
            texture = new RenderTexture(textureScale, textureScale, 0);
            ClearRenderTexture(texture);
            decalableMaterials = mats;
            lastUse = Time.time;
        }
        public RenderTexture texture;
        public List<Material> decalableMaterials;
        public float lastUse;
    }
    [Tooltip("The material used to actually project decals onto meshes. This generally should be the one included-- though custom ones with custom blend modes could be used instead.")]
    public Material decalProjector;
    [Tooltip("Same as the decal projector, but this time with a subtractive mode enabled.")]
    public Material subtractiveProjector;

    [Range(32,1024)]
    [Tooltip("Memory usage in megabytes before old textures get removed.")]
    public float memoryBudget = 512;
    private float memoryInUsage = 0f;
    [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
    public int texelsPerMeter = 256;
    private Dictionary<Renderer, PackedRenderer> rendererCache = new Dictionary<Renderer, PackedRenderer>();
    private WaitForSeconds waitForASec = new WaitForSeconds(8f);

    public IEnumerator CleanUpOccasionally() {
        while(true) {
            yield return waitForASec;
            Validate();
            while (memoryInUsage > memoryBudget && memoryInUsage > 0 && rendererCache.Count > 0) {
                RemoveOldest();
            }
        }
    }
    private void RemoveOldest() {
        Renderer oldestRenderer = null;
        foreach (var pair in rendererCache) {
            oldestRenderer = pair.Key;
            break;
        }

        float oldestTime = float.MaxValue;
        foreach( var pair in rendererCache) {
            if (pair.Value.lastUse < oldestTime) {
                oldestRenderer = pair.Key;
                oldestTime = pair.Value.lastUse;
            }
        }
        if (oldestRenderer != null) {
            foreach(Material m in rendererCache[oldestRenderer].decalableMaterials) {
                m.SetTexture("_DecalColorMap", null);
            }
            RenderTexture t = rendererCache[oldestRenderer].texture;
            memoryInUsage -= (t.width * t.height * 4f) / (float)(1e+6f);
            t.Release();
            rendererCache.Remove(oldestRenderer);
        }
    }
    private void Validate() {
        List<Renderer> removeList = new List<Renderer>();
        foreach( var pair in rendererCache) {
            if (pair.Key == null) {
                removeList.Add(pair.Key);
                // Don't need to unset materials, since the renderer was destroyed!
                pair.Value.texture.Release();
                memoryInUsage -= (pair.Value.texture.width*pair.Value.texture.height*4f)/(float)(1e+6f);
            }
        }
        foreach(var r in removeList) {
            rendererCache.Remove(r);
        }
    }

    public void Start() {
        StartCoroutine(CleanUpOccasionally());
    }

    public bool IsDecalable(Material m) {
        foreach(string s in m.GetTexturePropertyNames()) {
            if (s == "_DecalColorMap") {
                return true;
            }
        }
        return false;
    }

    public List<Material> GetDecalableMaterials(Renderer r) {
        List<Material> decalableMaterials = new List<Material>();
        foreach (Material m in r.materials) {
            if (IsDecalable(m)) {
                decalableMaterials.Add(m);
            }
        }
        return decalableMaterials;
    }
    private static void ClearRenderTexture(RenderTexture target) {
        var rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = target;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;
    }

    // Check if we're a valid renderer to render a decal too.
    // If we have any material with a _DecalColorMap texture variable
    // We try to use the render target that exists there, if not we create one.
    // We also try to use dedicated DecalMaps for lightmapped objects.
    private PackedRenderer GetPackedRenderer(Renderer r) {
        // we've already cached this material, return it
        if (rendererCache.ContainsKey(r)) {
            rendererCache[r].lastUse = Time.time;
            return rendererCache[r];
        }

        List<Material> mats;
        mats = GetDecalableMaterials(r);
        if (mats.Count <= 0 ) {
            return null;
        }
        // FIXME: On mac, there's a chance this texture is not null, but also uninitialized. Need to know when to clear in those cases!
        RenderTexture target = (RenderTexture)mats[0].GetTexture("_DecalColorMap");
        if (target == null || !target.IsCreated()) {
            // Gotta find a texture location to place it on.
            rendererCache.Add(r, new PackedRenderer(r, mats, texelsPerMeter));
            target = rendererCache[r].texture;
            memoryInUsage += (target.width*target.height*4f)/(float)(1e+6f);
            foreach (Material m in mats) {
                m.SetTexture("_DecalColorMap", target);
            }
            return rendererCache[r];
        }
        return null;
    }

    public RenderTexture RenderDecal(Renderer r, Texture decal, Vector3 position, Quaternion rotation, Color color, Vector2 size, float depth = 0.5f, bool addPadding = true, bool cullBack = true, bool subtract = false) {
        RenderTexture target = null;
        PackedRenderer packed = GetPackedRenderer(r);
        if (packed == null) {
            return null;
        }
        target = packed.texture;
        Material projector = subtract ? subtractiveProjector : decalProjector;

        // With a valid target, generate a material list with the decal projector on the right submesh, with all other submeshes set to an invisible material.
        projector.SetTexture("_Decal", decal);
        if (addPadding) {
            color.a *= 0.25f;
        }
        projector.SetColor("_BaseColor", color);

        if (cullBack) {
            projector.EnableKeyword("_BACKFACECULLING");
        } else {
            projector.DisableKeyword("_BACKFACECULLING");
        }

        Matrix4x4 projection = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, 0f, depth);
        Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1)));

        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(target);
        // Model matrix comes from CommandBuffer.DrawRenderer
        buffer.SetViewProjectionMatrices(view, projection);
        Vector2 pixelRect = new Vector2(target.width, target.height);
        int maxIndex = 1;
        MeshFilter f = r.GetComponent<MeshFilter>();
        if (f != null) {
            maxIndex = f.sharedMesh.subMeshCount;
        }
        if (r is SkinnedMeshRenderer) {
            SkinnedMeshRenderer sr = (SkinnedMeshRenderer)r;
            maxIndex = sr.sharedMesh.subMeshCount;
        }
        buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
        for(int i=0;i<maxIndex;i++) {
            // There could be other materials on this renderer that aren't related to decals (usually eyeballs on characters)
            // We need to make sure we skip them.
            if (r.materials.Length == 0 || r.materials.Length <= i || !IsDecalable(r.materials[i])) {
                continue;
            }
            // For padding we just render the same thing repeatedly with diagonal offsets.
            // This doesn't look good on lightmapped geo, since a one pixel offset can send it drastically off-target,
            // so we only render to dynamic meshes.
            if (addPadding) {
                Vector2 pixelSize = new Vector2(1f,1f);
                Vector2 pixelSizeAlso = new Vector2(-1f,1f);
                buffer.SetViewport(new Rect(pixelSize, pixelRect + pixelSize));
                buffer.DrawRenderer(r, projector, i);
                buffer.SetViewport(new Rect(-pixelSize, pixelRect - pixelSize));
                buffer.DrawRenderer(r, projector, i);
                buffer.SetViewport(new Rect(pixelSizeAlso, pixelRect + pixelSizeAlso));
                buffer.DrawRenderer(r, projector, i);
                buffer.SetViewport(new Rect(-pixelSizeAlso, pixelRect - pixelSizeAlso));
                buffer.DrawRenderer(r, projector, i);
            } else {
                buffer.DrawRenderer(r, projector, i);
            }
        }
        Graphics.ExecuteCommandBuffer(buffer);
        return target;
    }

    public void ClearDecalMaps() {
        foreach(var pair in rendererCache) {
            foreach (var material in pair.Value.decalableMaterials) {
                material.SetTexture("_DecalColorMap", null);
            }
            pair.Value.texture.Release();
        }
        rendererCache.Clear();
        memoryInUsage = 0f;
    }
}


