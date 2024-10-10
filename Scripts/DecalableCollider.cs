using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkinnedMeshDecals {
public class DecalableCollider : MonoBehaviour {

    public class RenderList : IList<Renderer> {
        private bool initialized => decalableRenderers != null;
        private List<Renderer> renderers;
        private HashSet<MonoBehaviourHider.DecalableRenderer> decalableRenderers;

        internal RenderList(List<Renderer> defaultRenderers = null) {
            if (defaultRenderers != null) {
                renderers = new List<Renderer>(defaultRenderers);
            } else {
                renderers = new List<Renderer>();
            }
            Initialize();
        }
        
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

        public IEnumerator<Renderer> GetEnumerator() {
            return renderers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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

        internal void Destroy() {
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
    }

    [FormerlySerializedAs("renderers")] [SerializeField]
    private List<Renderer> startingRenderers;

    private RenderList decalableRenderers;

    public RenderList renderers {
        get {
            decalableRenderers ??= new RenderList(startingRenderers);
            return decalableRenderers;
        }
    }

    public void QueueDecal(DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null, PaintDecal.QueueType queueType = PaintDecal.QueueType.Deferred) {
        foreach (var decalableRenderer in renderers) {
            PaintDecal.QueueDecal(decalableRenderer, projector, projection, decalSettings, queueType);
        }
    }

    private void OnDestroy() {
        decalableRenderers?.Destroy();
    }

}

}
