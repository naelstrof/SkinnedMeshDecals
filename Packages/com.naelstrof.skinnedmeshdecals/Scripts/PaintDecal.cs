using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkinnedMeshDecals {
    public struct DecalTextureRendererPair {
        public RenderTexture texture;
        public Renderer renderer;
    }
    public static class PaintDecal {
        [Range(16f,2048f)]
        [Tooltip("Memory usage in megabytes before old textures get removed.")]
        private static float memoryBudgetMB = 512f;

        private static Shader dilationAlphaShader;
        
        private static Shader dilationAdditiveShader;
        
        private static Material dilationAlphaMaterial;
        private static Material dilationAdditiveMaterial;
        
        private static CommandBuffer commandBuffer;
        private static readonly List<MonoBehaviourHider.DecalableInfo> rendererCache = new List<MonoBehaviourHider.DecalableInfo>();
        
        private static int memoryBudget => Mathf.RoundToInt(memoryBudgetMB * 1000000f);
        /// <summary>
        /// Gets the memory budget in bytes.
        /// </summary>
        /// <returns>The memory budget in bytes.</returns>
        public static int GetMemoryBudget() => memoryBudget;

        /// <summary>
        /// Sets the maximum amount of memory the decal system can consume before it starts deleting old decal maps.
        /// </summary>
        /// <param name="mb">The memory size in megabytes.</param>
        public static void SetMemoryBudgetMB(float mb) {
            memoryBudgetMB = mb;
            while (GetMemoryInUse() > memoryBudget && rendererCache.Count > 0) {
                if (!RemoveOldest()) {
                    break;
                }
            }
        }

        internal static Material GetDilationMaterial(DilationType dilationType) {
            return dilationType switch {
                DilationType.Alpha => dilationAlphaMaterial,
                DilationType.Additive => dilationAdditiveMaterial,
                _ => throw new ArgumentOutOfRangeException(nameof(dilationType), dilationType, "No associated dilation material for this type.")
            };
        }

        internal static Material GetAdditiveDilationMaterial() => dilationAlphaMaterial;

        private static int InternalMemoryInUse() {
            int memoryInUse = 0;
            foreach(var info in rendererCache) {
                memoryInUse += info.GetMemoryUsed();
            }
            return memoryInUse;
        }


        /// <summary>
        /// Returns all render textures used by the decal system. This includes overridden textures. For use in command buffers or similar.
        /// </summary>
        /// <param name="textures">The list where to store the textures</param>
        /// <param name="textureId">The parameter name hash of the textures you're interested in. It defaults to _DecalColorMap if null.</param>
        public static void GetDecalTextures(List<DecalTextureRendererPair> textures, int? textureId = null) {
            textures.Clear();
            foreach (var decalInfo in rendererCache) {
                var textureRendererPair = decalInfo.GetRenderTexture(textureId ?? DecalSettings.Default.textureID);
                if (textureRendererPair != null) {
                    textures.Add(textureRendererPair.Value);
                }
            }
        }

        /// <summary>
        /// Gets an estimation on how much memory is being used by decal textures.
        /// </summary>
        /// <returns>The memory use in bytes.</returns>
        public static int GetMemoryInUse() => InternalMemoryInUse();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            commandBuffer = new CommandBuffer();
            dilationAlphaShader = Shader.Find("Hidden/Naelstrof/DilationShaderAlpha");
            dilationAdditiveShader = Shader.Find("Hidden/Naelstrof/DilationShaderAdditive");
            if (dilationAlphaShader== null) {
                throw new UnityException("SkinnedMeshDecals: Failed to find shader Hidden/Naelstrof/DilationShaderAlpha, check if SMD is imported correctly.");
            }
            if (dilationAdditiveShader == null) {
                throw new UnityException("SkinnedMeshDecals: Failed to find shader Hidden/Naelstrof/DilationShaderAdditive, check if SMD is imported correctly.");
            }
            dilationAlphaMaterial = new Material(dilationAlphaShader);
            dilationAdditiveMaterial = new Material(dilationAdditiveShader);
        }

        internal static bool TryReserveMemory(int amount) {
            while (amount < GetMemoryBudget() && GetMemoryBudget()-GetMemoryInUse() < amount && amount < GetMemoryBudget() && rendererCache.Count > 0) {
                if (!RemoveOldest()) {
                    break;
                }
            }
            return amount < GetMemoryBudget() - GetMemoryInUse();
        }

        private static bool RemoveOldest() {
            MonoBehaviourHider.DecalableInfo oldestInfo = null;
            float oldestTime = float.MaxValue;
            foreach(var info in rendererCache) {
                if (!(info.GetLastUseTime() < oldestTime)) continue;
                oldestInfo = info;
                oldestTime = info.GetLastUseTime();
            }
            if (oldestInfo == null && rendererCache.Count > 0) {
                oldestInfo = rendererCache[0];
            }
            if (oldestInfo == null) return false;
            RemoveDecalableInfo(oldestInfo);
            Object.Destroy(oldestInfo);
            return true;
        }
        internal static void AddDecalableInfo(MonoBehaviourHider.DecalableInfo info) {
            if (!rendererCache.Contains(info)) {
                rendererCache.Add(info);
            }
        }
        internal static void RemoveDecalableInfo(MonoBehaviourHider.DecalableInfo info) {
            if (rendererCache.Contains(info)) {
                rendererCache.Remove(info);
            }
        }

        /// <summary>
        /// Directly access the decal texture of a given renderer.
        /// </summary>
        /// <param name="r">The renderer which has the decals splatted on it.</param>
        /// <param name="textureId">The texture name, hashed via Shader.PropertyToID(), from which the decal map should be grabbed.</param>
        /// <returns>A copy of the decal map associated with the textureId.</returns>
        public static RenderTexture GetDecalTexture(Renderer r, int? textureId = null) {
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
                return null;
            }
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableInfo>();
                info.Initialize();
            }
            // The render texture is ephemeral, so we copy it!
            var texture = info.GetRenderTexture(textureId ?? DecalSettings.Default.textureID);
            if (texture == null) {
                return null;
            }
            RenderTexture copy = new RenderTexture(texture.Value.texture);
            CommandBuffer buffer = new CommandBuffer();
            buffer.Blit(texture.Value.texture, copy);
            buffer.GenerateMips(copy);
            Graphics.ExecuteCommandBuffer(buffer);
            return copy;
        }
        
        /// <summary>
        /// Allows you to immediately set the decal map of a renderer. Which will be used in the future for decals.
        /// Useful for situations where you need to delete a dirty object, then respawn it later. Retaining the decals applied for the duration. See GetDecalTexture on how to save decal maps.
        /// BE AWARE that this disables automatic memory management for that specific decal map. You are responsible for deleting your own texture when you're done with it.
        /// </summary>
        /// <param name="r">The renderer that should recieve the decal map.</param>
        /// <param name="customDecalRenderTexture">The texture you want to replace the decal map with.</param>
        /// <param name="textureId">The parameter name of the decal map, gotten via Shader.PropertyToID("textureName"). You can use null for the default.</param>
        /// <param name="dilationEnabled">If the texture should have dilation enabled, this should be set to true for mask maps.</param>
        /// <exception cref="UnityException">This method will throw exceptions for textures that aren't configured properly. Mipmaps need to be enabled, and autoGenerateMips should be disabled.</exception>
        public static void OverrideDecalTexture(Renderer r, RenderTexture customDecalRenderTexture, int? textureId = null, DilationType dilation = DilationType.None) {
            if (customDecalRenderTexture.useMipMap == false) {
                throw new UnityException("Can't set RenderTexture of decalable because it has mipmaps disabled!");
            }
#if UNITY_EDITOR
            if (customDecalRenderTexture.autoGenerateMips) {
                Debug.LogWarning( "Using a custom decal RenderTexture with autoGenerateMips enabled, this will cause performance problems if you draw lots of decals per frame.");
            }
#endif
            
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
                return;
            }
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableInfo>();
                info.Initialize();
            }
            info.OverrideTexture(customDecalRenderTexture, textureId ?? DecalSettings.Default.textureID, dilation);
        }

        /// <summary>
        /// Creates a decal map if needed, then splats it with the given parameters.
        /// </summary>
        /// <param name="r">The renderer to be splatted.</param>
        /// <param name="projector">The special decal material used to unwrap the renderer to the screen and splat the decal.</param>
        /// <param name="projection">The projection of the decal, this would be a projection and view matrix from which the decal is shot from.</param>
        /// <param name="decalSettings">Special settings used to configure how the render texture used for splatting is generated, if it should have dilation applied, and how large the texture should be.</param>
        public static void RenderDecal(Renderer r, DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null) {
            // Only can draw on meshes.
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
                return;
            }
            
#if UNITY_EDITOR
            if (r.localToWorldMatrix.determinant < 0f) {
                Debug.LogError("Tried to render a decal on an inside-out object, this isn't supported! Make sure scales aren't negative.", r.gameObject);
            }
#endif

            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableInfo>();
                info.Initialize();
            }
            
            commandBuffer.Clear();
            commandBuffer.SetViewProjectionMatrices(projection.view, projection.projection);
            info.Render(commandBuffer, projector, decalSettings ?? DecalSettings.Default);
            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        /// <summary>
        /// Immediately destroys all decals everywhere. And frees up the memory associated with them.
        /// </summary>
        public static void ClearDecalMaps() {
            while(rendererCache.Count > 0) {
                Object.Destroy(rendererCache[0]);
                RemoveDecalableInfo(rendererCache[0]);
            }
        }
        // This clears the decals and frees memory for the specified renderer.
        // If you wanted to "clean" renderers in a more believable way, you could draw decals in a subtractive mode on the renderer.
        public static void ClearDecalsForRenderer(Renderer r) {
            if (r.TryGetComponent(out MonoBehaviourHider.DecalableInfo info)) {
                Object.Destroy(info);
                RemoveDecalableInfo(info);
            }
        }
    }

}