using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkinnedMeshDecals {

public class VerifyIncludedShaders : AssetPostprocessor {
#if UNITY_EDITOR
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        Initialize();
    }

    private static void Initialize() {
        TryAddAlwaysIncludedShader("Naelstrof/DecalProjectorAlphaBlend");
        TryAddAlwaysIncludedShader("Naelstrof/DecalProjectorSubtractiveBlend");
        TryAddAlwaysIncludedShader("Naelstrof/DecalProjectorAdditiveBlend");
        TryAddAlwaysIncludedShader("Hidden/Naelstrof/DilationShaderAlpha");
        TryAddAlwaysIncludedShader("Hidden/Naelstrof/DilationShaderAdditive");
        TryAddAlwaysIncludedShader("Naelstrof/SphereProjectorAlphaBlend");
        TryAddAlwaysIncludedShader("Naelstrof/SphereProjectorSubtractiveBlend");
        TryAddAlwaysIncludedShader("Naelstrof/SphereProjectorAdditiveBlend");
    }
    private static bool TryAddAlwaysIncludedShader(string shaderName) {
        var shader = Shader.Find(shaderName);
        if (shader == null) {
            Debug.LogError($"SkinnedMeshDecals: Couldn't find shader {shaderName}... Try reimporting?");
            return false;
        }

        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsObj == null) {
            Debug.LogError($"SkinnedMeshDecals: Couldn't load graphics settings from path ProjectSettings/GraphicsSettings.asset. It was missing! Make sure to press Ctrl+S, then try reimporting SkinnedMeshDecals.");
            return false;
        }
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        if (arrayProp == null) {
            Debug.LogError($"SkinnedMeshDecals: Couldn't add shaders to the AlwaysIncludedShaders list, property was missing... Try reimporting?");
            return false;
        }
        bool hasShader = false;
        for (int i = 0; i < arrayProp.arraySize; ++i) {
            var arrayElem = arrayProp.GetArrayElementAtIndex(i);
            if (shader == arrayElem.objectReferenceValue) {
                hasShader = true;
                break;
            }
        }
        if (!hasShader) {
            int arrayIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(arrayIndex);
            var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
            arrayElem.objectReferenceValue = shader;
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return true;
        }
        return false;
    }
#endif
}



}
