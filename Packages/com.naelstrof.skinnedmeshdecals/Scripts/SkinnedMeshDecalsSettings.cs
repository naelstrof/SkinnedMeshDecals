using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkinnedMeshDecals {

public class SkinnedMeshDecalsSettings : ScriptableObject {
    private static SkinnedMeshDecalsSettings settings;
    
    [Tooltip("The target VRAM usage in megabytes. If textures go over this, we release one of the oldest.")]
    [SerializeField] public float defaultTargetGraphicsMemoryUsageMB = 512f;
    [Tooltip("The default shader property name that is a texture where decals get blitted into.")]
    [SerializeField] public string defaultTextureName = "_DecalColorMap";
    [Tooltip("Default resolution type, Auto tries to estimate surface area and allocate an appropriately sized power of 2 texture.")]
    [SerializeField] public DecalResolutionType defaultResolutionType = DecalResolutionType.Auto;
    [Tooltip("Resolution of created textures if the type is set to Custom.")]
    [SerializeField] public Vector2Int defaultResolution = Vector2Int.one * 512;
    [Tooltip("Resolution of created textures if the type is set to Auto.")]
    [SerializeField] public float defaultTexelsPerMeter = 64f;
    [Tooltip("Which format should render textures default to?")]
    [SerializeField] public RenderTextureFormat defaultRenderTextureFormat = RenderTextureFormat.Default;
    [Tooltip("If the render texture should be Linear or Gamma encoded.")]
    [SerializeField] public RenderTextureReadWrite defaultRenderTextureReadWrite = RenderTextureReadWrite.Default;
    [Tooltip("If the textures should have a secondary dilation blit to fix UV seams, and if so, which dilation strategy to use.")]
    [SerializeField] public DilationType defaultDilationType = DilationType.Alpha;

    [NonSerialized] private DecalSettings defaultSettings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize() {
        settings = null;
    }

    public static ulong TargetMemoryBudgetBits => (ulong)(instance.defaultTargetGraphicsMemoryUsageMB * 8000000L);
    public static DecalSettings DefaultDecalSettings => instance.defaultSettings;
    private static SkinnedMeshDecalsSettings instance {
        get {
            if (settings != null) return settings;
            
            settings = Resources.Load<SkinnedMeshDecalsSettings>(nameof(SkinnedMeshDecalsSettings));
                
            if (settings == null) {
                settings = CreateInstance<SkinnedMeshDecalsSettings>();
#if UNITY_EDITOR
                if (!AssetDatabase.IsValidFolder($"Assets/{nameof(SkinnedMeshDecals)}/")) {
                    AssetDatabase.CreateFolder("Assets", $"{nameof(SkinnedMeshDecals)}");
                }

                if (!AssetDatabase.IsValidFolder($"Assets/{nameof(SkinnedMeshDecals)}/Resources/")) {
                    AssetDatabase.CreateFolder($"Assets/{nameof(SkinnedMeshDecals)}", "Resources");
                }

                AssetDatabase.CreateAsset(settings, $"Assets/{nameof(SkinnedMeshDecals)}/Resources/{nameof(SkinnedMeshDecalsSettings)}.asset");
#endif
            }
                
            settings.defaultSettings = new DecalSettings(
                decalResolution: new DecalResolution(size: settings.defaultResolution,
                    texelsPerMeter: settings.defaultTexelsPerMeter, resolutionType: settings.defaultResolutionType),
                dilation: settings.defaultDilationType,
                textureName: settings.defaultTextureName,
                renderTextureFormat: settings.defaultRenderTextureFormat,
                renderTextureReadWrite: settings.defaultRenderTextureReadWrite);

            return settings;
        }
    }
}

}
