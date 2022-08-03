using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkinnedMeshDecals {
#if UNITY_EDITOR
    [CustomEditor(typeof(PaintDecal))]
    public class PaintDecalEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (Application.isPlaying) {
                float progress = (float)PaintDecal.GetMemoryInUse() / (float)PaintDecal.GetMemoryBudget();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(true), progress, "Memory in use");
            } else {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(true), 0, "Memory in use");
            }
        }
    }
#endif

    public class PaintDecal : MonoBehaviour {
        [SerializeField] [Range(32f,2048f)]
        [Tooltip("Memory usage in megabytes before old textures get removed.")]
        private float memoryBudgetMB = 512f;
        
        [SerializeField] [Range(32f,4096f)]
        [Tooltip("This determines how big the textures are for each objects' scale. Calculated by pixels per meter.")]
        private float texelsPerMeter = 512f;

        [SerializeField] [Tooltip("The shader used to dilate the decals.")]
        private Shader dilationShader;

        [SerializeField] [Tooltip("If we should run dilation techniques to hide seams.")]
        private bool dilate = true;

        private Material dilationMaterial;
        
        private static PaintDecal instance;
        private Collider[] colliders;
        private List<Renderer> staticRenderers;
        private List<Renderer> staticTempRenderers;
        private CommandBuffer commandBuffer;
        private int memoryBudget => Mathf.RoundToInt(memoryBudgetMB * 1000000f);
        public static int GetMemoryBudget() => instance.memoryBudget;
        public static bool IsDilateEnabled() => instance.dilate;


        private const string defaultTextureName = "_DecalColorMap";
        public static Material GetDilationMaterial() => instance.dilationMaterial;
        public static float GetTexelsPerMeter() => instance.texelsPerMeter;
        private static void GetComponentsInChildrenNoAlloc<T>(Transform t, List<T> temp, List<T> result) {
            t.GetComponents<T>(temp);
            result.AddRange(temp);
            for(int i=0;i<t.childCount;i++) {
                GetComponentsInChildrenNoAlloc<T>(t.GetChild(i), temp, result);
            }
        }

        private int InternalMemoryInUse() {
            int memoryInUse = 0;
            foreach(var info in rendererCache) {
                memoryInUse += info.GetSize();
            }
            return memoryInUse;
        }

        public static int GetMemoryInUse() => instance.InternalMemoryInUse();

        private void Awake() {
            if (instance == null || instance == this) {
                instance = this;
            } else {
                Destroy(this);
                return;
            }
            colliders = new Collider[32];
            staticRenderers = new List<Renderer>();
            staticTempRenderers = new List<Renderer>();
            commandBuffer = new CommandBuffer();
            dilationMaterial = new Material(dilationShader);
        }

        public static void SetMemoryBudgetMB(float mb) {
            instance.memoryBudgetMB = mb;
            while (GetMemoryInUse() > instance.memoryBudget) {
                instance.RemoveOldest();
            }
        }

        public static bool TryReserveMemory(int amount) {
            while (amount < GetMemoryBudget() && GetMemoryBudget()-GetMemoryInUse() < amount) {
                instance.RemoveOldest();
            }
            return amount < GetMemoryBudget() - GetMemoryInUse();
        }

        private readonly List<MonoBehaviourHider.DecalableInfo> rendererCache = new List<MonoBehaviourHider.DecalableInfo>();
        private void RemoveOldest() {
            MonoBehaviourHider.DecalableInfo oldestInfo = null;
            float oldestTime = float.MaxValue;
            foreach(var info in rendererCache) {
                if (info.GetLastUseTime() < oldestTime) {
                    oldestInfo = info;
                    oldestTime = info.GetLastUseTime();
                }
            }
            if (oldestInfo != null) {
                RemoveDecalableInfo(oldestInfo);
                Destroy(oldestInfo);
            }
        }
        public static bool IsDecalable(Material m, string textureTarget = defaultTextureName) {
            return m.HasProperty(textureTarget);
        }
        public static void AddDecalableInfo(MonoBehaviourHider.DecalableInfo info) {
            instance.rendererCache.Add(info);
        }
        public static void RemoveDecalableInfo(MonoBehaviourHider.DecalableInfo info) {
            instance.rendererCache.Remove(info);
        }

        public static void RenderDecalForCollider(Collider c, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f, string textureName = defaultTextureName) {
            LODGroup group = c.GetComponentInParent<LODGroup>();
            if (group != null) {
                instance.staticRenderers.Clear();
                GetComponentsInChildrenNoAlloc<Renderer>(group.transform, instance.staticTempRenderers, instance.staticRenderers);
                foreach(Renderer renderer in instance.staticRenderers) {
                    RenderDecal(renderer, projector, position, rotation, size, depth, textureName);
                }
                return;
            }
            Renderer parentRenderer = c.GetComponentInParent<Renderer>();
            if (parentRenderer != null) {
                RenderDecal(parentRenderer, projector, position, rotation, size, depth, textureName);
            }

            instance.staticRenderers.Clear();
            GetComponentsInChildrenNoAlloc<Renderer>(c.transform, instance.staticTempRenderers, instance.staticRenderers);
            foreach(Renderer renderer in instance.staticRenderers) {
                RenderDecal(renderer, projector, position, rotation, size, depth, textureName);
            }
        }
        public static void RenderDecalInSphere(Vector3 position, float radius, Material projector, Quaternion rotation, LayerMask hitMask, string textureName = defaultTextureName) {
            int hits = Physics.OverlapSphereNonAlloc(position, radius, instance.colliders, hitMask, QueryTriggerInteraction.UseGlobal);
            for(int i=0;i<hits;i++) {
                Collider c = instance.colliders[i];
                RenderDecalForCollider(c, projector, position-rotation*Vector3.forward*radius, rotation, Vector2.one*radius, radius*2f, textureName);
            }
        }
        public static void RenderDecalInBox(Vector3 boxHalfExtents, Vector3 position, Material projector, Quaternion boxOrientation, LayerMask hitMask, string textureName = defaultTextureName) {
            int hits = Physics.OverlapBoxNonAlloc(position, boxHalfExtents, instance.colliders, boxOrientation, hitMask, QueryTriggerInteraction.UseGlobal);
            for(int i=0;i<hits;i++) {
                Collider c = instance.colliders[i];
                RenderDecalForCollider(c, projector, position-boxOrientation*Vector3.forward*boxHalfExtents.z, boxOrientation, new Vector2(boxHalfExtents.x, boxHalfExtents.y)*2f, boxHalfExtents.z*2f, textureName);
            }
        }
        public static void RenderDecalForCollision(Collider c, Material projector, Vector3 position, Vector3 normal, float rotationAboutNormal, Vector2 size, float halfDepth = 0.5f, string textureName = defaultTextureName) {
            RenderDecalForCollider(c, projector, position+normal*halfDepth, Quaternion.AngleAxis(rotationAboutNormal, normal)*Quaternion.FromToRotation(Vector3.forward, -normal), size, halfDepth*2f, textureName);
        }
        public static void RenderDecal(Renderer r, Material projector, Vector3 position, Quaternion rotation, Vector2 size, float depth = 0.5f, string textureName = defaultTextureName) {
            // Only can draw on meshes.
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
                return;
            }

            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableInfo>();
                info.Initialize();
            }
            // Could use a Matrix4x4.Perspective instead! depends on use case.
            Matrix4x4 projection = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, 0f, depth);
            Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1)));
            
            // We just queue up the commands, paintdecal will send them all together when its ready.
            instance.commandBuffer.Clear();
            instance.commandBuffer.SetViewProjectionMatrices(view, projection);
            info.Render(instance.commandBuffer, projector, textureName);
            Graphics.ExecuteCommandBuffer(instance.commandBuffer);
        }

        public static void ClearDecalMaps() {
            while(instance.rendererCache.Count > 0) {
                Destroy(instance.rendererCache[0]);
                RemoveDecalableInfo(instance.rendererCache[0]);
            }
        }
        // This clears the decals and frees memory for the specified renderer.
        // If you wanted to "clean" renderers in a more believable way, you could draw decals in a subtractive mode on the renderer.
        public static void ClearDecalsForRenderer(Renderer r) {
            if (r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                Destroy(info);
                RemoveDecalableInfo(info);
            }
        }

        private void OnValidate() {
            if (Application.isPlaying) {
                while (InternalMemoryInUse() > memoryBudget) {
                    RemoveOldest();
                }
            }
        }
    }

}