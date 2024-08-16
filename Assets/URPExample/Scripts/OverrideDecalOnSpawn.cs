using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Rendering;

public class OverrideDecalOnSpawn : MonoBehaviour {
    void Start() {
        RenderTexture newTexture = new RenderTexture(512, 512, 24) {
            autoGenerateMips = false,
            useMipMap = true,
        };
        CommandBuffer buffer = new CommandBuffer();
        buffer.SetRenderTarget(newTexture);
        buffer.ClearRenderTarget(true, true, Color.green);
        buffer.GenerateMips(newTexture);
        Graphics.ExecuteCommandBuffer(buffer);
        foreach (var thing in GetComponentsInChildren<Renderer>()) {
            PaintDecal.OverrideDecalTexture(thing, newTexture);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
