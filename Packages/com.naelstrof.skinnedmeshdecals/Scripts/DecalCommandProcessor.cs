using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {

public class DecalCommandProcessor : MonoBehaviour {
    private static List<DecalCommand> decalCommands;
    private static CommandBuffer commandBuffer;
    private static DecalCommandProcessor instance;

    public static void EnsureInstanceAlive() {
        if (instance) return;
        new GameObject("DecalCommandProcessor", typeof(DecalCommandProcessor));
    }

    private void Awake() {
        if (instance != this && instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize() {
        commandBuffer = new CommandBuffer();
        decalCommands = new List<DecalCommand>();
    }
    
    private void Update() {
        commandBuffer.Clear();
        foreach (var decalCommand in decalCommands) {
            decalCommand.TryApply(commandBuffer);
        }
        decalCommands.Clear();
        Graphics.ExecuteCommandBuffer(commandBuffer);
        MonoBehaviourHider.DecalableRenderer.TryHitTargetMemory(SkinnedMeshDecalsSettings.TargetMemoryBudgetBits);
    }
    
    internal static void AddDecalCommand(DecalCommand command) {
        decalCommands.Add(command);
    }
    
}

}
