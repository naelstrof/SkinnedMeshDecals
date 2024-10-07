using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {
internal partial class MonoBehaviourHider {
    
internal class DecalableRenderer : MonoBehaviour {
    private static readonly List<DecalableRenderer> decalableRenderers = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void StaticInitialize() {
        decalableRenderers.Clear();
    }
    
    internal static void GetRenderTextures(List<RenderTexture> textures, int textureId) {
        foreach (var decalableRenderer in decalableRenderers) {
            textures.Add(decalableRenderer.GetRenderTexture(textureId));
        }
    }

    internal static void TryHitTargetMemory(ulong targetBits) {
        while (GetTotalBitsInUse() > targetBits) {
            foreach (var decalableRenderer in decalableRenderers) {
                if (!decalableRenderer.initialized) continue;
                decalableRenderer.Release();
                break;
            }
        }
    }
    internal static void ClearAll() {
        foreach (var decalableRenderer in decalableRenderers) {
            decalableRenderer.Release();
        }
    }
    private static void AddDecalableRenderer(DecalableRenderer renderer) {
        if (!decalableRenderers.Contains(renderer)) {
            decalableRenderers.Add(renderer);
        }
    }
    private static void RemoveDecalableRenderer(DecalableRenderer renderer) {
        if (decalableRenderers.Contains(renderer)) {
            decalableRenderers.Remove(renderer);
        }
    }
    private static ulong GetTotalBitsInUse() {
        ulong bitsInUse = 0;
        foreach (var decalableRenderer in decalableRenderers) {
            decalableRenderer.GetBitsInUse(ref bitsInUse);
        }
        return bitsInUse;
    }
    

    private static bool TryReleaseOldest() {
        DecalableRenderer oldestRenderer = null;
        float oldestTime = float.MaxValue;
        foreach(var info in decalableRenderers) {
            if (info.GetLastUseTime() >= oldestTime) continue;
            oldestRenderer = info;
            oldestTime = info.GetLastUseTime();
        }
        if (oldestRenderer == null && decalableRenderers.Count > 0) {
            oldestRenderer = decalableRenderers[0];
        }
        if (oldestRenderer == null) return false;
        oldestRenderer.Release();
        return true;
    }
    
    private class TextureTarget {
        private RenderTexture baseTexture;
        private RenderTexture outputTexture;
        public readonly DilationType dilation;
        private bool overridden = false;

        public void GetBitsInUse(ref ulong bits) {
            if (baseTexture != null) {
                bits += baseTexture.GetTotalBitsVRAM();
            }

            if (outputTexture != null) {
                bits += outputTexture.GetTotalBitsVRAM();
            }
        }
        
        public int textureId { get; private set; }
        private static void ClearRenderTexture(RenderTexture target) {
            CommandBuffer buffer = new CommandBuffer();
            buffer.SetRenderTarget(target);
            buffer.ClearRenderTarget(true, true, Color.clear);
            buffer.GenerateMips(target);
            Graphics.ExecuteCommandBuffer(buffer);
        }

        public RenderTexture GetBaseTexture() => baseTexture;

        public RenderTexture GetOutputTexture() => dilation != DilationType.None ? outputTexture : baseTexture;

        public int GetSize() {
            if (overridden) {
                return 0;
            }
            int size = 0;
            size += baseTexture.width*baseTexture.height*4;
            if (dilation != DilationType.None) {
                size += outputTexture.width * outputTexture.height * 4;
            }
            return size;
        }

        public void Release() {
            if (baseTexture != null && !overridden) {
                baseTexture.Release();
                baseTexture = null;
            }
            if (outputTexture != null) {
                outputTexture.Release();
                outputTexture = null;
            }
        }

        public void OverrideTexture(RenderTexture texture) {
            Release();
            overridden = true;
            baseTexture = texture;
            if (dilation != DilationType.None) {
                outputTexture = new RenderTexture(texture);
                CommandBuffer buffer = new CommandBuffer();
                buffer.Blit(texture, outputTexture, PaintDecal.GetDilationMaterial(dilation));
                buffer.GenerateMips(outputTexture);
                Graphics.ExecuteCommandBuffer(buffer);
            }
        }
        
        public TextureTarget(int textureId, RenderTexture texture, DilationType dilation) {
            overridden = true;
            this.dilation = dilation;
            this.textureId = textureId;
            baseTexture = texture;
            if (this.dilation != DilationType.None) {
                outputTexture = new RenderTexture(texture);
                CommandBuffer buffer = new CommandBuffer();
                buffer.Blit(texture, outputTexture, PaintDecal.GetDilationMaterial(this.dilation));
                buffer.GenerateMips(outputTexture);
                Graphics.ExecuteCommandBuffer(buffer);
            }
        }

        public TextureTarget(int textureId, Vector2Int textureSize, DilationType dilation, RenderTextureFormat renderTextureFormat, RenderTextureReadWrite renderTextureReadWrite) {
            overridden = false;
            this.dilation = dilation;
            this.textureId = textureId;
            baseTexture = new RenderTexture(textureSize.x, textureSize.y, 0, renderTextureFormat, renderTextureReadWrite) {
                antiAliasing = 1,
                useMipMap = true,
                autoGenerateMips = false,
            };
            ClearRenderTexture(baseTexture);
            if (this.dilation != DilationType.None) {
                outputTexture = new RenderTexture(textureSize.x, textureSize.y, 0, renderTextureFormat, renderTextureReadWrite) {
                    antiAliasing = 1,
                    useMipMap = true,
                    autoGenerateMips = false,
                };
                ClearRenderTexture(outputTexture);
            }
        }
    }
    private Dictionary<int, TextureTarget> textureTargets;
    private new Renderer renderer;
    
    private float lastUse;
    private MaterialPropertyBlock propertyBlock;
    private MaterialPropertyBlock decalblitPropertyBlock;

    public float GetLastUseTime() {
        return lastUse;
    }

    private void Awake() {
        renderer = GetComponent<Renderer>();
        AddDecalableRenderer(this);
        DecalCommandProcessor.EnsureInstanceAlive();
        hideFlags = HideFlags.HideAndDontSave;
    }

    public RenderTexture GetRenderTexture(int textureId) {
        if (textureTargets.ContainsKey(textureId)) {
            return textureTargets[textureId].GetBaseTexture();
        }
        return null;
    }

    private void GetBitsInUse(ref ulong memory) {
        if (!initialized) return;
        foreach (var textureTarget in textureTargets) {
            textureTarget.Value.GetBitsInUse(ref memory);
        }
    }
    private static int CeilPowerOfTwo(int v) {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;
        return v;
    }
    private bool initialized => textureTargets != null;
    private void Initialize() {
        if (initialized) {
            return;
        }
        propertyBlock = new MaterialPropertyBlock();
        textureTargets = new Dictionary<int, TextureTarget>();
        lastUse = Time.time;
    }

    public void OverrideTexture(RenderTexture texture, int textureId, DilationType dilation) {
        if (!textureTargets.ContainsKey(textureId)) {
            TextureTarget texTarget = new TextureTarget(textureId, texture, dilation);
            textureTargets.Add(textureId, texTarget);
        } else {
            textureTargets[textureId].OverrideTexture(texture);
        }
        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(textureId, textureTargets[textureId].GetOutputTexture());
        renderer.SetPropertyBlock(propertyBlock);
    }

    public bool TryApply(CommandBuffer buffer, DecalProjector projector, DecalSettings decalSettings) {
        try {
            Initialize();
            // Create the texture if we don't have it.
            if (!textureTargets.ContainsKey(decalSettings.textureID)) {
                Vector2Int textureSize = Vector2Int.one * 16;
                if (decalSettings.resolution.resolutionType == DecalResolutionType.Auto) {
                    textureSize = Vector2Int.one * ProcessAutoTextureScale(GetSurfaceArea(renderer),
                        decalSettings.resolution.texelsPerMeter);
                } else {
                    textureSize = decalSettings.resolution.size;
                }

                TextureTarget texTarget = new TextureTarget(decalSettings.textureID, textureSize,
                    decalSettings.dilation, decalSettings.renderTextureFormat, decalSettings.renderTextureReadWrite);
                textureTargets.Add(decalSettings.textureID, texTarget);
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetTexture(decalSettings.textureID, texTarget.GetOutputTexture());
                renderer.SetPropertyBlock(propertyBlock);
            }

            TextureTarget target = textureTargets[decalSettings.textureID];
            buffer.SetRenderTarget(target.GetBaseTexture());
            Vector2 pixelRect = new Vector2(target.GetBaseTexture().width, target.GetBaseTexture().height);
            buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
            buffer.DrawRenderer(renderer, projector.material);

            if (textureTargets[decalSettings.textureID].dilation != DilationType.None) {
                buffer.Blit(target.GetBaseTexture(), target.GetOutputTexture(),
                    PaintDecal.GetDilationMaterial(textureTargets[decalSettings.textureID].dilation));
                buffer.GenerateMips(target.GetOutputTexture());
            } else {
                buffer.GenerateMips(target.GetBaseTexture());
            }

            lastUse = Time.time;
        } catch (UnityException e) {
//#if UNITY_EDITOR
            Debug.LogException(e);
//#endif
            return false;
        }

        return true;
    }

    public static float GetSurfaceArea(Renderer renderer) {
        var bounds = renderer.bounds;
        float surfaceArea = 2f*bounds.size.z*bounds.size.x + 2f*bounds.size.z * bounds.size.y + 2f*bounds.size.x*bounds.size.y;
        if (renderer is SkinnedMeshRenderer) {
            surfaceArea *= 2f;
        }
        return surfaceArea;
    }
    
    public static int ProcessAutoTextureScale(float surfaceArea, float texelsPerMeter) {
        int worldTexelScale = Mathf.RoundToInt(surfaceArea * texelsPerMeter);
        int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldTexelScale), 16, 2048);
        if (float.IsNaN(textureScale) || float.IsInfinity(textureScale)) {
            textureScale = 1024;
        }
        return textureScale;
    }
    
    public void Release() {
        if (!initialized) {
            return;
        }
        foreach (var pair in textureTargets) {
            pair.Value?.Release();
        }
        if (renderer == null || propertyBlock == null) {
            textureTargets = null;
            return;
        }

        renderer.GetPropertyBlock(propertyBlock);
        foreach(var pair in textureTargets) {
            propertyBlock.SetTexture(pair.Key, Texture2D.blackTexture);
        }
        renderer.SetPropertyBlock(propertyBlock);
        textureTargets = null;
    }
    
    void OnDestroy() {
        Release();
        RemoveDecalableRenderer(this);
    }
}

}
}