using System;
using UnityEngine;

namespace SkinnedMeshDecals {

public enum DilationType {
    None,
    Alpha,
    Additive,
}

[System.Serializable]
public struct DecalSettings : IEquatable<DecalSettings> {
    private const string defaultTextureName = "_DecalColorMap";
    public static readonly DecalSettings Default = new DecalSettings(DecalResolutionType.Auto);

    [Tooltip("The texture id used in decalable shaders.")] [SerializeField]
    private int m_textureID;

    [Tooltip("The texture type that will be created if a new decal map is created from this decal splat.")]
    [SerializeField]
    private RenderTextureFormat m_renderTextureFormat;

    [Tooltip("The texture color mode that will be used on new decal maps created from this decal splat.")]
    [SerializeField]
    private RenderTextureReadWrite m_renderTextureReadWrite;

    [SerializeField] private DecalResolution m_resolution;
    
    [SerializeField] private DilationType m_dilation;

    public int textureID => m_textureID;
    public RenderTextureFormat renderTextureFormat => m_renderTextureFormat;
    public RenderTextureReadWrite renderTextureReadWrite => m_renderTextureReadWrite;
    public DecalResolution resolution => m_resolution;
    public DilationType dilation => m_dilation;

    /// <summary>
    /// Creates a set of settings used to paint a single decal. It uses the texture name to determine if a texture
    /// needs to be created for the hit material, and uses the provided formats to generate it if it doesn't exist.
    /// </summary>
    /// <param name="decalResolution">How many pixels to dedicate to the decal map of the hit object. If set to DecalResolutionType.Auto, we'll estimate it based on the surface area of the world bounds of the object.</param>
    /// <param name="dilation">If the decals should get dilated, this fixes issues with uv seams, but causes a blur effect at the edges as a cost.</param>
    /// <param name="textureName">The name of the texture input on the renderer's material where the decal map will be placed.</param>
    /// <param name="renderTextureFormat">If a decal map is created from this splat, this is the format it will be created with.</param>
    /// <param name="renderTextureReadWrite">If a decal map is created from this splat, this is the read/write format it will be created with.</param>
    public DecalSettings(DecalResolution decalResolution = default, DilationType dilation = DilationType.Alpha, string textureName = defaultTextureName,
        RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default,
        RenderTextureReadWrite renderTextureReadWrite = RenderTextureReadWrite.Default) {
        m_textureID = Shader.PropertyToID(textureName);
        m_renderTextureFormat = renderTextureFormat;
        m_renderTextureReadWrite = renderTextureReadWrite;
        m_resolution = decalResolution;
        m_dilation = dilation;
    }

    public static bool operator ==(DecalSettings lhs, DecalSettings rhs) {
        return lhs.m_textureID == rhs.m_textureID && lhs.m_renderTextureFormat == rhs.m_renderTextureFormat &&
               lhs.m_renderTextureReadWrite == rhs.m_renderTextureReadWrite;
    }

    public static bool operator !=(DecalSettings lhs, DecalSettings rhs) => !(lhs == rhs);

    public static implicit operator DecalSettings(DecalResolution resolution) => new DecalSettings(resolution);

    public static implicit operator DecalSettings(string texturename) =>
        new DecalSettings(DecalResolutionType.Auto, DilationType.Alpha, texturename);

    public bool Equals(DecalSettings other) => other == this;

    public override bool Equals(object obj) => obj is DecalSettings other && this.Equals(other);

    public override int GetHashCode() {
        return HashCode.Combine(m_textureID, (int)m_renderTextureFormat, (int)m_renderTextureReadWrite);
    }

    public override string ToString() =>
        $"{{Decal Settings:\ttextureID:{m_textureID}\trenderTextureFormat:{m_renderTextureFormat}\trenderTextureReadWrite:{m_renderTextureReadWrite}}}";
}

}