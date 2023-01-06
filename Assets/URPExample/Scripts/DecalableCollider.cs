using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalableCollider : MonoBehaviour {
    [SerializeField]
    private Renderer[] decalableRenderers;

    public Renderer[] GetDecalableRenderers() => decalableRenderers;
}
