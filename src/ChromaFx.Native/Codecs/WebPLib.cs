/*
 * Copyright 2026 Ho Tzin Mein
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Runtime.InteropServices;

namespace ChromaFx.Native.Codecs;

[StructLayout(LayoutKind.Sequential)]
internal struct WebPBitstreamFeatures
{
    internal int Width;
    internal int Height;
    internal int HasAlpha;
    internal int HasAnimation;
    internal int Format;
    internal uint Pad0;
    internal uint Pad1;
    internal uint Pad2;
    internal uint Pad3;
    internal uint Pad4;
}

internal static unsafe partial class WebPLib
{
    internal const int DecoderAbiVersion = 0x0210;
    private const string Lib = "webp";

    [LibraryImport(Lib, EntryPoint = "WebPGetFeaturesInternal")]
    internal static partial int WebPGetFeaturesInternal(
        byte* data,
        nuint size,
        WebPBitstreamFeatures* features,
        int version
    );

    [LibraryImport(Lib, EntryPoint = "WebPDecodeRGBA")]
    internal static partial byte* WebPDecodeRGBA(byte* data, nuint size, int* width, int* height);

    [LibraryImport(Lib, EntryPoint = "WebPEncodeRGBA")]
    internal static partial nuint WebPEncodeRGBA(
        byte* rgba,
        int width,
        int height,
        int stride,
        float quality,
        byte** output
    );

    [LibraryImport(Lib, EntryPoint = "WebPEncodeLosslessRGBA")]
    internal static partial nuint WebPEncodeLosslessRGBA(
        byte* rgba,
        int width,
        int height,
        int stride,
        byte** output
    );

    [LibraryImport(Lib, EntryPoint = "WebPFree")]
    internal static partial void WebPFree(void* pointer);
}
