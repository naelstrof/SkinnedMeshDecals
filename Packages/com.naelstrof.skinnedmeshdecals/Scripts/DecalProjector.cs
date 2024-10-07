using System;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SkinnedMeshDecals {

public enum DecalProjectorType {
    TextureAlpha,
    TextureAdditive,
    TextureSubtractive,
    SphereAlpha,
    SphereAdditive,
    SphereSubtractive,
    Custom,
}

[System.Serializable]
public struct DecalProjector : IEquatable<DecalProjector> {
    [SerializeField] private DecalProjectorType m_ProjectorType;
    [SerializeField] private Material m_Material;
    private Color? m_Color;
    private float? m_Power;
    private Texture m_MainTexture;

    private static Material textureAlpha;
    private static Material textureAdditive;
    private static Material textureSubtractive;
    private static Material sphereAlpha;
    private static Material sphereAdditive;
    private static Material sphereSubtractive;
    
    private static Material textureAlphaBackfaceCulling;
    private static Material textureAdditiveBackfaceCulling;
    private static Material textureSubtractiveBackfaceCulling;
    private static Material sphereAlphaBackfaceCulling;
    private static Material sphereAdditiveBackfaceCulling;
    private static Material sphereSubtractiveBackfaceCulling;
    private static Texture2D defaultTexture;
    private static readonly int Power = Shader.PropertyToID("_Power");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize() {
        var alphaShader = Shader.Find("Naelstrof/DecalProjectorAlphaBlend");
        if (alphaShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/DecalProjectorAlphaBlend, ensure SMD is imported correctly.");
        }
        textureAlpha = new Material(alphaShader);
        textureAlpha.DisableKeyword("_BACKFACECULLING_ON");
        
        textureAlphaBackfaceCulling = new Material(alphaShader);
        textureAlphaBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");
        
        var additiveShader = Shader.Find("Naelstrof/DecalProjectorAdditiveBlend");
        if (additiveShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/DecalProjectorAdditiveBlend, ensure SMD is imported correctly.");
        }
        textureAdditive = new Material(additiveShader);
        textureAdditive.DisableKeyword("_BACKFACECULLING_ON");
        
        textureAdditiveBackfaceCulling = new Material(additiveShader);
        textureAdditiveBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");

        var subtractiveShader = Shader.Find("Naelstrof/DecalProjectorSubtractiveBlend");
        if (subtractiveShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/DecalProjectorSubtractiveBlend, ensure SMD is imported correctly.");
        }
        textureSubtractive = new Material(subtractiveShader) {
            color = Color.black
        };
        textureSubtractive.DisableKeyword("_BACKFACECULLING_ON");
        textureSubtractiveBackfaceCulling = new Material(subtractiveShader) {
            color = Color.black
        };
        textureSubtractiveBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");
        
        var sphereShader = Shader.Find("Naelstrof/SphereProjectorAlphaBlend");
        if (sphereShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/SphereProjectorAlphaBlend, ensure SMD is imported correctly.");
        }
        sphereAlpha = new Material(sphereShader);
        sphereAlpha.DisableKeyword("_BACKFACECULLING_ON");
        
        sphereAlphaBackfaceCulling = new Material(sphereShader);
        sphereAlphaBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");
        
        var sphereAdditiveShader = Shader.Find("Naelstrof/SphereProjectorAdditiveBlend");
        if (sphereAdditiveShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/SphereProjectorAdditiveBlend, ensure SMD is imported correctly.");
        }

        sphereAdditive = new Material(sphereAdditiveShader);
        sphereAdditive.DisableKeyword("_BACKFACECULLING_ON");
        
        sphereAdditiveBackfaceCulling = new Material(sphereAdditiveShader);
        sphereAdditiveBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");
        var sphereSubtractiveShader = Shader.Find("Naelstrof/SphereProjectorSubtractiveBlend");
        if (sphereSubtractiveShader == null) {
            throw new UnityException( "SkinnedMeshDecals: Failed to find shader Naelstrof/SphereProjectorSubtractiveBlend, ensure SMD is imported correctly.");
        }
        sphereSubtractive = new Material(sphereSubtractiveShader) {
            color = Color.black
        };
        sphereSubtractive.DisableKeyword("_BACKFACECULLING_ON");
        sphereSubtractiveBackfaceCulling = new Material(sphereSubtractiveShader) {
            color = Color.black
        };
        sphereSubtractiveBackfaceCulling.EnableKeyword("_BACKFACECULLING_ON");
        
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
        get {
            if (m_ProjectorType != DecalProjectorType.Custom) {
                if (m_Color != null) {
                    m_Material.SetColor(ColorProperty, m_Color.Value);
                }
                if (m_MainTexture) {
                    m_Material.SetTexture(MainTex, m_MainTexture);
                }
                if (m_Power != null) {
                    m_Material.SetFloat(Power, m_Power.Value);
                }
            }
            return m_Material;
        }
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
        m_Power = power;
    }

    internal DecalProjector(Material v, Texture texture, Color? color, DecalProjectorType projectorType, bool backfaceCulling) {
        m_ProjectorType = projectorType;
        
        m_Material = m_ProjectorType switch {
            DecalProjectorType.TextureAlpha => backfaceCulling ? textureAlphaBackfaceCulling : textureAlpha,
            DecalProjectorType.TextureAdditive => backfaceCulling ? textureAdditiveBackfaceCulling : textureAdditive,
            DecalProjectorType.TextureSubtractive => backfaceCulling ? textureSubtractiveBackfaceCulling : textureSubtractive,
            DecalProjectorType.SphereAlpha => backfaceCulling ? sphereAlphaBackfaceCulling : sphereAlpha,
            DecalProjectorType.SphereAdditive => backfaceCulling ? sphereAdditiveBackfaceCulling : sphereAdditive,
            DecalProjectorType.SphereSubtractive => backfaceCulling ? sphereSubtractiveBackfaceCulling : sphereSubtractive,
            DecalProjectorType.Custom => v,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        m_MainTexture = texture;
        m_Color = color;
        m_Power = null;

    }

    public static bool operator ==(DecalProjector lhs, DecalProjector rhs) {
        return lhs.m_ProjectorType == rhs.m_ProjectorType && lhs.m_Material == rhs.m_Material;
    }

    public static bool operator !=(DecalProjector lhs, DecalProjector rhs) => !(lhs == rhs);

    public static implicit operator DecalProjector(DecalProjectorType keyword) => new(keyword);

    public static implicit operator DecalProjector(Texture texture) => new(DecalProjectorType.TextureAlpha, texture);
    public static implicit operator DecalProjector(Material v) => new(v);

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