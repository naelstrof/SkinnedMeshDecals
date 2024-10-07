using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkinnedMeshDecals {
public class DecalableCollider : MonoBehaviour, IList<Renderer> {
    [SerializeField]
    private List<Renderer> renderers;

    private bool initialized => decalableRenderers != null;
    private HashSet<MonoBehaviourHider.DecalableRenderer> decalableRenderers;
    
    private void Initialize() {
        if (initialized) {
            return;
        }
        decalableRenderers = new HashSet<MonoBehaviourHider.DecalableRenderer>();
        foreach (var r in renderers) {
            if (!r) {
                return;
            }
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer decalableRenderer)) {
                decalableRenderer = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
            }
            decalableRenderers.Add(decalableRenderer);
            decalableRenderer.destroyed += OnRendererDestroyed;
        }
    }

    private void InitializeRenderer(Renderer r) {
        if (!r) {
            return;
        }
        if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer decalableRenderer)) {
            decalableRenderer = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
        }
        decalableRenderers.Add(decalableRenderer);
        decalableRenderer.destroyed += OnRendererDestroyed;
    }

    private void OnRendererDestroyed(MonoBehaviourHider.DecalableRenderer decalableRenderer) {
        if (renderers.Contains(decalableRenderer.GetRenderer())) {
            renderers.Remove(decalableRenderer.GetRenderer());
        }
        if (decalableRenderers.Contains(decalableRenderer)) {
            decalableRenderers.Remove(decalableRenderer);
        }
    }

    public void QueueDecal(DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null, PaintDecal.QueueType queueType = PaintDecal.QueueType.Deferred) {
        Initialize();
        foreach (var decalableRenderer in decalableRenderers) {
            PaintDecal.QueueDecal(decalableRenderer, projector, projection, decalSettings, queueType);
        }
    }

    private void OnDestroy() {
        if (!initialized) {
            return;
        }

        foreach (var r in decalableRenderers) {
            if (!r) {
                return;
            }
            r.destroyed -= OnRendererDestroyed;
        }
    }

    public IEnumerator<Renderer> GetEnumerator() {
        return renderers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(Renderer item) {
        renderers.Add(item);
        if (initialized) {
            InitializeRenderer(item);
        }
    }

    public void Clear() {
        renderers.Clear();
        decalableRenderers.Clear();
    }

    public bool Contains(Renderer item) {
        return renderers.Contains(item);
    }

    public void CopyTo(Renderer[] array, int arrayIndex) {
        renderers.CopyTo(array, arrayIndex);
    }

    public bool Remove(Renderer item) {
        if (!renderers.Contains(item)) {
            return false;
        }
        decalableRenderers.RemoveWhere((a) => a.GetRenderer() == item);
        return renderers.Remove(item);
    }

    public int Count => renderers.Count;
    public bool IsReadOnly => false;
    public int IndexOf(Renderer item) {
        return renderers.IndexOf(item);
    }
    public void Insert(int index, Renderer item) {
        renderers.Insert(index, item);
        if (initialized) {
            InitializeRenderer(item);
        }
    }
    
    public void RemoveAt(int index) {
        var renderer = renderers[index];
        if (initialized) {
            decalableRenderers.RemoveWhere((a) => a.GetRenderer() == renderer);
        }
        renderers.RemoveAt(index);
    }

    public Renderer this[int index] {
        get => renderers[index];
        set {
            var oldRenderer = renderers[index];
            if (initialized) {
                decalableRenderers.RemoveWhere((a) => a.GetRenderer() == oldRenderer);
            }
            renderers[index] = value;
            if (initialized) {
                InitializeRenderer(value);
            }
        }
    }
}

}
