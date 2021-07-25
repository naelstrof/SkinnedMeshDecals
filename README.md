# Unity Skinned Mesh Renderer Decals

[![openupm](https://img.shields.io/npm/v/com.naelstrof.skinnedmeshdecals?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.naelstrof.skinnedmeshdecals/)

An example of how to create efficient projected decals on moving skinned meshes. This should work on all render pipelines, though HDRP is what this project is built on.

![some decals](showcase.gif)

## Instructions on how to use.

1. Install the SkinnedMeshDecals package by adding `https://github.com/naelstrof/SkinnedMeshDecals.git#upm` through the Package Manager's "Add package from git URL". (Or use the openupm badge above.)
2. Enable "Lightmap generation" in the target models' import settings, or ensure that models have atlased uv2s so that decals can be mapped to any part of the mesh.
3. Apply a material to the target model's mesh that has a `_DecalColorMap` input, and uses UV2 only for the `_DecalColorMap`. I use AmplifyShaderEditor to create the shaders for it, this is the only pipeline dependant part! URP, HDRP, and Standard shader examples are included in the package's Shaders folder.
4. Ensure your decal textures, the ones that you'll be splattering, each have mipmapping disabled, and have texture wrap set to clamped. This is important! Otherwise the decals will cover the whole model.
5. Create a script that calls `PaintDecal.instance.RenderDecal(...)` with the target being one of the renderers you've set up. It should let you draw decals!

Find a more technical description of this technology here: https://www.patreon.com/posts/36195193
