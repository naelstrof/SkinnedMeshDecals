using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshDecals {

public partial class MonoBehaviourHider {
internal class DecalCommandProcessor : MonoBehaviour {
    private static List<DecalCommand> decalCommands;
    private static CommandBuffer commandBuffer;
    private static DecalCommandProcessor instance;

    public static void EnsureInstanceAlive() {
        if (instance) return;
        var _ = new GameObject("DecalCommandProcessor", typeof(DecalCommandProcessor)) {
            hideFlags = HideFlags.HideAndDontSave
        };
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
        int stepSize = Mathf.Max(1, decalCommands.Count / SkinnedMeshDecalsSettings.MaxDecalsPerFrame);
        for (int i = 0; i < decalCommands.Count; i += stepSize) {
            try {
                commandBuffer.Clear();
                decalCommands[i].TryApply(commandBuffer);
                Graphics.ExecuteCommandBuffer(commandBuffer);
            } catch(Exception e) {
                decalCommands[i].Invalidate();
                Debug.LogException(e);
            }
        }

        if (stepSize != 1) {
            for (int i = decalCommands.Count - 1; i >= 0; i--) {
                if (!decalCommands[i].valid) {
                    decalCommands.RemoveAt(i);
                }
            }
            if (decalCommands.Count >= 2) {
                decalCommands.RemoveRange(decalCommands.Count / 2, decalCommands.Count / 2);
            }
        } else {
            decalCommands.Clear();
        }

        commandBuffer.Clear();
        DecalableRenderer.DoDilation(commandBuffer);
        Graphics.ExecuteCommandBuffer(commandBuffer);
        
        DecalableRenderer.TryHitTargetMemory(SkinnedMeshDecalsSettings.TargetMemoryBudgetBits);
    }

    internal static void AddDecalCommand(DecalCommand command) {
        decalCommands.Add(command);
    }
}

}

}
