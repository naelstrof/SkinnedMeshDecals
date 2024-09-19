using System;
using UnityEngine;
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
    private static Texture2D defaultTexture;
    private static readonly int Power = Shader.PropertyToID("_Power");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize() {
        textureAlpha = new Material(Shader.Find("Naelstrof/DecalProjectorAlphaBlend"));
        textureAlpha.EnableKeyword("_BACKFACECULLING_ON");
        
        textureSubtractive = new Material(Shader.Find("Naelstrof/DecalProjectorSubtractiveBlend")) {
            color = Color.black
        };
        textureSubtractive.EnableKeyword("_BACKFACECULLING_ON");
        
        sphereAlpha = new Material(Shader.Find("Naelstrof/SphereProjectorAlphaBlend"));
        sphereSubtractive = new Material(Shader.Find("Naelstrof/SphereProjectorSubtractiveBlend")) {
            color = Color.black
        };
        
        // Create a box texture as an example.
        defaultTexture = Object.Instantiate(Texture2D.whiteTexture);
        var pixels = defaultTexture.GetPixels();
        for (int x = 0; x < defaultTexture.width; x++) {
            for (int y = 0; y < defaultTexture.height; y++) {
                if (x == 0 || y == 0 || x == defaultTexture.width - 1 || y == defaultTexture.height - 1) {
                    pixels[x + y * defaultTexture.width] = Color.clear;
                }
            }
        }
        defaultTexture.SetPixels(pixels, 0);
        defaultTexture.wrapMode = TextureWrapMode.Clamp;
        defaultTexture.Apply();
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
    public DecalProjector(Material v) : this(v, null, null, DecalProjectorType.Custom, false) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a specific texture and color.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAlpha: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAlpha: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    /// <param name="texture">The texture to use for Texture based projectors</param>
    /// <param name="color">The color of the projector.</param>
    /// <param name="backfaceCulling">If backfaces should be ignored during painting.</param>
    public DecalProjector(DecalProjectorType projectorType, Texture texture, Color? color = null, bool backfaceCulling = false) : this(null, texture, color, projectorType, backfaceCulling) {
    }

    /// <summary>
    /// Gets a shared material with the specified projector type, set with a specific color.
    /// </summary>
    /// <param name="projectorType">
    /// TextureAlpha: A texture-based projector that adds color to the decal map.
    /// TextureSubtractive: A texture-based projector that removes alpha from the decal map based on the alpha of the texture.
    /// SphereAlpha: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.</param>
    /// <param name="color">The color of the projector.</param>
    /// <param name="backfaceCulling">If backfaces should be ignored during painting.</param>
    public DecalProjector(DecalProjectorType projectorType, Color? color = null, bool backfaceCulling = false) : this(null, defaultTexture, color, projectorType, backfaceCulling) {
    }
    
    /// <summary>
    /// Gets a sphere projector with a specific power setting.
    /// </summary>
    /// <param name="projectorType">Must be one of the following:
    /// SphereAlpha: A pure shader that adds color based on the distance from the "center" of the DecalProjection.
    /// SphereSubtractive: A pure shader that removes alpha based on the distance from the "center" of the DecalProjection.
    /// </param>
    /// <param name="power">How strong the sphere projection edge is.</param>
    /// <param name="color">The color to blit.</param>
    /// <param name="backfaceCulling">If backfaces should be ignored during painting.</param>
    public DecalProjector(DecalProjectorType projectorType, float power, Color? color, bool backfaceCulling = false) : this(null, defaultTexture, color, projectorType, backfaceCulling) {
        Assert.IsTrue(projectorType is DecalProjectorType.SphereAlpha or DecalProjectorType.SphereSubtractive);
        m_Material.SetFloat(Power, power);
    }

    internal DecalProjector(Material v, Texture texture, Color? color, DecalProjectorType projectorType, bool backfaceCulling) {
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

        if (backfaceCulling) {
            m_Material.EnableKeyword("_BACKFACECULLING_ON");
        } else {
            m_Material.DisableKeyword("_BACKFACECULLING_ON");
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