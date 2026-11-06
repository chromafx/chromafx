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

internal static unsafe partial class JxlLib
{
    private const string Lib = "chromajxl";

    internal const int Ok = 0;
    internal const int ErrAnimation = 2;

    [LibraryImport(Lib, EntryPoint = "chroma_jxl_decode")]
    internal static partial int Decode(
        byte* data,
        nuint size,
        byte** rgba,
        int* width,
        int* height
    );

    [LibraryImport(Lib, EntryPoint = "chroma_jxl_encode")]
    internal static partial int Encode(
        byte* rgba,
        int width,
        int height,
        int stride,
        int quality,
        int lossless,
        byte** output,
        nuint* outputSize
    );

    [LibraryImport(Lib, EntryPoint = "chroma_jxl_free")]
    internal static partial void Free(void* pointer);
}
