using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkinnedMeshDecals {

public static class AtlasPacker {

    class MeshAtlasRect {
        public Mesh mesh;
        public Rect rect;
        public float scale;
    }

    public static void Pack(Renderer[] renderers) {
        var atlasRects = new List<MeshAtlasRect>();
        foreach (var renderer in renderers) {
            atlasRects.Add(new MeshAtlasRect() {
                mesh = null,
                rect = new Rect(0f,0f,1f,1f),
                scale = MonoBehaviourHider.DecalableRenderer.GetSurfaceArea(renderer)
            });
            if (renderer is MeshRenderer meshRenderer) {
                var mesh = Mesh.Instantiate(meshRenderer.GetComponent<MeshFilter>().sharedMesh);
                meshRenderer.GetComponent<MeshFilter>().sharedMesh = mesh;
                atlasRects[^1].mesh = mesh;
            }
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer) {
                var mesh = Mesh.Instantiate(skinnedMeshRenderer.sharedMesh);
                skinnedMeshRenderer.sharedMesh = mesh;
                atlasRects[^1].mesh = mesh;
            }
        }
        var totalArea = 0f;
        foreach (var atlasRect in atlasRects) {
            totalArea += atlasRect.scale * atlasRect.scale;
        }
        foreach (var atlasRect in atlasRects) {
            atlasRect.scale *= 0.8f/Mathf.Sqrt(totalArea);
            atlasRect.rect = new Rect(0f, 0f, atlasRect.scale, atlasRect.scale);
        }
        atlasRects.Sort((a, b) => b.scale.CompareTo(a.scale));
        var freeRects = new List<Rect>() { new Rect(0f,0f,1f,1f) };
        for (var index = 0; index < atlasRects.Count; index++) {
            var atlasRect = atlasRects[index];
            // TODO: SORT FREERECTS
            if (!PlaceRect(ref freeRects, ref atlasRect))
                throw new UnityException("Unable to atlas meshes.. They didn't fit somehow..");
        }
        foreach (var atlasRect in atlasRects) {
            var uv2 = atlasRect.mesh.uv2;
            for (int i = 0; i < uv2.Length; i++) {
                uv2[i] = new Vector2(
                    uv2[i].x.Remap(0f,1f,atlasRect.rect.xMin, atlasRect.rect.xMax),
                    uv2[i].y.Remap(0f,1f,atlasRect.rect.yMin, atlasRect.rect.yMax)
                );
            }
            atlasRect.mesh.uv2 = uv2;
        }
    }
    
    private static float Remap(this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    static bool PlaceRect(ref List<Rect> freeRects, ref MeshAtlasRect meshAtlasRect, int failsafe = 10) {
        if (failsafe == 0) return false;
        for (var index = 0; index < freeRects.Count; index++) {
            if (freeRects[index].width < meshAtlasRect.rect.width
                || freeRects[index].height < meshAtlasRect.rect.height) continue;
            meshAtlasRect.rect = new Rect(
                freeRects[index].x,
                freeRects[index].y,
                meshAtlasRect.rect.width,
                meshAtlasRect.rect.height
            );
            var remainingWidth = freeRects[index].width - meshAtlasRect.rect.width;
            var remainingHeight = freeRects[index].height - meshAtlasRect.rect.height;
            if (remainingWidth > remainingHeight) {
                if (remainingHeight > 0) {
                    freeRects.Add(new Rect(
                        freeRects[index].x,
                        freeRects[index].y + meshAtlasRect.rect.height,
                        meshAtlasRect.rect.width,
                        remainingHeight
                    ));
                }
                if (remainingWidth > 0) {
                    freeRects.Add(new Rect(
                        freeRects[index].x+meshAtlasRect.rect.width,
                        freeRects[index].y,
                        remainingWidth,
                        freeRects[index].height
                    ));
                }
                freeRects.RemoveAt(index);
            } else {
                if (remainingWidth > 0) {
                    freeRects.Add(new Rect(
                        freeRects[index].x + meshAtlasRect.rect.width,
                        freeRects[index].y,
                        remainingWidth,
                        meshAtlasRect.rect.height
                    ));
                }
                if (remainingHeight > 0) {
                    freeRects.Add(new Rect(
                        freeRects[index].x,
                        freeRects[index].y + meshAtlasRect.rect.height,
                        freeRects[index].width,
                        remainingHeight
                    ));
                }
                freeRects.RemoveAt(index);
            }
            return true;
        }
        meshAtlasRect.rect = new Rect(
            meshAtlasRect.rect.x,
            meshAtlasRect.rect.y,
            meshAtlasRect.rect.width * 0.5f,
            meshAtlasRect.rect.height * 0.5f
            );
        return PlaceRect(ref freeRects, ref meshAtlasRect, failsafe--);
    }

}

}