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

namespace ChromaFx.Core;

/// <summary>
/// Optional still-image metadata (EXIF TIFF IFD bytes and/or XMP packet).
/// Decode may populate these; encode may write them back (JPEG/PNG).
/// Orientation tag 274 is rewritten to 1 on write because pixels are already upright.
/// </summary>
public sealed class ImageMetadata
{
    /// <summary>
    /// Raw TIFF EXIF payload (starts with <c>II</c> or <c>MM</c>), without the JPEG <c>Exif\0\0</c> prefix.
    /// </summary>
    public byte[] Exif { get; set; }

    /// <summary>
    /// XMP packet as UTF-8 text (may include <c>xpacket</c> wrapper).
    /// </summary>
    public string Xmp { get; set; }

    /// <summary>
    /// True when neither EXIF nor XMP is set.
    /// </summary>
    public bool IsEmpty =>
        (Exif is null || Exif.Length == 0) && string.IsNullOrEmpty(Xmp);

    /// <summary>
    /// Deep-copies EXIF bytes and XMP string.
    /// </summary>
    public ImageMetadata Clone() =>
        new()
        {
            Exif = Exif is null ? null : (byte[])Exif.Clone(),
            Xmp = Xmp,
        };
}
