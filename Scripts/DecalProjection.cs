using System;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct DecalProjection : IEquatable<DecalProjection> {
    public enum DecalProjectionType {
        Orthographic,
        Perspective,
        Cube,
        Sphere,
        Custom,
    }
    [SerializeField] private DecalProjectionType m_ProjectionType;
    [SerializeField] private Matrix4x4 m_Projection;
    [SerializeField] private Matrix4x4 m_View;

    public Matrix4x4 projection {
        get => m_Projection;
        set {
            m_Projection = value;
            m_ProjectionType = DecalProjectionType.Custom;
        }
    }
    
    public Matrix4x4 view {
        get => m_View;
        set {
            m_View = value;
            m_ProjectionType = DecalProjectionType.Custom;
        }
    }

    public DecalProjectionType type {
        get => m_ProjectionType;
        set => m_ProjectionType = value;
    }

    public DecalProjection(Vector3 center, Vector3 forward, float radius) {
        m_ProjectionType = DecalProjectionType.Cube;
        m_Projection = Matrix4x4.Ortho(-radius, radius, -radius, radius, -radius, radius);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(center, Quaternion.FromToRotation(Vector3.forward, forward), new Vector3(1, 1, -1)));
    }
    public DecalProjection(Vector3 center, float radius) {
        m_ProjectionType = DecalProjectionType.Sphere;
        m_Projection = Matrix4x4.Ortho(-radius, radius, -radius, radius, -radius*2f, radius*2f);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(center, Quaternion.identity, new Vector3(1, 1, -1)));
    }
    
    public DecalProjection(Vector3 center, Vector3 forward, float radius, float depthRadius) {
        m_ProjectionType = DecalProjectionType.Orthographic;
        m_Projection = Matrix4x4.Ortho(-radius, radius, -radius, radius, -depthRadius, depthRadius);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(center, Quaternion.FromToRotation(Vector3.forward, forward), new Vector3(1, 1, -1)));
    }
    public DecalProjection(Vector3 startPosition, Quaternion rotation, float fieldOfView, float aspect, float nearClip, float farClip) {
        m_ProjectionType = DecalProjectionType.Perspective;
        m_Projection = Matrix4x4.Perspective(fieldOfView, aspect, nearClip, farClip);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(startPosition, rotation, new Vector3(1, 1, -1)));
    }
    
    public DecalProjection(Vector3 center, Vector3 forward, Vector3 extents) {
        m_ProjectionType = DecalProjectionType.Orthographic;
        m_Projection = Matrix4x4.Ortho(-extents.x, extents.x, -extents.y, extents.y, -extents.z, extents.z);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(center, Quaternion.FromToRotation(Vector3.forward, forward), new Vector3(1, 1, -1)));
    }
    
    public DecalProjection(Vector3 center, Quaternion rotation, Vector3 extents) {
        m_ProjectionType = DecalProjectionType.Orthographic;
        m_Projection = Matrix4x4.Ortho(-extents.x, extents.x, -extents.y, extents.y, -extents.z, extents.z);
        m_View = Matrix4x4.Inverse(Matrix4x4.TRS(center, rotation, new Vector3(1, 1, -1)));
    }

    public DecalProjection(Matrix4x4 projection, Matrix4x4 view) {
        m_ProjectionType = DecalProjectionType.Custom;
        m_Projection = projection;
        m_View = view;
    }
    public static bool operator ==(DecalProjection lhs, DecalProjection rhs) {
        return lhs.m_ProjectionType == rhs.m_ProjectionType && lhs.m_Projection == rhs.m_Projection && lhs.m_View == rhs.m_View;
    }

    public static bool operator !=(DecalProjection lhs, DecalProjection rhs) => !(lhs == rhs);

    public bool Equals(DecalProjection other) => other == this;
    public override bool Equals(object obj) => obj is DecalProjection other && Equals(other);

    public override int GetHashCode() {
        return HashCode.Combine((int)m_ProjectionType, m_Projection, m_View);
    }

    public override string ToString() => $"{{Decal Projection: {m_ProjectionType}}}";
}