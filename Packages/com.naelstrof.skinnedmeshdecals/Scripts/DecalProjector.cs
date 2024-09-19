using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SkinnedMeshDecals {

public enum DecalProjectorType {
    TextureAlpha,
    TextureSubtractive,
    SphereAlpha,
    SphereSubtractive,
    Custom,
}

[System.Serializable]
public struct DecalProjector : IEquatable<DecalProjector> {
    [SerializeField] private DecalProjectorType m_ProjectorType;
    [SerializeField] private Material m_Material;

    private static Material textureAlpha;
    private static Material textureSubtractive;
    private static Material sphereAlpha;
    private static Material sphereSubtractive;
    private static Texture defaultTexture;
    private static readonly int Power = Shader.PropertyToID("_Power");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize() {
        var textureAlphaHandle = Addressables.LoadAssetAsync<Material>( "Packages/com.naelstrof.skinnedmeshdecals/ExampleProjectors/Splat.mat");
        var textureSubtractiveHandle = Addressables.LoadAssetAsync<Material>( "Packages/com.naelstrof.skinnedmeshdecals/ExampleProjectors/SplatErase.mat");
        var sphereAlphaHandle = Addressables.LoadAssetAsync<Material>( "Packages/com.naelstrof.skinnedmeshdecals/ExampleProjectors/SphereSplat.mat");
        var sphereSubtractiveHandle = Addressables.LoadAssetAsync<Material>( "Packages/com.naelstrof.skinnedmeshdecals/ExampleProjectors/SphereErase.mat");
        var textureHandle = Addressables.LoadAssetAsync<Texture>("Packages/com.naelstrof.skinnedmeshdecals/Textures/Splat_Splat_basecolor.png");
        textureAlpha = Object.Instantiate(textureAlphaHandle.WaitForCompletion());
        textureSubtractive = Object.Instantiate(textureSubtractiveHandle.WaitForCompletion());
        sphereAlpha = Object.Instantiate(sphereAlphaHandle.WaitForCompletion());
        sphereSubtractive = Object.Instantiate(sphereSubtractiveHandle.WaitForCompletion());
        defaultTexture = textureHandle.WaitForCompletion();
        
        Addressables.Release(textureAlphaHandle);
        Addressables.Release(textureSubtractiveHandle);
        Addressables.Release(sphereAlphaHandle);
        Addressables.Release(sphereSubtractiveHandle);
        Addressables.Release(textureHandle);
    }

    public Material material {
        get => m_Material;
        set {
            m_Material = value;
            m_ProjectorType = DecalProjectorType.Custom;
        }
    }

    public DecalProjectorType projectorType {
        get => m_ProjectorType;
        set => m_ProjectorType = value;
    }

    /// <summary>
    /// A custom projector material.
    /// </summary>
    /// <param name="v">A material that projects decals onto meshes. In order to create your own, simply use one of the provided projector shaders,
    /// or use the included Amplify Template (within Addons) to generate your own projection shader.</param>
    public DecalProjector(Material v) : this(v, null, null, DecalProjectorType.Custom) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a default texture and a white color.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAdditive: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAdditive: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    public DecalProjector(DecalProjectorType projectorType) : this(null, defaultTexture, Color.white, projectorType) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a specific texture.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAdditive: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAdditive: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    /// <param name="texture">The texture to use for Texture based projectors</param>
    public DecalProjector(DecalProjectorType projectorType, Texture texture) :
        this(null, texture, Color.white, projectorType) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a specific texture and color.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAdditive: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAdditive: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    /// <param name="texture">The texture to use for Texture based projectors</param>
    /// <param name="color">The color of the projector.</param>
    public DecalProjector(DecalProjectorType projectorType, Texture texture, Color color) : this(null, texture, color, projectorType) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a specific color.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAdditive: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAdditive: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    /// <param name="color">The color of the projector.</param>
    public DecalProjector(DecalProjectorType projectorType, Color color) : this(null, defaultTexture, color, projectorType) {
    }
    
    public DecalProjector(DecalProjectorType projectorType, Color color, float power) : this(null, defaultTexture, color, projectorType) {
        Assert.IsTrue(projectorType is DecalProjectorType.SphereAlpha or DecalProjectorType.SphereSubtractive);
        m_Material.SetFloat(Power, power);
    }

    internal DecalProjector(Material v, Texture texture, Color? color, DecalProjectorType projectorType) {
        m_ProjectorType = projectorType;
        
        m_Material = m_ProjectorType switch {
            DecalProjectorType.TextureAlpha => textureAlpha,
            DecalProjectorType.TextureSubtractive => textureSubtractive,
            DecalProjectorType.SphereAlpha => sphereAlpha,
            DecalProjectorType.SphereSubtractive => sphereSubtractive,
            DecalProjectorType.Custom => v,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (texture != null) {
            m_Material.mainTexture = texture;
        }

        if (color != null) {
            m_Material.color = color.Value;
        }
    }

    public static bool operator ==(DecalProjector lhs, DecalProjector rhs) {
        return lhs.m_ProjectorType == rhs.m_ProjectorType && lhs.m_Material == rhs.m_Material;
    }

    public static bool operator !=(DecalProjector lhs, DecalProjector rhs) => !(lhs == rhs);

    public static implicit operator DecalProjector(DecalProjectorType keyword) => new DecalProjector(keyword);

    public static implicit operator DecalProjector(Texture texture) =>
        new DecalProjector(DecalProjectorType.TextureAlpha, texture);

    public static implicit operator DecalProjector(Material v) => new DecalProjector(v);

    public bool Equals(DecalProjector other) => other == this;

    public override bool Equals(object obj) => obj is DecalProjector other && this.Equals(other);

    public override int GetHashCode() {
        return (int)((DecalProjectorType)(this.m_Material.GetHashCode() * 397) ^ this.m_ProjectorType);
    }

    public override string ToString() => m_ProjectorType == DecalProjectorType.Custom
        ? $"{{Custom Decal Projector:{m_Material}}}"
        : m_ProjectorType.ToString();
}

}