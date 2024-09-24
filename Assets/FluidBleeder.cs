using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Rendering;

public class FluidBleeder : MonoBehaviour {
    [SerializeField]
    private Material materialBleeder;
    private List<DecalTextureRendererPair> pairs = new ();
    private CommandBuffer cmd;
    private int temporaryTexture = Shader.PropertyToID("_FluidBleedTexture");
    private int bleederInputTexture = Shader.PropertyToID("_FluidInput");
    [SerializeField]
    private Material blitCopy;

    private void Awake() {
        cmd = new CommandBuffer();
    }

    void Update() {
        cmd.Clear();
        PaintDecal.GetDecalTextures(pairs);
        foreach (var pair in pairs) {
            var descriptor = pair.texture.descriptor;
            cmd.GetTemporaryRT(temporaryTexture, pair.texture.descriptor);
            cmd.SetRenderTarget(temporaryTexture);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.SetGlobalTexture(bleederInputTexture, pair.texture);
            cmd.DrawRenderer(pair.renderer, materialBleeder);
            cmd.Blit(temporaryTexture, pair.texture);
            cmd.GenerateMips(pair.texture);
            cmd.ReleaseTemporaryRT(temporaryTexture);
        }
        Graphics.ExecuteCommandBuffer(cmd);
    }
}
