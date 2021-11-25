using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {
public class DecalableInfo : MonoBehaviour {
    private static List<Material> staticDecalables = new List<Material>();
    private RenderTexture texture;
    private new Renderer renderer;
    private List<Material> decalableMaterials;
    private List<int> drawIndices;
    public float lastUse;
    public float memorySizeMB => texture != null ? (texture.width*texture.height*4f)/(float)(1e+6f) : 0f;
    public bool IsValid() => texture != null;
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
        decalableMaterials = new List<Material>();
        drawIndices = new List<int>();
        renderer = GetComponent<Renderer>();
        renderer.GetMaterials(staticDecalables);
        foreach(Material mat in staticDecalables) {
            if (PaintDecal.IsDecalable(mat)) {
                decalableMaterials.Add(mat);
            }
        }
        // If we've got nothing to decal to, we just quit out.
        if (decalableMaterials.Count <= 0) {
            return;
        }
        int worldScale = Mathf.RoundToInt(renderer.bounds.extents.magnitude*PaintDecal.instance.texelsPerMeter);
        int textureScale = Mathf.Clamp(CeilPowerOfTwo(worldScale), 16, 4096);
        texture = new RenderTexture(textureScale, textureScale, 0);
        PaintDecal.ClearRenderTexture(texture);

        foreach(Material mat in decalableMaterials) {
            mat.SetTexture("_DecalColorMap", texture);
        }
        lastUse = Time.time;

        int maxIndex = 1;
        if (renderer is MeshRenderer) {
            MeshFilter f = renderer.GetComponent<MeshFilter>();
            if (f != null) {
                maxIndex = f.sharedMesh.subMeshCount;
            }
        } else if (renderer is SkinnedMeshRenderer) {
            SkinnedMeshRenderer sr = (SkinnedMeshRenderer)renderer;
            maxIndex = sr.sharedMesh.subMeshCount;
        }
        for(int i=0;i<maxIndex;i++) {
            // There could be other materials on this renderer that aren't related to decals (usually eyeballs on characters)
            // We need to make sure we skip them.
            if (staticDecalables.Count == 0 || staticDecalables.Count <= i || !PaintDecal.IsDecalable(staticDecalables[i])) {
                continue;
            }
            drawIndices.Add(i);
        }
        PaintDecal.instance.AddDecalableInfo(this);
    }
    public void Render(CommandBuffer buffer, Material projector) {
        if (texture == null) {
            return;
        }
        buffer.SetRenderTarget(texture);
        Vector2 pixelRect = new Vector2(texture.width, texture.height);
        buffer.SetViewport(new Rect(Vector2.zero, pixelRect));
        foreach(int drawIndex in drawIndices) {
            buffer.DrawRenderer(renderer, projector, drawIndex);
        }
        lastUse = Time.time;
    }
    void OnDestroy() {
        PaintDecal.instance.RemoveDecalableInfo(this);
        foreach(Material mat in decalableMaterials) {
            mat.SetTexture("_DecalColorMap", null);
        }
        texture.Release();
    }
}

}