using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkinnedMeshDecals {
#if UNITY_EDITOR
[CustomEditor(typeof(SkinnedMeshDecalsConfiguration))]
public class SkinnedMeshDecalsConfigurationEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("Memory in use: " + PaintDecal.memoryInUsage + " MB", MessageType.Info);
        DrawDefaultInspector();
    }
}
#endif
public class SkinnedMeshDecalsConfiguration : MonoBehaviour {
    [Range(32,1024)]
    [Tooltip("Memory usage in megabytes before old textures get removed.")]
    public float memoryBudget = 512;
    [Range(32,4096)]
    [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
    public int texelsPerMeter = 512;
    void Start() {
        PaintDecal.memoryBudget = memoryBudget;
        PaintDecal.texelsPerMeter = texelsPerMeter;
    }
    void OnValidate() {
        Start();
    }
}

}