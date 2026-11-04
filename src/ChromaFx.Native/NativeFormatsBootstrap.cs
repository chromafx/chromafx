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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ChromaFx.IO;
using ChromaFx.Native.Formats.Avif;
using ChromaFx.Native.Formats.WebP;

namespace ChromaFx.Native;

/// <summary>
/// Registers stub WebP/AVIF formats when this package is referenced.
/// </summary>
internal static class NativeFormatsBootstrap
{
    [ModuleInitializer]
    [UnconditionalSuppressMessage(
        "AOT",
        "CA2255",
        Justification = "Package entry bootstrap for ImageFormats.Register."
    )]
    internal static void Initialize()
    {
        ImageFormats.EnsureBuiltIns();
        ImageFormats.Register(new WebPFormat());
        ImageFormats.Register(new AvifFormat());
    }
}
