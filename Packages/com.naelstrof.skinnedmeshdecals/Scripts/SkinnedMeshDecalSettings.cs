using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

[System.Serializable]
public struct SkinnedMeshDecalSettings {
    [SerializeField]
    public ulong targetMemoryBudgetBits;
    [SerializeField]
    public float defaultTexelsPerMeter;
    [SerializeField]
    public DecalSettings defaultDecalSettings;

    public SkinnedMeshDecalSettings(float targetMemoryBudgetMB = 512f, string defaultDecalTexturePropertyName = "_DecalColorMap", float defaultTexelsPerMeter = 64f) {
        targetMemoryBudgetBits = (ulong)(targetMemoryBudgetMB * 8000000);
        this.defaultTexelsPerMeter = defaultTexelsPerMeter;
        defaultDecalSettings = new DecalSettings(defaultTexelsPerMeter, dilation: DilationType.Alpha, defaultDecalTexturePropertyName);
    }
}
