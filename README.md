# Unity Skinned Mesh Renderer Decals

An example of how to create efficient projected decals on moving skinned meshes. This should work on all render pipelines, though HDRP is what this project is built on.

![some decals](showcase.gif)

## Instructions on how to use.

Dynamic Meshes (MeshRenderer, SkinnedMeshRenderer)
1. Enable "Lightmap generation" in the model import settings, or ensure the model has atlased uv2s so that decals can be mapped to any part of the mesh.
2. Apply a material to the mesh that has a `_DecalColorMap` input, and uses UV2 only for the `_DecalColorMap`. I use AmplifyShaderEditor to create the shaders for it, this is the only pipeline dependant part!
3. Ensure your decal textures have mipmapping disabled, and have texture wrap set to clamped.
4. Create a script that calls `PaintDecal.instance.RenderDecal(...)` with the target being one of the renderers you've set up. It should let you draw decals!

Static Meshes (Static, lightmapped geometry)
1. Generate lightmaps for any static geometry that need decals. The PaintDecal class re-uses the atlasing that Unity creates for it. Lower resolution lightmaps mean higher resolution decal maps!
2. Apply a material to the mesh that has a `_DecalColorMap` input, though instead of just using UV2, you gotta use UV2 scaled and translated by the `unity_LightmapST` transformation. Again use AmplifyShaderEditor to check out how the Example works.
3. Ensure your decal textures have mipmapping disabled, and have texture wrap set to clamped.
4. Create a script that calls `PaintDecal.instance.RenderDecal(...)` with the target being one of the static renderables, using your decal texture.


Find a more technical description of this technology here: https://www.patreon.com/posts/36195193
