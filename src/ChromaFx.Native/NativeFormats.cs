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
/// Ids for the codecs shipped by ChromaFx.Native.
/// Referencing this package registers the codecs (still and animated).
/// </summary>
public static class NativeFormats
{
    /// <summary>
    /// WebP (still and animated).
    /// </summary>
    public static readonly ImageFormatKey WebP = new("webp");

    /// <summary>
    /// AVIF (still and animated).
    /// </summary>
    public static readonly ImageFormatKey Avif = new("avif");

    /// <summary>
    /// JPEG XL (still and animated).
    /// </summary>
    public static readonly ImageFormatKey JpegXl = new("jxl");
}
