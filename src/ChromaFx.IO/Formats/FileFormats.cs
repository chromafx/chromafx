/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Ids for the codecs shipped by ChromaFx.IO.
/// </summary>
public static class FileFormats
{
    /// <summary>
    /// BMP.
    /// </summary>
    public static readonly ImageFormatKey Bmp = new("bmp");

    /// <summary>
    /// PNG.
    /// </summary>
    public static readonly ImageFormatKey Png = new("png");

    /// <summary>
    /// JPEG.
    /// </summary>
    public static readonly ImageFormatKey Jpeg = new("jpeg");

    /// <summary>
    /// GIF.
    /// </summary>
    public static readonly ImageFormatKey Gif = new("gif");

    /// <summary>
    /// TGA.
    /// </summary>
    public static readonly ImageFormatKey Tga = new("tga");

    /// <summary>
    /// TIFF.
    /// </summary>
    public static readonly ImageFormatKey Tiff = new("tiff");

    /// <summary>
    /// ICO.
    /// </summary>
    public static readonly ImageFormatKey Ico = new("ico");
}
