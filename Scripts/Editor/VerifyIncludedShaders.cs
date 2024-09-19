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
        AddAlwaysIncludedShader("Naelstrof/DecalProjectorAlphaBlend");
        AddAlwaysIncludedShader("Naelstrof/DecalProjectorSubtractiveBlend");
        AddAlwaysIncludedShader("Hidden/Naelstrof/DilationShader");
        AddAlwaysIncludedShader("Naelstrof/SphereProjectorAlphaBlend");
        AddAlwaysIncludedShader("Naelstrof/SphereProjectorSubtractiveBlend");
    }
    private static void AddAlwaysIncludedShader(string shaderName) {
        var shader = Shader.Find(shaderName);
        if (shader == null)
            throw new UnityException($"Couldn't find shader {shaderName}");

        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
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
        }
    }
#endif
}

}
