using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkinnedMeshDecals {

public enum DecalResolutionType {
    Auto,
    Custom,
}

[System.Serializable]
public struct DecalResolution : IEquatable<DecalResolution> {
    private const int minimumRenderTextureSizeAllowedByUnity = 16;

    private const float defaultTexelsPerMeter = 512f;
    [SerializeField] private DecalResolutionType m_ResolutionType;

    [SerializeField]
    private Vector2Int m_Size;

    private float m_TexelsPerMeter;

    public Vector2Int size {
        get => m_Size;
        set {
            m_Size = value;
            m_ResolutionType = DecalResolutionType.Custom;
        }
    }
    
    public float texelsPerMeter {
        get => m_TexelsPerMeter;
        set {
            m_TexelsPerMeter = value;
            m_ResolutionType = DecalResolutionType.Auto;
        }
    }

    public DecalResolutionType resolutionType {
        get => m_ResolutionType;
        set => m_ResolutionType = value;
    }

    /// <summary>
    /// Decal resolution is used to determine how large a texture should be created if it is missing during decal splatting.
    /// </summary>
    /// <param name="resolutionType">The only valid input here is DecalResolutionType.Auto, use a different overload if you want more control.</param>
    public DecalResolution(DecalResolutionType resolutionType) : this(Vector2Int.one * minimumRenderTextureSizeAllowedByUnity, defaultTexelsPerMeter, resolutionType) {
    }

    /// <summary>
    /// Decal resolution is used to determine how large a texture should be created if it is missing during decal splatting.
    /// </summary>
    /// <param name="size">The resolution of the texture.</param>
    public DecalResolution(Vector2Int size) : this(size, defaultTexelsPerMeter, DecalResolutionType.Custom) {
    }
    
    /// <summary>
    /// Decal resolution is used to determine how large a texture should be created if it is missing during decal splatting.
    /// </summary>
    /// <param name="texelsPerMeter">How many pixels per surface volume of the renderer's bounds in meters.</param>
    public DecalResolution(float texelsPerMeter) : this(Vector2Int.one * minimumRenderTextureSizeAllowedByUnity, texelsPerMeter, DecalResolutionType.Auto) {
    }
    

    internal DecalResolution(Vector2Int size, float texelsPerMeter, DecalResolutionType resolutionType) {
        m_ResolutionType = resolutionType;
        m_Size = size;
        m_TexelsPerMeter = texelsPerMeter;
    }

    public static bool operator ==(DecalResolution lhs, DecalResolution rhs) {
        return lhs.m_Size == rhs.m_Size && lhs.m_ResolutionType == rhs.m_ResolutionType;
    }

    public static bool operator !=(DecalResolution lhs, DecalResolution rhs) => !(lhs == rhs);

    public static implicit operator DecalResolution(DecalResolutionType keyword) => new DecalResolution(keyword);
    public static implicit operator DecalResolution(Vector2Int resolution) => new DecalResolution(resolution);

    public bool Equals(DecalResolution other) => other == this;

    public override bool Equals(object obj) => obj is DecalResolution other && this.Equals(other);

    public override int GetHashCode() {
        return HashCode.Combine((int)m_ResolutionType, m_Size);
    }

    public override string ToString() => m_ResolutionType == DecalResolutionType.Custom
        ? $"{{Decal Resolution: {m_Size}}}"
        : "{Decal Resolution: AUTO}";
}

}