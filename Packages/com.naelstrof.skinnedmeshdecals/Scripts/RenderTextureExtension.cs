using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SkinnedMeshDecals {

public static class RenderTextureExtension {
    public static ulong GetTotalBitsGraphicsMemory(int width, int height, int depth, GraphicsFormat format) {
        switch (format) {
            case GraphicsFormat.None:
                return (ulong)width * (ulong)height * (ulong)depth;
            case GraphicsFormat.R8_SRGB:
            case GraphicsFormat.R8_UNorm:
            case GraphicsFormat.R8_SNorm:
            case GraphicsFormat.R8_UInt:
            case GraphicsFormat.R8_SInt:
            case GraphicsFormat.S8_UInt:
                return (ulong)(width*height)*8L;
            case GraphicsFormat.R8G8_SRGB:
            case GraphicsFormat.R8G8_UNorm:
            case GraphicsFormat.R8G8_SNorm:
            case GraphicsFormat.R8G8_UInt:
            case GraphicsFormat.R8G8_SInt:
            case GraphicsFormat.R16_UNorm:
            case GraphicsFormat.R16_SNorm:
            case GraphicsFormat.R16_UInt:
            case GraphicsFormat.R16_SInt:
            case GraphicsFormat.R16_SFloat:
            case GraphicsFormat.R4G4B4A4_UNormPack16:
            case GraphicsFormat.B4G4R4A4_UNormPack16:
            case GraphicsFormat.R5G6B5_UNormPack16:
            case GraphicsFormat.B5G6R5_UNormPack16:
            case GraphicsFormat.R5G5B5A1_UNormPack16:
            case GraphicsFormat.B5G5R5A1_UNormPack16:
            case GraphicsFormat.A1R5G5B5_UNormPack16:
            case GraphicsFormat.D16_UNorm:
            case GraphicsFormat.YUV2:
                return (ulong)(width*height)*16;
            case GraphicsFormat.R8G8B8_SRGB:
            case GraphicsFormat.R8G8B8_UNorm:
            case GraphicsFormat.R8G8B8_SNorm:
            case GraphicsFormat.R8G8B8_UInt:
            case GraphicsFormat.R8G8B8_SInt:
            case GraphicsFormat.B8G8R8_SRGB:
            case GraphicsFormat.B8G8R8_UNorm:
            case GraphicsFormat.B8G8R8_SNorm:
            case GraphicsFormat.B8G8R8_UInt:
            case GraphicsFormat.B8G8R8_SInt:
            case GraphicsFormat.D24_UNorm:
            case GraphicsFormat.D24_UNorm_S8_UInt:
                return (ulong)(width*height)*24L;
            case GraphicsFormat.R8G8B8A8_SRGB:
            case GraphicsFormat.R8G8B8A8_UNorm:
            case GraphicsFormat.R8G8B8A8_SNorm:
            case GraphicsFormat.R8G8B8A8_UInt:
            case GraphicsFormat.R8G8B8A8_SInt:
            case GraphicsFormat.B8G8R8A8_SRGB:
            case GraphicsFormat.B8G8R8A8_UNorm:
            case GraphicsFormat.B8G8R8A8_SNorm:
            case GraphicsFormat.B8G8R8A8_UInt:
            case GraphicsFormat.B8G8R8A8_SInt:
            case GraphicsFormat.R16G16_UNorm:
            case GraphicsFormat.R16G16_SNorm:
            case GraphicsFormat.R16G16_UInt:
            case GraphicsFormat.R16G16_SInt:
            case GraphicsFormat.R16G16_SFloat:
            case GraphicsFormat.R32_UInt:
            case GraphicsFormat.R32_SInt:
            case GraphicsFormat.R32_SFloat:
            case GraphicsFormat.E5B9G9R9_UFloatPack32:
            case GraphicsFormat.B10G11R11_UFloatPack32:
            case GraphicsFormat.A2B10G10R10_UNormPack32:
            case GraphicsFormat.A2B10G10R10_UIntPack32:
            case GraphicsFormat.A2B10G10R10_SIntPack32:
            case GraphicsFormat.A2R10G10B10_UNormPack32:
            case GraphicsFormat.A2R10G10B10_UIntPack32:
            case GraphicsFormat.A2R10G10B10_SIntPack32:
            case GraphicsFormat.A2R10G10B10_XRSRGBPack32:
            case GraphicsFormat.A2R10G10B10_XRUNormPack32:
            case GraphicsFormat.R10G10B10_XRSRGBPack32:
            case GraphicsFormat.R10G10B10_XRUNormPack32:
            case GraphicsFormat.A10R10G10B10_XRSRGBPack32:
            case GraphicsFormat.A10R10G10B10_XRUNormPack32:
            case GraphicsFormat.D32_SFloat:
                return (ulong)(width*height)*32L;
            case GraphicsFormat.D32_SFloat_S8_UInt:
                return (ulong)(width*height)*40L;
            case GraphicsFormat.R16G16B16_UNorm:
            case GraphicsFormat.R16G16B16_SNorm:
            case GraphicsFormat.R16G16B16_UInt:
            case GraphicsFormat.R16G16B16_SInt:
            case GraphicsFormat.R16G16B16_SFloat:
                return (ulong)(width*height)*48L;
            case GraphicsFormat.R16G16B16A16_UNorm:
            case GraphicsFormat.R16G16B16A16_SNorm:
            case GraphicsFormat.R16G16B16A16_UInt:
            case GraphicsFormat.R16G16B16A16_SInt:
            case GraphicsFormat.R16G16B16A16_SFloat:
            case GraphicsFormat.R32G32_UInt:
            case GraphicsFormat.R32G32_SInt:
            case GraphicsFormat.R32G32_SFloat:
                return (ulong)(width*height)*64L;
            case GraphicsFormat.R32G32B32_UInt:
            case GraphicsFormat.R32G32B32_SInt:
            case GraphicsFormat.R32G32B32_SFloat:
                return (ulong)(width*height)*96L;
            case GraphicsFormat.R32G32B32A32_UInt:
            case GraphicsFormat.R32G32B32A32_SInt:
            case GraphicsFormat.R32G32B32A32_SFloat:
                return (ulong)(width*height)*128L;
            case GraphicsFormat.RGBA_DXT1_SRGB:
            case GraphicsFormat.RGBA_DXT1_UNorm:
            case GraphicsFormat.R_BC4_UNorm:
            case GraphicsFormat.R_BC4_SNorm:
            case GraphicsFormat.RGB_PVRTC_4Bpp_SRGB:
            case GraphicsFormat.RGB_PVRTC_4Bpp_UNorm:
            case GraphicsFormat.RGB_ETC_UNorm:
            case GraphicsFormat.RGB_ETC2_SRGB:
            case GraphicsFormat.RGB_ETC2_UNorm:
            case GraphicsFormat.RGB_A1_ETC2_SRGB:
            case GraphicsFormat.RGB_A1_ETC2_UNorm:
            case GraphicsFormat.RGBA_ETC2_SRGB:
            case GraphicsFormat.R_EAC_UNorm:
            case GraphicsFormat.R_EAC_SNorm:
                return (ulong)(width*height) * 64L / (4L * 4L);
            case GraphicsFormat.RGBA_DXT3_SRGB:
            case GraphicsFormat.RGBA_DXT3_UNorm:
            case GraphicsFormat.RGBA_DXT5_SRGB:
            case GraphicsFormat.RGBA_DXT5_UNorm:
            case GraphicsFormat.RG_BC5_UNorm:
            case GraphicsFormat.RG_BC5_SNorm:
            case GraphicsFormat.RGB_BC6H_UFloat:
            case GraphicsFormat.RGB_BC6H_SFloat:
            case GraphicsFormat.RGBA_BC7_SRGB:
            case GraphicsFormat.RGBA_BC7_UNorm:
            case GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB:
            case GraphicsFormat.RGBA_PVRTC_4Bpp_UNorm:
            case GraphicsFormat.RGBA_ETC2_UNorm:
            case GraphicsFormat.RG_EAC_UNorm:
            case GraphicsFormat.RG_EAC_SNorm:
            case GraphicsFormat.RGBA_ASTC4X4_SRGB:
            case GraphicsFormat.RGBA_ASTC4X4_UNorm:
            case GraphicsFormat.RGBA_ASTC4X4_UFloat:
                return (ulong)(width*height) * 128L / (4L * 4L);
            case GraphicsFormat.RGB_PVRTC_2Bpp_SRGB:
            case GraphicsFormat.RGB_PVRTC_2Bpp_UNorm:
            case GraphicsFormat.RGBA_PVRTC_2Bpp_SRGB:
            case GraphicsFormat.RGBA_PVRTC_2Bpp_UNorm:
                return (ulong)(width*height) * 64L / (8L * 4L);
            case GraphicsFormat.RGBA_ASTC5X5_SRGB:
            case GraphicsFormat.RGBA_ASTC5X5_UNorm:
            case GraphicsFormat.RGBA_ASTC5X5_UFloat:
                return (ulong)(width*height) * 128L / (5L * 5L);
            case GraphicsFormat.RGBA_ASTC6X6_SRGB:
            case GraphicsFormat.RGBA_ASTC6X6_UNorm:
            case GraphicsFormat.RGBA_ASTC6X6_UFloat:
                return (ulong)(width*height) * 128L / (6L * 6L);
            case GraphicsFormat.RGBA_ASTC8X8_SRGB:
            case GraphicsFormat.RGBA_ASTC8X8_UNorm:
            case GraphicsFormat.RGBA_ASTC8X8_UFloat:
                return (ulong)(width*height) * 128L / (8L * 8L);
            case GraphicsFormat.RGBA_ASTC10X10_SRGB:
            case GraphicsFormat.RGBA_ASTC10X10_UNorm:
            case GraphicsFormat.RGBA_ASTC10X10_UFloat:
                return (ulong)(width*height) * 128L / (10L * 10L);
            case GraphicsFormat.RGBA_ASTC12X12_SRGB:
            case GraphicsFormat.RGBA_ASTC12X12_UNorm:
            case GraphicsFormat.RGBA_ASTC12X12_UFloat:
                return (ulong)(width*height) * 128L / (12L * 12L);
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Not a known graphics format (Or might be obsolete)...");
        }
    }
    public static ulong GetTotalBitsGraphicsMemory(this RenderTexture texture) {
        return GetTotalBitsGraphicsMemory(texture.width, texture.height, texture.depth, texture.graphicsFormat);
    }
}

}