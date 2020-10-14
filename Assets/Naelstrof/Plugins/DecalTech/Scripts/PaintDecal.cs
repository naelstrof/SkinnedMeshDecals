using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
    private class CachedRenderTexture {
        public RenderTexture texture;
        public float timeCreated;
        public float probableAlpha = 1f;
    }
    public Camera decalCamera;
    [Tooltip("Material with a shader that projects decals on skinned meshes. Uses _BaseColorMap and _BaseColor to set the texture and color respectively.")]
    public Material decalProjector;
    [Tooltip("Material used to fade existing decals away (By blending a low-transparency texture over existing framebuffers). I use a AlphaBlendOp of Subtract with the AlphaBlend set to One One.")]
    public Material alphaBlend;

    [Tooltip("Resolution for textures generated on skinned mesh renderers.")]
    public int renderTextureResolution = 1024;

    [Tooltip("Resolution for world-decals (Expensive!), makes one-per-lightmap atlas. Set to 0 if you don't want map decals.")]
    public int decalMapTextureResolution = 2048;
    [Tooltip("Maximum number of allowed render textures for dynamic objects.")]
    public int maxRenderTextures = 16;
    [Tooltip("Number of seconds before dynamic textures are completely transparent.")]
    public float fadeTime = 60f;
    private Dictionary<Renderer, CachedRenderTexture> dynamicRenderTextureCache = new Dictionary<Renderer, CachedRenderTexture>();
    private HashSet<Renderer> staticRenderables = new HashSet<Renderer>();
    private RenderTexture transparentTex;
    private List<RenderTexture> decalMaps = new List<RenderTexture>();
    // Debug stuff
    //public GameObject debugPlane;
    //public Material shader;
    //public Transform debugLocation;
    //private List<GameObject> debugPlanes = new List<GameObject>();

    public void Start() {
        RegenerateDecalMaps();
        transparentTex = new RenderTexture(16, 16, 0);
        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(transparentTex);
        buffer.ClearRenderTarget(false, true, new Color(0, 0, 0, 1f/256f));
        Graphics.ExecuteCommandBuffer(buffer);
        StartCoroutine(FadeOutDynamicTextures(fadeTime/256f));
        SceneManager.sceneLoaded += RegenerateDecalMaps;
    }

    /*void CreateDebugPlanes() {
        foreach( GameObject p in debugPlanes) {
            Destroy(p);
        }
        debugPlanes.Clear();
        for (int i=0;i<decalMaps.Count;i++) {
            GameObject plane = GameObject.Instantiate(debugPlane);
            Material m = Material.Instantiate(shader);
            m.SetTexture("_BaseMap", decalMaps[i]);
            plane.GetComponent<Renderer>().material = m;
            plane.transform.position = debugLocation.position + debugLocation.right * i;
            debugPlanes.Add(plane);
        }
    }*/

    IEnumerator FadeOutDynamicTextures(float period) {
        while(true) {
            yield return new WaitForSeconds(period);
            List<Renderer> shouldRemoveBecauseFadedOut = new List<Renderer>();
            CommandBuffer buffer = new CommandBuffer();
            foreach (KeyValuePair<Renderer, CachedRenderTexture> p in dynamicRenderTextureCache) {
                buffer.Blit(transparentTex, p.Value.texture, alphaBlend);
                p.Value.probableAlpha=Mathf.Max(p.Value.probableAlpha-(1f/255f),0f);
                if (p.Value.probableAlpha <= 0f) {
                    shouldRemoveBecauseFadedOut.Add(p.Key);
                }
            }
            Graphics.ExecuteCommandBuffer(buffer);
            buffer.Release();
            // We try to clean up textures if they're completely transparent, or if we're over the max
            foreach(Renderer r in shouldRemoveBecauseFadedOut) {
                dynamicRenderTextureCache[r].texture.Release();
                dynamicRenderTextureCache.Remove(r);
            }
            while (dynamicRenderTextureCache.Count > maxRenderTextures && maxRenderTextures > 0) {
                Renderer r = null;
                float oldest = float.MaxValue;
                foreach (KeyValuePair<Renderer, CachedRenderTexture> p in dynamicRenderTextureCache) {
                    if (p.Value.timeCreated < oldest) {
                        oldest = p.Value.timeCreated;
                        r = p.Key;
                    }
                }
                dynamicRenderTextureCache.Remove(r);
            }
        }
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
                break;
            }
        }
        return decalableMaterials;
    }

    public RenderTexture GetStaticRenderTexture(Renderer r) {
        if (staticRenderables.Contains(r)) {
            return decalMaps[r.lightmapIndex];
        }
        // If we already have an instanciated material of the right type, use that.
        foreach(Material m in r.materials) {
            if (!IsDecalable(m)) {
                continue;
            }
            m.SetTexture("_DecalColorMap", decalMaps[r.lightmapIndex]);
        }
        staticRenderables.Add(r);
        return decalMaps[r.lightmapIndex];
    }

    // Check if we're a valid renderer to render a decal too.
    // If we have any material with a _DecalColorMap texture variable
    // We try to use the render target that exists there, if not we create one.
    // We also try to use dedicated DecalMaps for lightmapped objects.
    public RenderTexture GetDynamicRenderTexture(Renderer r) {
        // we've already cached this material, return it
        if (dynamicRenderTextureCache.ContainsKey(r)) {
            dynamicRenderTextureCache[r].probableAlpha = 1f;
            dynamicRenderTextureCache[r].timeCreated = Time.timeSinceLevelLoad;
            return dynamicRenderTextureCache[r].texture;
        }
        List<Material> mats;
        mats = GetDecalableMaterials(r);
        if (mats.Count <= 0 ) {
            return null;
        }
        RenderTexture target = (RenderTexture)mats[0].GetTexture("_DecalColorMap");
        if (target == null) {
            target = new RenderTexture(renderTextureResolution, renderTextureResolution, 0);
            target.name = r.name + " Decalmap";
            dynamicRenderTextureCache[r] = new CachedRenderTexture { texture=target, timeCreated=Time.timeSinceLevelLoad, probableAlpha=1f };
        }
        foreach (Material m in mats) {
            m.SetTexture("_DecalColorMap", target);
        }
        return target;
    }

    public RenderTexture RenderDecal(Renderer r, Texture decal, Vector3 position, Quaternion rotation, Color color, float size = 1f, float depth = 0.5f, bool addPadding = true) {
        //Debug.Log(r + " " + decal + " " + color + " " + size);
        RenderTexture target = null;
        if (r.lightmapIndex >= 0) {
            // Skip static geo if we don't have decal maps to render to.
            if (decalMapTextureResolution <= 0) {
                return null;
            }
            target = GetStaticRenderTexture(r);
        } else {
            target = GetDynamicRenderTexture(r);
        }
        if (target == null) {
            return null;
        }
        // With a valid target, generate a material list with the decal projector on the right submesh, with all other submeshes set to an invisible material.
        decalProjector.SetTexture("_Decal", decal);
        decalProjector.SetColor("_BaseColor", color);

        decalCamera.transform.position = position;
        decalCamera.transform.rotation = rotation;
        if (r.lightmapIndex >= 0) {
            // FIXME: Not sure why, but there is a discrepancy between URP and HDRP's lightmap information. This might actually depend on lightmap settings.
            // Though for now this works fine.
            if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition")) {
                // HDRP uses LightmapScaleOffset
                decalProjector.SetVector("_lightmapST", r.lightmapScaleOffset);
            } else {
                // URP uses the realtimeLightmapScaleOffset
                decalProjector.SetVector("_lightmapST", r.realtimeLightmapScaleOffset);
            }
            decalCamera.orthographicSize = size * 3;
        } else {
            decalProjector.SetVector("_lightmapST", new Vector4(1,1,0,0));
            decalCamera.orthographicSize = size;
        }
        decalCamera.farClipPlane = depth;
        decalCamera.nearClipPlane = 0f;
        decalCamera.targetTexture = target;
        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(target);
        buffer.SetViewProjectionMatrices(decalCamera.worldToCameraMatrix, decalCamera.projectionMatrix);
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
            if (addPadding && r.lightmapIndex == -1) {
                Vector2 pixelSize = new Vector2(1f,1f);
                Vector2 pixelSizeAlso = new Vector2(-1f,1f);
                buffer.SetViewport(new Rect(pixelSize, pixelRect + pixelSize));
                buffer.DrawRenderer(r, decalProjector, i);
                buffer.SetViewport(new Rect(-pixelSize, pixelRect - pixelSize));
                buffer.DrawRenderer(r, decalProjector, i);
                buffer.SetViewport(new Rect(pixelSizeAlso, pixelRect + pixelSizeAlso));
                buffer.DrawRenderer(r, decalProjector, i);
                buffer.SetViewport(new Rect(-pixelSizeAlso, pixelRect - pixelSizeAlso));
                buffer.DrawRenderer(r, decalProjector, i);
            } else {
                buffer.DrawRenderer(r, decalProjector, i);
            }
        }
        Graphics.ExecuteCommandBuffer(buffer);
        return target;
    }

    // This releases all render textures and then creates new ones at the desired resolution.
    // A cool feature might be to copy the data over so that decals don't get reset completely, but this doesn't do that.
    public void RegenerateDecalMaps(Scene scene = default(Scene), LoadSceneMode mode = LoadSceneMode.Single) {
        if (decalMapTextureResolution <= 0) {
            return;
        }
        for(int i=0;i<decalMaps.Count;i++) {
            decalMaps[i].Release();
        }
        decalMaps.Clear();
        for(int i=0;i<LightmapSettings.lightmaps.Length;i++) {
            RenderTexture t = new RenderTexture(decalMapTextureResolution, decalMapTextureResolution,0);
            t.name = "Lightmap" + i;
            decalMaps.Add(t);
        }
        foreach (KeyValuePair<Renderer, CachedRenderTexture> p in dynamicRenderTextureCache) {
            p.Value.texture.Release();
        }
        dynamicRenderTextureCache.Clear();
        staticRenderables.Clear();
        //CreateDebugPlanes();
    }

/* public void OnEventRaised(GraphicsOptions.OptionType target, float value) {
        switch(target) {
            case GraphicsOptions.OptionType.DecalQuality:
                // 0 == 256, 1 == 512, 2 == 1024
                renderTextureResolution = (int)Mathf.Pow(2,8+(value-1));
                // 0 == 512, 1 == 1024, 2 == 2048
                decalMapTextureResolution = (int)Mathf.Pow(2,9+(value-1));
                // 0 == 4, 1 == 8, 2 == 16
                maxRenderTextures = (int)Mathf.Pow(2,2+(value-1));
                RegenerateDecalMaps();
                break;
        }
    }*/
}


