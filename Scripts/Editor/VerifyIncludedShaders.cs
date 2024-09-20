using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkinnedMeshDecals {

public static class VerifyIncludedShaders {
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void Initialize() {
        TryAddAlwaysIncludedShader("Naelstrof/DecalProjectorAlphaBlend");
        TryAddAlwaysIncludedShader("Naelstrof/DecalProjectorSubtractiveBlend");
        TryAddAlwaysIncludedShader("Hidden/Naelstrof/DilationShader");
        TryAddAlwaysIncludedShader("Naelstrof/SphereProjectorAlphaBlend");
        TryAddAlwaysIncludedShader("Naelstrof/SphereProjectorSubtractiveBlend");
    }
    private static bool TryAddAlwaysIncludedShader(string shaderName) {
        var shader = Shader.Find(shaderName);
        if (shader == null) {
            Debug.LogError($"SkinnedMeshDecals: Couldn't find shader {shaderName}... Try reimporting?");
            return false;
        }

        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsObj == null) {
            Debug.LogError($"SkinnedMeshDecals: Couldn't load graphics settings from path ProjectSettings/GraphicsSettings.asset. It was missing!");
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
