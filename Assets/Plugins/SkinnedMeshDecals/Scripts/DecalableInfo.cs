using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {
public class MonobehaviourHider {
public class DecalableInfo : MonoBehaviour {
    private Material[] materials;
    private class TextureTarget {
        private static void ClearRenderTexture(RenderTexture target) {
            var rt = UnityEngine.RenderTexture.active;
            UnityEngine.RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            UnityEngine.RenderTexture.active = rt;
        }
        public TextureTarget(string textureName, int textureScale, Renderer renderer, Material[] materials) {
            drawIndices = new List<int>();
            decalableMaterials = new List<Material>();
            texture = new RenderTexture(textureScale, textureScale, 0);
            ClearRenderTexture(texture);
            for (int i=0;i<materials.Length;i++) {
                if (materials[i].HasProperty(textureName)) {
                    decalableMaterials.Add(materials[i]);
                    drawIndices.Add(i);
                }
            }
            PaintDecal.OnMemoryChanged();
        }
        public RenderTexture texture;
        public List<int> drawIndices;
        public List<Material> decalableMaterials;
    }
    private Dictionary<string, TextureTarget> textureTargets = new Dictionary<string, TextureTarget>();
    private new Renderer renderer;
    public float lastUse;
    public float memorySizeMB {
        get {
            float size = 0f;
            foreach(var pair in textureTargets) {
                size += (pair.Value.texture.width*pair.Value.texture.height*4f)/(float)(1e+6f);
            }
            return size;
        }
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
    void Awake() {
        renderer = GetComponent<Renderer>();
        materials = renderer.materials;
        lastUse = Time.time;
        PaintDecal.AddDecalableInfo(this);
    }
    public void Render(CommandBuffer buffer, Material projector, string textureName) {
        // Create the texture if we don't have it.
        if (!textureTargets.ContainsKey(textureName)) {
            int worldScale = Mathf.RoundToInt(renderer.bounds.extents.magnitude*PaintDecal.texelsPerMeter);
            int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldScale), 16, 4096);

            textureTargets[textureName] = new TextureTarget(textureName, textureScale, renderer, materials);
            foreach(Material mat in materials) {
                if (mat.HasProperty(textureName)) {
                    mat.SetTexture(textureName, textureTargets[textureName].texture);
                }
            }
        }
        TextureTarget target = textureTargets[textureName];
        buffer.SetRenderTarget(target.texture);
        Vector2 pixelRect = new Vector2(target.texture.width, target.texture.height);
        buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
        foreach(int drawIndex in target.drawIndices) {
            buffer.DrawRenderer(renderer, projector, drawIndex);
        }
        lastUse = Time.time;
    }
    void OnDestroy() {
        PaintDecal.RemoveDecalableInfo(this);
        foreach(var pair in textureTargets) {
            foreach(Material mat in pair.Value.decalableMaterials) {
                mat.SetTexture(pair.Key, null);
            }
            pair.Value.texture.Release();
        }
    }
}

}
}