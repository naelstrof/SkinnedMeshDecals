using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

public class AtlasPackTest : MonoBehaviour {

    [SerializeField] List<Renderer> _renderers;

    void Start() {
        var renderers = new List<Renderer>();
        foreach (var renderer in _renderers) {
            renderers.Add(renderer);
        }
        AtlasPacker.Pack(renderers.ToArray());
    }

}
