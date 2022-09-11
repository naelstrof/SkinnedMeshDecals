using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {
public class MonoBehaviourHider {
public class DecalableInfo : MonoBehaviour {
    private class TextureTarget {
        private RenderTexture baseTexture;
        private RenderTexture outputTexture;
        private readonly List<int> drawIndices;
        private bool dilationEnabled;
        private static void ClearRenderTexture(RenderTexture target) {
            var rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        public List<int> GetDrawIndices() => drawIndices;
        public RenderTexture GetBaseTexture() => baseTexture;

        public RenderTexture GetOutputTexture() => outputTexture;

        public int GetSize() {
            int size = 0;
            size += baseTexture.width*baseTexture.height*4;
            if (dilationEnabled) {
                size += outputTexture.width * outputTexture.height * 4;
            }
            return size;
        }

        public void Release() {
            if (baseTexture != null) {
                baseTexture.Release();
                baseTexture = null;
            }
            if (dilationEnabled && outputTexture != null) {
                outputTexture.Release();
                outputTexture = null;
            }
        }

        public TextureTarget(string textureName, int textureScale, Material[] materials, bool dilationEnabled) {
            this.dilationEnabled = dilationEnabled;
            drawIndices = new List<int>();
            baseTexture = new RenderTexture(textureScale, textureScale, 0) {
                antiAliasing = 1,
                useMipMap = !this.dilationEnabled,
                autoGenerateMips = false,
            };
            ClearRenderTexture(baseTexture);
            if (this.dilationEnabled) {
                outputTexture = new RenderTexture(textureScale, textureScale, 0) {
                    antiAliasing = 1,
                    useMipMap = true,
                    autoGenerateMips = false,
                };
                ClearRenderTexture(outputTexture);
            }
            for (int i=0;i<materials.Length;i++) {
                if (materials[i].HasProperty(textureName)) {
                    drawIndices.Add(i);
                }
            }
        }
    }
    private Dictionary<string, TextureTarget> textureTargets;
    private new Renderer renderer;
    private float lastUse;
    private MaterialPropertyBlock propertyBlock;
    private bool dilationEnabled;

    public float GetLastUseTime() {
        return lastUse;
    }

    public int GetSize() {
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
        textureTargets = new Dictionary<string, TextureTarget>();
        renderer = GetComponent<Renderer>();
        lastUse = Time.time;
        dilationEnabled = PaintDecal.IsDilateEnabled();
        PaintDecal.AddDecalableInfo(this);
    }
    public void Render(CommandBuffer buffer, Material projector, string textureName) {
        if (textureTargets == null) {
            Destroy(this);
            return;
        }

        // Create the texture if we don't have it.
        if (!textureTargets.ContainsKey(textureName)) {
            var bounds = renderer.bounds;
            float maxA = Mathf.Max(bounds.extents.x*bounds.extents.y, bounds.extents.x*bounds.extents.z);
            float maxBounds = Mathf.Max(maxA, bounds.extents.y*bounds.extents.z);
            int worldScale = Mathf.RoundToInt(maxBounds*PaintDecal.GetTexelsPerMeter());
            int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldScale), 16, 2048);
            if (!PaintDecal.TryReserveMemory(dilationEnabled ? 2*textureScale*textureScale*4 : textureScale*textureScale*4)) {
                return;
            }

            TextureTarget texTarget = new TextureTarget(textureName, textureScale, renderer.materials, dilationEnabled);
            textureTargets.Add(textureName, texTarget);
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(textureName, dilationEnabled ? texTarget.GetOutputTexture() : texTarget.GetBaseTexture());
            renderer.SetPropertyBlock(propertyBlock);
        }
        TextureTarget target = textureTargets[textureName];
        buffer.SetRenderTarget(target.GetBaseTexture());
        Vector2 pixelRect = new Vector2(target.GetBaseTexture().width, target.GetBaseTexture().height);
        buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
        foreach(int drawIndex in target.GetDrawIndices()) {
            buffer.DrawRenderer(renderer, projector, drawIndex);
        }

        if (dilationEnabled) {
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