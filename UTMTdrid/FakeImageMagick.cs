#nullable enable

using System;
using System.Runtime.InteropServices;

namespace ImageMagick;

internal partial class Environment
{
    private unsafe static class NativeMethods
    {
        public static class Arm64
        {
            [DllImport("Magick.Native-Android.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Environment_Initialize")]
            public static extern void Environment_Initialize();
            [DllImport("Magick.Native-Android.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Environment_GetEnv")]
            public static extern IntPtr Environment_GetEnv(IntPtr name);
            [DllImport("Magick.Native-Android.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Environment_SetEnv")]
            public static extern void Environment_SetEnv(IntPtr name, IntPtr value);
        }
    }
}
