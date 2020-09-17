using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System;

public class PaintDecal : MonoBehaviour {
    public static PaintDecal instance = null;
    //public Material invisible;
    public Camera decalCamera;
    [Tooltip("Material with a shader that projects decals on skinned meshes. Uses _BaseColorMap and _BaseColor to set the texture and color respectively.")]
    public Material decalProjector;
    [Tooltip("Material used to fade existing decals away (By blending a low-transparency texture over existing framebuffers). I use a AlphaBlendOp of Subtract with the AlphaBlend set to One One.")]
    public Material alphaBlend;

    [Tooltip("How quickly textures fade away per second.")]
    [Range(0,0.1f)]
    public float fadeRate = 0.003f;

    [Tooltip("How big of render textures should we allocate when we need them?")]
    public int renderTextureResolution = 1024;
    [Tooltip("Maximum number of render textures before we start culling random ones.")]
    public int maxRenderTextures = 16;
    // Store both the material id and the rendertexture
    private Dictionary<Renderer, RenderTexture> renderTexturesCache = new Dictionary<Renderer, RenderTexture>();
    private RenderTexture transparentTex;
    private float fadeRateAccumulator;
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

    private void Start() {
        transparentTex = new RenderTexture(16, 16, 0);
        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(transparentTex);
        buffer.ClearRenderTarget(false, true, new Color(0, 0, 0, 1/256f));
        Graphics.ExecuteCommandBuffer(buffer);
    }

    private void FixedUpdate() {
        // Really lazily figure out if we should destroy framebuffers, also really lazily figure out which one to destroy.
        // TODO: Each framebuffer should probably be kept track of (how much its been faded, most recent decal), and should probably be re-used rather than destroyed/released.
        HashSet<Renderer> removeList = new HashSet<Renderer>();
        int count = 0;
        foreach(KeyValuePair<Renderer, RenderTexture> p in renderTexturesCache) {
            if (p.Key == null) {
                p.Value.Release();
                //renderTexturesCache.Remove(p.Key);
                removeList.Add(p.Key);
                count--;
                continue;
            }
            count++;
        }

        foreach (KeyValuePair<Renderer, RenderTexture> p in renderTexturesCache) {
            if (count > maxRenderTextures && UnityEngine.Random.Range(0f,1f)<(1f/(float)maxRenderTextures)) {
                count--;
                p.Value.Release();
                removeList.Add(p.Key);
            }
        }

        foreach (Renderer r in removeList) {
            if (renderTexturesCache.ContainsKey(r)) {
                renderTexturesCache.Remove(r);
            }
        }

        // Really slowly fade the decals away, can only do it at one color step at a time (1f/256f).
        fadeRateAccumulator += Time.fixedDeltaTime*fadeRate;
        if (fadeRateAccumulator > 1f/256f) {
            CommandBuffer buffer = new CommandBuffer();
            foreach (KeyValuePair<Renderer, RenderTexture> p in renderTexturesCache) {
                buffer.Blit(transparentTex, p.Value, alphaBlend);
            }
            Graphics.ExecuteCommandBuffer(buffer);
            fadeRateAccumulator -= 1f / 256f;
        }
    }
    // Retreieve a list of materials that contain the texture input "_DecalColorMap"
    private List<Material> GetDecalableMaterials(Renderer[] renderers) {
        List<Material> decalableMaterials = new List<Material>();
        foreach(Renderer r in renderers) {
            foreach (Material m in r.materials) {
                foreach(string s in m.GetTexturePropertyNames()) {
                    if (s == "_DecalColorMap") {
                        decalableMaterials.Add(m);
                        break;
                    }
                }
            }
        }
        return decalableMaterials;
    }

    /// <summary>
    /// Renders a decal to the given renderer, and to all the renderers in the same heirarchy as it (so that LODs also get the decal.)
    /// </summary>
    /// <returns>
    /// The RenderTexture used to render the decal.
    /// </returns>
    /// <param name="r">The renderer to render the decal to.</param>
    /// <param name="decal">The texture to render as a decal onto the renderer.</param>
    /// <param name="position">Where to render the decal.</param>
    /// <param name="rotation">Which direction to render the decal at.</param>
    /// <param name="color">Which color to set the decal to.</param>
    /// <param name="size">Sets the orthographic size of the camera, if it's an orthographic camera.</param>
    public RenderTexture RenderDecal(Renderer r, Texture decal, Vector3 position, Quaternion rotation, Color color, float size = 1f) {
        // Check if we're a valid renderer to render a decal too.
        // If we have any material with a _DecalColorMap texture variable
        // We try to use the render target that exists there, if not we create one.
        RenderTexture target = null;
        if (renderTexturesCache.ContainsKey(r)) {
            target = renderTexturesCache[r];
        } else {
            // FIXME: We apply the rendertexture to EVERY renderer that has a _DecalColorMap with the same parent as the given renderer.
            // This is so that a model can have LODs set up and still get the decals applied to every LOD group.
            List<Material> mats = GetDecalableMaterials(r.transform.parent.GetComponentsInChildren<Renderer>());
            if (mats.Count <= 0 ) {
                return null;
            }
            RenderTexture matRenderTarget = (RenderTexture)mats[0].GetTexture("_DecalColorMap");
            if (matRenderTarget == null) {
                target = new RenderTexture(renderTextureResolution, renderTextureResolution, 0);
                renderTexturesCache[r] = target;
            } else {
                target = matRenderTarget;
            }
            foreach (Material m in mats) {
                m.SetTexture("_DecalColorMap", target);
            }
        }
        if (target == null) {
            return target;
        }

        // With a valid target, generate a material list with the decal projector on the right submesh, with all other submeshes set to an invisible material.
        decalProjector.SetTexture("_Decal", decal);
        decalProjector.SetColor("_BaseColor", color);

        decalCamera.transform.position = position;
        decalCamera.transform.rotation = rotation;
        decalCamera.orthographicSize = size;
        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(decalCamera.targetTexture);
        buffer.SetViewProjectionMatrices(decalCamera.worldToCameraMatrix, decalCamera.projectionMatrix);
        // Jitter the render so that seams don't show up, should be pretty cheap for most GPUs to do.
        Vector2 pixelRect = new Vector2(decalCamera.scaledPixelWidth, decalCamera.scaledPixelHeight);
        Vector2 pixelSize = new Vector2(1.5f,1.5f);
        Vector2 pixelSizeAlso = new Vector2(-1.5f,1.5f);
        buffer.SetViewport(new Rect(pixelSize, pixelRect + pixelSize));
        buffer.DrawRenderer(r, decalProjector);
        buffer.SetViewport(new Rect(-pixelSize, pixelRect - pixelSize));
        buffer.DrawRenderer(r, decalProjector);
        buffer.SetViewport(new Rect(pixelSizeAlso, pixelRect + pixelSizeAlso));
        buffer.DrawRenderer(r, decalProjector);
        buffer.SetViewport(new Rect(-pixelSizeAlso, pixelRect - pixelSizeAlso));
        buffer.DrawRenderer(r, decalProjector);
        Graphics.ExecuteCommandBuffer(buffer);
        // Finally copy the result into our _DecalColorMap target
        CopyTexture(decalCamera.targetTexture, target);
        return target;
    }

    private void CopyTexture(RenderTexture src, RenderTexture dst) {
        RenderTexture.active = dst;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, src.width, src.height, 0);
        Graphics.DrawTexture(new Rect(0, 0, src.width, src.height), src);
        GL.PopMatrix();
        RenderTexture.active = null;
    }
}


