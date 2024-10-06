using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkinnedMeshDecals {
    public static class PaintDecal {
        private static SkinnedMeshDecalSettings settings;
        private static Shader dilationAlphaShader;
        private static Shader dilationAdditiveShader;
        private static Material dilationAlphaMaterial;
        private static Material dilationAdditiveMaterial;
        public static SkinnedMeshDecalSettings GetSkinnedMeshDecalSettings() => settings;
        public static void SetSkinnedMeshDecalSettings(SkinnedMeshDecalSettings settings) {
            PaintDecal.settings = settings;
        }

        internal static Material GetDilationMaterial(DilationType dilationType) {
            return dilationType switch {
                DilationType.Alpha => dilationAlphaMaterial,
                DilationType.Additive => dilationAdditiveMaterial,
                _ => throw new ArgumentOutOfRangeException(nameof(dilationType), dilationType, "No associated dilation material for this type.")
            };
        }

        /// <summary>
        /// Returns all render textures used by the decal system. This includes overridden textures. For use in command buffers or similar.
        /// </summary>
        /// <param name="textures">The list where to store the textures</param>
        /// <param name="textureId">The parameter name hash of the textures you're interested in. It defaults to _DecalColorMap if null.</param>
        public static void GetDecalTextures(List<RenderTexture> textures, int? textureId = null) {
            textures.Clear();
            MonoBehaviourHider.DecalableRenderer.GetRenderTextures(textures, textureId ?? GetSkinnedMeshDecalSettings().defaultDecalSettings.textureID);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            settings = new SkinnedMeshDecalSettings(targetMemoryBudgetMB: 512f, "_DecalColorMap", 64f);
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
            printedConfigurationWarning = false;
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
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
            }
            // The render texture is ephemeral, so we copy it!
            var texture = info.GetRenderTexture(textureId ?? GetSkinnedMeshDecalSettings().defaultDecalSettings.textureID);
            if (texture == null) {
                return null;
            }
            RenderTexture copy = new RenderTexture(texture);
            CommandBuffer buffer = new CommandBuffer();
            buffer.Blit(texture, copy);
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
            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
            }
            info.OverrideTexture(customDecalRenderTexture, textureId ?? PaintDecal.GetSkinnedMeshDecalSettings().defaultDecalSettings.textureID, dilation);
        }

        private static bool printedConfigurationWarning;
        /// <summary>
        /// Finds a DecalableCollider, creates decal maps if needed, then splats the corresponding Renderers with the given parameters.
        /// </summary>
        /// <param name="collider">The collider we "hit" with the decal.</param>
        /// <param name="projector">The special decal material used to unwrap the renderer to the screen and splat the decal.</param>
        /// <param name="projection">The projection of the decal, this would be a projection and view matrix from which the decal is shot from.</param>
        /// <param name="decalSettings">Special settings used to configure how the render texture used for splatting is generated, if it should have dilation applied, and how large the texture should be.</param>
        public static void QueueDecal(Collider collider, DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null) {
            if (!collider.TryGetComponent(out DecalableCollider decalableCollider)) {
                if (!printedConfigurationWarning) {
                    Debug.LogWarning("Tried to render a decal on a collider which is missing a DecalableCollider component. <color=cyan>This will only print once.</color>\nThis is a misconfiguration, colliders that are meant to be decaled should have one. Or shouldn't be hit by decals in the first place", collider.gameObject);
                    printedConfigurationWarning = true;
                }
                return;
            }
            decalableCollider.QueueDecal(projector, projection, decalSettings);
        }

        internal static void QueueDecal(MonoBehaviourHider.DecalableRenderer r, DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null) {
            DecalCommandProcessor.AddDecalCommand(new DecalCommand() {
                decalableRenderer = r,
                projector = projector,
                projection = projection,
                decalSettings = decalSettings,
            });
        }

        /// <summary>
        /// Creates a decal map if needed, then splats it with the given parameters.
        /// </summary>
        /// <param name="r">The renderer to be splatted.</param>
        /// <param name="projector">The special decal material used to unwrap the renderer to the screen and splat the decal.</param>
        /// <param name="projection">The projection of the decal, this would be a projection and view matrix from which the decal is shot from.</param>
        /// <param name="decalSettings">Special settings used to configure how the render texture used for splatting is generated, if it should have dilation applied, and how large the texture should be.</param>
        public static void QueueDecal(Renderer r, DecalProjector projector, DecalProjection projection, DecalSettings? decalSettings = null) {
            // Only can draw on meshes.
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) {
                return;
            }
            
#if UNITY_EDITOR
            if (r.localToWorldMatrix.determinant < 0f) {
                Debug.LogError("Tried to render a decal on an inside-out object, this isn't supported! Make sure scales aren't negative.", r.gameObject);
            }
#endif

            if (!r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer info)) {
                info = r.gameObject.AddComponent<MonoBehaviourHider.DecalableRenderer>();
            }
            QueueDecal(info, projector, projection, decalSettings);
        }

        /// <summary>
        /// Immediately destroys all decals everywhere. And frees up the memory associated with them.
        /// </summary>
        public static void ClearDecalMaps() {
            MonoBehaviourHider.DecalableRenderer.ClearAll();
        }
        // This clears the decals and frees memory for the specified renderer.
        // If you wanted to "clean" renderers in a more believable way, you could draw decals in a subtractive mode on the renderer.
        public static void ClearDecalsForRenderer(Renderer r) {
            if (r.TryGetComponent(out MonoBehaviourHider.DecalableRenderer info)) {
                info.Release();
            }
        }
    }

}