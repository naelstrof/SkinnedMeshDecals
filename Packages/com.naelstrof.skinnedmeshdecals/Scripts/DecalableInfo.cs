using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {
internal class MonoBehaviourHider {
internal class DecalableInfo : MonoBehaviour {
    private class TextureTarget {
        private RenderTexture baseTexture;
        private RenderTexture outputTexture;
        private readonly List<int> drawIndices;
        public readonly bool dilationEnabled;
        private bool overridden = false;
        private static void ClearRenderTexture(RenderTexture target) {
            CommandBuffer buffer = new CommandBuffer();
            buffer.SetRenderTarget(target);
            buffer.ClearRenderTarget(true, true, Color.clear);
            buffer.GenerateMips(target);
            Graphics.ExecuteCommandBuffer(buffer);
        }

        public List<int> GetDrawIndices() => drawIndices;
        public RenderTexture GetBaseTexture() => baseTexture;

        public RenderTexture GetOutputTexture() => dilationEnabled ? outputTexture : baseTexture;

        public int GetSize() {
            if (overridden) {
                return 0;
            }
            int size = 0;
            size += baseTexture.width*baseTexture.height*4;
            if (dilationEnabled) {
                size += outputTexture.width * outputTexture.height * 4;
            }
            return size;
        }

        public void Release() {
            if (baseTexture != null && !overridden) {
                baseTexture.Release();
                baseTexture = null;
            }
            if (dilationEnabled && outputTexture != null) {
                outputTexture.Release();
                outputTexture = null;
            }
        }

        public void OverrideTexture(RenderTexture texture) {
            Release();
            overridden = true;
            baseTexture = texture;
            if (dilationEnabled) {
                outputTexture = new RenderTexture(texture);
                CommandBuffer buffer = new CommandBuffer();
                buffer.Blit(texture, outputTexture, PaintDecal.GetDilationMaterial());
                buffer.GenerateMips(outputTexture);
                Graphics.ExecuteCommandBuffer(buffer);
            }
        }
        
        public TextureTarget(int textureID, RenderTexture texture, Material[] materials, bool dilationEnabled) {
            overridden = true;
            this.dilationEnabled = dilationEnabled;
            drawIndices = new List<int>();
            baseTexture = texture;
            if (this.dilationEnabled) {
                outputTexture = new RenderTexture(texture);
                CommandBuffer buffer = new CommandBuffer();
                buffer.Blit(texture, outputTexture, PaintDecal.GetDilationMaterial());
                buffer.GenerateMips(outputTexture);
                Graphics.ExecuteCommandBuffer(buffer);
            }
            for (int i=0;i<materials.Length;i++) {
                if (materials[i].HasProperty(textureID)) {
                    drawIndices.Add(i);
                }
            }
        }

        public TextureTarget(int textureID, Vector2Int textureSize, Material[] materials, bool dilationEnabled, RenderTextureFormat renderTextureFormat, RenderTextureReadWrite renderTextureReadWrite) {
            overridden = false;
            this.dilationEnabled = dilationEnabled;
            drawIndices = new List<int>();
            baseTexture = new RenderTexture(textureSize.x, textureSize.y, 0, renderTextureFormat, renderTextureReadWrite) {
                antiAliasing = 1,
                useMipMap = true,
                autoGenerateMips = false,
            };
            ClearRenderTexture(baseTexture);
            if (this.dilationEnabled) {
                outputTexture = new RenderTexture(textureSize.x, textureSize.y, 0, renderTextureFormat, renderTextureReadWrite) {
                    antiAliasing = 1,
                    useMipMap = true,
                    autoGenerateMips = false,
                };
                ClearRenderTexture(outputTexture);
            }
            for (int i=0;i<materials.Length;i++) {
                if (materials[i].HasProperty(textureID)) {
                    drawIndices.Add(i);
                }
            }
        }
    }
    private Dictionary<int, TextureTarget> textureTargets;
    private new Renderer renderer;
    private float lastUse;
    private MaterialPropertyBlock propertyBlock;

    public float GetLastUseTime() {
        return lastUse;
    }

    public RenderTexture GetRenderTexture(int textureId) {
        if (textureTargets.ContainsKey(textureId)) {
            return textureTargets[textureId].GetBaseTexture();
        }
        return null;
    }

    public int GetMemoryUsed() {
        int size = 0;
        foreach(var pair in textureTargets) {
            size += pair.Value.GetSize();
        }
        return size;
    }
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
    public void Initialize() {
        propertyBlock = new MaterialPropertyBlock();
        textureTargets = new Dictionary<int, TextureTarget>();
        renderer = GetComponent<Renderer>();
        lastUse = Time.time;
        PaintDecal.AddDecalableInfo(this);
    }

    public void OverrideTexture(RenderTexture texture, int textureId, bool dilationEnabled) {
        if (!textureTargets.ContainsKey(textureId)) {
            TextureTarget texTarget = new TextureTarget(textureId, texture, renderer.materials, dilationEnabled);
            textureTargets.Add(textureId, texTarget);
        } else {
            textureTargets[textureId].OverrideTexture(texture);
        }
        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(textureId, textureTargets[textureId].GetOutputTexture());
        renderer.SetPropertyBlock(propertyBlock);
    }
    public void Render(CommandBuffer buffer, DecalProjector projector, DecalSettings decalSettings) {
        if (textureTargets == null) {
            Destroy(this);
            return;
        }

        // Create the texture if we don't have it.
        if (!textureTargets.ContainsKey(decalSettings.textureID)) {
            Vector2Int textureSize = Vector2Int.one * 16;
            if (decalSettings.resolution.resolutionType == DecalResolutionType.Auto) {
                var bounds = renderer.bounds;
                float maxA = Mathf.Max(bounds.extents.x * bounds.extents.y, bounds.extents.x * bounds.extents.z);
                float maxBounds = Mathf.Max(maxA, bounds.extents.y * bounds.extents.z);
                int worldScale = Mathf.RoundToInt(maxBounds * decalSettings.resolution.texelsPerMeter);
                int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldScale), 16, 2048);
                if (float.IsNaN(textureScale) || float.IsInfinity(textureScale)) {
                    textureScale = 1024;
                }
                textureSize = Vector2Int.one * textureScale;
            } else {
                textureSize = decalSettings.resolution.size;
            }

            int reserveMemory = decalSettings.dilation ? 2 * textureSize.x * textureSize.y * 4 : textureSize.x * textureSize.y * 4;
            reserveMemory = Mathf.Max(reserveMemory, 16*4);
            if (!PaintDecal.TryReserveMemory(reserveMemory)) {
                return;
            }

            TextureTarget texTarget = new TextureTarget(decalSettings.textureID, textureSize, renderer.materials, decalSettings.dilation, decalSettings.renderTextureFormat, decalSettings.renderTextureReadWrite);
            textureTargets.Add(decalSettings.textureID, texTarget);
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(decalSettings.textureID, texTarget.GetOutputTexture());
            renderer.SetPropertyBlock(propertyBlock);
        }
        TextureTarget target = textureTargets[decalSettings.textureID];
        buffer.SetRenderTarget(target.GetBaseTexture());
        Vector2 pixelRect = new Vector2(target.GetBaseTexture().width, target.GetBaseTexture().height);
        buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
        foreach(int drawIndex in target.GetDrawIndices()) {
            buffer.DrawRenderer(renderer, projector.material, drawIndex);
        }

        if (textureTargets[decalSettings.textureID].dilationEnabled) {
            buffer.Blit(target.GetBaseTexture(), target.GetOutputTexture(), PaintDecal.GetDilationMaterial());
            buffer.GenerateMips(target.GetOutputTexture());
        } else {
            buffer.GenerateMips(target.GetBaseTexture());
        }

        lastUse = Time.time;
    }
    
    void OnDestroy() {
        PaintDecal.RemoveDecalableInfo(this);
        if (textureTargets == null) {
            return;
        }
        foreach (var pair in textureTargets) {
            pair.Value?.Release();
        }

        if (renderer == null || propertyBlock == null) {
            return;
        }

        renderer.GetPropertyBlock(propertyBlock);
        foreach(var pair in textureTargets) {
            propertyBlock.SetTexture(pair.Key, Texture2D.blackTexture);
        }
        renderer.SetPropertyBlock(propertyBlock);
    }
}

}
}