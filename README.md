# Unity Skinned Mesh Renderer Decals

[![openupm](https://img.shields.io/npm/v/com.naelstrof.skinnedmeshdecals?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.naelstrof.skinnedmeshdecals/)

An example of how to create efficient projected decals on moving skinned meshes. This should work on all render pipelines, though HDRP is what this project is built on.

![some decals](showcase.gif)

## Installation

You can install this package by adding `https://github.com/naelstrof/SkinnedMeshDecals.git#upm` through the Package Manager's "Add package from git URL".

It's good practice to version lock by replacing #upm with a tag so that your project remains portable.

## Usage

1. Enable "Lightmap generation" in the target models' import settings, or ensure that models have atlased uv2s so that decals can be mapped to any part of the mesh.
2. Apply a material to the target model's mesh that has a `_DecalColorMap` input, and uses UV2 only for the `_DecalColorMap`.
I use AmplifyShaderEditor to create the shaders for this material, this is the only pipeline dependant part! You can also use Shader Graph.
3. Ensure your decal textures, the ones that you'll be splattering, each have mipmapping disabled, and have texture wrap set to clamped. **This is important!**
Otherwise the decals will cover the whole model.
4. Create a script that calls `PaintDecal.QueueDecal(...)` with the target being one of the renderers you've set up. It should let you draw decals!

## Examples

This is a working unity project that can be opened to see the test scene seen above in the gif. You can also simply read some of the example usage scripts like [DecalShooter.cs here](https://github.com/naelstrof/SkinnedMeshDecals/blob/master/Assets/URPExample/Scripts/DecalShooter.cs).

# Detailed explanation

*PaintDecal.QueueDecal* takes three parameters, a *target*, a DecalProjector, and a DecalProjection. It adds the decal to a queue to be rendered later.

## PaintDecal Target

A target can be a Renderer or a Collider. In most games, decals are usually done due to a detected hit event via colliders, this is so common that SkinnedMeshDecals comes with a DecalableCollider class that directly associates a Collider with a set of Renderers.

Collider targets *must* have a DecalableCollider monobehavior on it to work properly. Renderers are if you want to handle decal targets manually.

## DecalProjector

This is a class designed to represent a shader with parameters to be ran as the decal routine. This is extremely complex so it comes with some built-in solutions that serve as examples and the base-line for basic decaling.

Here's a few examples of DecalProjectors that do different things:
- A blue sphere splat: `new DecalProjector(SphereAlpha, Color.blue)`
- An "eraser" sphere: `new DecalProjector(SphereSubtractive, Color.white)`
- A texture splat that ignores backfaces:
```csharp
[SerializeField] private Texture2D splatTexture;
...
new DecalProjector(TextureAdditive, splatTexture, Color.red, true)
```

I highly recommend reading the constructors for the class, as they are XML documented.

## DecalProjection

This is a class that represents a camera where the decal will be blitted. It is interally a View and Projection matrix, and comes with a few constructors that simplifies the decision-making on how you want the decal to be projected.

<img width="852" height="382" alt="image" src="https://github.com/user-attachments/assets/e36e10b6-3f20-475c-a88d-43c66ebc3fa7" />

Here's a few examples of projections:
- 1x1x1 meter ortho projection on a surface from a raycast with a random rotation.
```csharp
Vector3 hitPoint;
Vector3 hitNormal;
...
var ortho1by1by1 = new DecalProjection(hitPoint, Quaternion.LookRotation(-hitNormal) * Quaternion.AngleAxis(Random.Range(0f,360f), Vector3.forward), Vector3.one);
```
- A one meter radius sphere projection, combos great with the Sphere-based DecalProjectors:
```csharp
var oneMeterSphereProj = new DecalProjection(hitPoint, 0.5f);
```

You can find more overloads for constructing projections in the DecalProjectors class. Working with Quaternions to specify the orientation of the projection is hard but worth it I promise!

## DecalSettings

This struct represents any extraneous data needed for blitting the decal. You can specify the following within it:
- The texture name, the name used in the shaders that it expects to override. Defaults to "_DecalColorMap".
- The *RenderTextureFormat* that the splatmap should use, this can be useful if you need more precision (or less!) than RGBA8.
- The *RenderTextureReadWrite* mode, allowing you to disable gamma/linear conversions to read/write raw values.
- *DecalResolution*, to specify an exact texture dimensions, or allow SkinnedMeshDecals to guess based on the texels per meter setting found within SkinnedMeshDecalsSettings.
- *DilationType*, to specify how dilation should be applied to avoid the rasterization error with seams.

For example if you wanted a high-resolution, single-channel floating point splatmap without dilation, you'd give QueueDecal a DecalSettings like so:

```csharp
new DecalSettings(decalResolution = new DecalResolution(new Vector2Int(2048,2048)), dilation = DilationType.None, renderTextureFormat = RenderTextureFormat.R32);
```

See the DecalSettings.cs script to find out more about these settings.

# Technical Feature Description

This package essentially generates textures on meshes, this is an extremely powerful graphical effect that can be used in many ways. Here's a few features that can be added to your project with this technology and a short description on how you'd achieve it.

## Texture Dilation

This decal strategy specifically suffers from a rendering artifact where UV seams won't get painted. The issue is described in great detail [here](https://youtu.be/c7HBxBfCsas?t=329).

Normally this can be solved by enabling Conservative Rasterization, but that is a hardware dependent feature, so we opted for a dilation shader instead.

Any splat maps that were decaled in a frame, will have their dilated versions generated at the end of the frame using the dilation shader specified within the SkinnedMeshDecalsSettings.

## Memory Management

SkinnedMeshDecals uses VRAM to store all the splat maps used, because VRAM is valuable and limited, SkinnedMeshDecals automatically attempts to budget splatmaps to fit within a specified budget.

512MB allows for about 30 2048x2048 RGBA textures. Dilation being enabled halves it to about 15 2048x2048 RGBA textures. This calculation is an estimate though, as the true capacity will vary depending on compatible in-gpu compression.

It can be really easy to go over the budget, oldest splatmaps are removed first before new splatmaps are allocated. If you want to allow for more than the default memory budget, see *SkinnedMeshDecalsSettings.cs*, and adjust the values of the SO found at `SkinnedMeshDecals/Resources/SkinnedMeshDecalsSettings.asset`.

(If it's not found, it is generated automatically as you use SkinnedMeshDecals for the first time in the editor.)

## Fading decals

If you want decals to fade over time, this is easier than it seems! Simply blit a giant subtractive sphere at a tempo that gives it the fade time you desire, something like so:

```csharp
class DecalFader : Monobehavior {
  void Update() {
    foreach(var renderer in GetComponentsInChildren<Renderer>()) {
      PaintDecal.QueueDecal(renderer, new DecalProjector(SphereSubtractive, new Color(1f/256f,1f/256f,1f/256f, 1f)), new DecalProjection(transform.position, 100f));
    }
  }
}
```
Be weary that the default decal texture format only supports 256 colors per pixel, so it will only take 256 frames to completely disappear. If you want it to take more time, space out the subtractive splats over time!

Don't worry about overloading the system, QueueDecal specifically limits how many decals can render per-frame, and it will half-discard commands if it goes over the limit. It could mean that it takes extra time for the fade to take place, but its worth the performance gain generally.

If you prefer not to do decal splats for fading, you can also manually query every RenderTexture in-use by the decal system with `PaintDecal.GetDecalTextures()`, then manually modify them with your own CommandBuffer commands.

## Saving splat state

In your game it might make sense to allow an object in the world to get dirty with splats, but also be able to store it away temporarily.

PaintDecal comes with a `PaintDecal.GetDecalTexture()`, and a `PaintDecal.OverrideDecalTexture()` function, which allows you to save out the image and apply it back. This can be useful for inventory games where you need to remember how "dirty" an object is even if it's inside of an inventory.

Be weary that this disables automatic memory management, and expects you to control the lifetime of the textures. This makes it BAD for saving and loading specifically. Please make a feature request if it's important for your game to be able to save and load without disrupting the memory management.

## RGBA masking

These decals work best as masks since they're fairly low resolution and have no overdraw considerations. This leads to better utilizing the channels as individual masks for things like damage, age, wetness...

Lets say you want to arrange your texture channels to be used in shaders like so:

- R: wetness
- G: damage
- B: unused
- A: age

Since the alpha channel isn't used as an alpha, this requires a custom DecalProjector shader. To create one you'll need to do the following:

1. Install AmplifyShaderEditor (Required to easily edit the built-in projectors!)
2. Install the AmplifyClipSpaceTemplate.unitypackage found in this package's Addon folder.
3. Duplicate one of the existing projector shaders.
4. Edit your custom projector shader so that Alpha blending blits like a regular color. [Check here for blending factor options.](https://docs.unity3d.com/530/Documentation/Manual/SL-Blend.html)
5. Assign this new custom projector shader to a Material in your project.
6. Supply this new material to a QueueDecal command by putting it inside a DecalProjector constructor. `new DecalProjector(material);`

This will allow you to create specific shaders with custom color masking, custom color blending, backface culling, while not worrying too much about how decals are actually projected onto meshes.

Please note that since drawing decals are deferred, configuring the material parameters might require cloning or instantiating the material at decal-time.

You will need to specify the dilation type to be DilationType.Additive in your QueueDecal command if alpha doesn't represent an alpha blend.

# Inspired by

This patreon post and the youtube video were used as the basis for this technology.

https://www.patreon.com/posts/skinned-mesh-22015350

https://www.youtube.com/watch?v=c7HBxBfCsas
