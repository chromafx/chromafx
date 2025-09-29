/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

using ChromaFx.Core;
using ChromaFx.IO.Formats.Gif.Format.BaseClasses;
using ChromaFx.IO.Formats.Gif.Format.Helpers;

namespace ChromaFx.IO.Formats.Gif.Format;

/// <summary>
/// Image descriptor
/// </summary>
/// <seealso cref="SectionBase" />
/// <remarks>
/// Initializes a new instance of the <see cref="ImageDescriptor"/> class.
/// </remarks>
/// <param name="left">The left.</param>
/// <param name="top">The top.</param>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
/// <param name="localColorTableExists">if set to <c>true</c> [local color table exists].</param>
/// <param name="localColorTableSize">Size of the local color table.</param>
/// <param name="interlace">if set to <c>true</c> [interlace].</param>
public class ImageDescriptor(short left,
    short top,
    short width,
    short height,
    bool localColorTableExists,
    int localColorTableSize,
    bool interlace) : SectionBase
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageDescriptor"/> class.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="bitDepth">The bit depth.</param>
    public ImageDescriptor(Image image, int bitDepth)
        : this(0, 0, (short)image.Width, (short)image.Height, true, bitDepth - 1, false)
    {
    }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public short Height { get; } = height;

    /// <summary>
    /// Gets a value indicating whether this <see cref="ImageDescriptor"/> is interlace.
    /// </summary>
    /// <value>
    ///   <c>true</c> if interlace; otherwise, <c>false</c>.
    /// </value>
    public bool Interlace { get; private set; } = interlace;

    /// <summary>
    /// Gets the left.
    /// </summary>
    /// <value>
    /// The left.
    /// </value>
    public short Left { get; private set; } = left;

    /// <summary>
    /// Gets a value indicating whether [local color table exists].
    /// </summary>
    /// <value>
    /// <c>true</c> if [local color table exists]; otherwise, <c>false</c>.
    /// </value>
    public bool LocalColorTableExists { get; private set; } = localColorTableExists;

    /// <summary>
    /// Gets the size of the local color table.
    /// </summary>
    /// <value>
    /// The size of the local color table.
    /// </value>
    public int LocalColorTableSize { get; } = localColorTableSize;

    /// <summary>
    /// Gets the size.
    /// </summary>
    /// <value>
    /// The size.
    /// </value>
    public static int Size => 9;

    /// <summary>
    /// Gets the top.
    /// </summary>
    /// <value>
    /// The top.
    /// </value>
    public short Top { get; private set; } = top;

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public short Width { get; } = width;

    /// <summary>
    /// Reads from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The resulting ImageDescriptor</returns>
    public static ImageDescriptor Read(Stream stream)
    {
        var tempBuffer = new byte[Size];
        stream.Read(tempBuffer, 0, tempBuffer.Length);
        var packed = tempBuffer[8];
        return new ImageDescriptor(BitConverter.ToInt16(tempBuffer, 0),
            BitConverter.ToInt16(tempBuffer, 2),
            BitConverter.ToInt16(tempBuffer, 4),
            BitConverter.ToInt16(tempBuffer, 6),
            (packed & 0x80) >> 7 == 1,
            2 << (packed & 0x07),
            (packed & 0x40) >> 6 == 1);
    }

    /// <summary>
    /// Writes to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <returns>
    /// True if it writes successfully, false otherwise
    /// </returns>
    public override bool Write(EndianBinaryWriter writer)
    {
        writer.Write(SectionTypes.ImageLabel);
        writer.Write((ushort)0);
        writer.Write((ushort)0);
        writer.Write((ushort)Width);
        writer.Write((ushort)Height);

        var field = new PackedField();
        field.SetBit(0, true);
        field.SetBit(1, false);
        field.SetBit(2, false);
        field.SetBits(5, 3, LocalColorTableSize);

        writer.Write(field.Byte);
        return true;
    }
}