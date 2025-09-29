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
/// Logical screen descriptor
/// </summary>
/// <seealso cref="SectionBase" />
/// <remarks>
/// Initializes a new instance of the <see cref="LogicalScreenDescriptor" /> class.
/// </remarks>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
/// <param name="backgroundColorIndex">Index of the background color.</param>
/// <param name="pixelAspectRatio">The pixel aspect ratio.</param>
/// <param name="globalColorTablePresent">if set to <c>true</c> [global color table present].</param>
/// <param name="globalColorTableSize">Size of the global color table.</param>
public class LogicalScreenDescriptor(short width,
    short height,
    byte backgroundColorIndex,
    byte pixelAspectRatio,
    bool globalColorTablePresent,
    int globalColorTableSize) : SectionBase
{

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalScreenDescriptor"/> class.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="transparencyIndex">Index of the transparency.</param>
    /// <param name="bitDepth">The bit depth.</param>
    public LogicalScreenDescriptor(Image image, int transparencyIndex, int bitDepth)
        : this((short)image.Width,
            (short)image.Height,
            (byte)(transparencyIndex > -1 ? transparencyIndex : 255),
            0,
            false,
            bitDepth - 1)
    {
    }

    /// <summary>
    /// Gets the index of the background color.
    /// </summary>
    /// <value>
    /// The index of the background color.
    /// </value>
    public byte BackgroundColorIndex { get; } = backgroundColorIndex;

    /// <summary>
    /// Gets a value indicating whether [global color table present].
    /// </summary>
    /// <value>
    /// <c>true</c> if [global color table present]; otherwise, <c>false</c>.
    /// </value>
    public bool GlobalColorTablePresent { get; } = globalColorTablePresent;

    /// <summary>
    /// Gets the size of the global color table.
    /// </summary>
    /// <value>
    /// The size of the global color table.
    /// </value>
    public int GlobalColorTableSize { get; } = globalColorTableSize;

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public short Height { get; } = height;

    /// <summary>
    /// Gets the pixel aspect ratio.
    /// </summary>
    /// <value>
    /// The pixel aspect ratio.
    /// </value>
    public byte PixelAspectRatio { get; } = pixelAspectRatio;

    /// <summary>
    /// Gets the size.
    /// </summary>
    /// <value>
    /// The size.
    /// </value>
    public static int Size => 7;

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
    /// <returns>The LogicalScreenDescriptor section read from the stream</returns>
    public static LogicalScreenDescriptor Read(Stream stream)
    {
        var buffer = new byte[Size];
        stream.Read(buffer, 0, buffer.Length);
        var packed = buffer[4];
        return new LogicalScreenDescriptor(BitConverter.ToInt16(buffer, 0),
            BitConverter.ToInt16(buffer, 2),
            buffer[5],
            buffer[6],
            (packed & 0x80) >> 7 == 1,
            2 << (packed & 0x07));
    }

    /// <summary>
    /// Writes to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <returns>True if it succeeds, false otherwise</returns>
    public override bool Write(EndianBinaryWriter writer)
    {
        writer.Write((ushort)Width);
        writer.Write((ushort)Height);

        var field = new PackedField();
        field.SetBit(0, GlobalColorTablePresent);
        field.SetBits(1, 3, GlobalColorTableSize);
        field.SetBit(4, false);
        field.SetBits(5, 3, GlobalColorTableSize);

        byte[] arr = [
            field.Byte,
            BackgroundColorIndex,
            PixelAspectRatio
        ];
        writer.Write(arr);
        return true;
    }
}