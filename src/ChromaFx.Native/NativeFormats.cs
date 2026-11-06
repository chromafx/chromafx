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

using ChromaFx.IO.Formats;

namespace ChromaFx.Native;

/// <summary>
/// Ids for the still-image codecs shipped by ChromaFx.Native.
/// Referencing this package registers the codecs.
/// </summary>
public static class NativeFormats
{
    /// <summary>
    /// Still WebP. Animated WebP is rejected.
    /// </summary>
    public static readonly ImageFormatKey WebP = new("webp");

    /// <summary>
    /// Still AVIF. Animated AVIF is rejected.
    /// </summary>
    public static readonly ImageFormatKey Avif = new("avif");

    /// <summary>
    /// Still JPEG XL. Animated JPEG XL is rejected.
    /// </summary>
    public static readonly ImageFormatKey JpegXl = new("jxl");
}
