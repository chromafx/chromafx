/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Formats.Gif.Format.BaseClasses;
using ChromaFx.Formats.Gif.Format.Helpers;
using ChromaFx.IO;
using ChromaFx.Quantizers;

namespace ChromaFx.Formats.Gif.Format;

/// <summary>
/// Frame indices
/// </summary>
/// <seealso cref="SectionBase" />
/// <remarks>
/// Initializes a new instance of the <see cref="FrameIndices" /> class.
/// </remarks>
/// <param name="indices">The indices.</param>
/// <param name="bitDepth">The bit depth.</param>
public class FrameIndices(byte[] indices, byte bitDepth) : SectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameIndices"/> class.
    /// </summary>
    /// <param name="quantizedImage">The quantized image.</param>
    /// <param name="bitDepth">The bit depth.</param>
    public FrameIndices(QuantizedImage quantizedImage, int bitDepth)
        : this(quantizedImage.Pixels, (byte)bitDepth)
    {
    }

    /// <summary>
    /// Gets or sets the bit depth.
    /// </summary>
    /// <value>
    /// The bit depth.
    /// </value>
    public byte BitDepth { get; set; } = bitDepth;

    /// <summary>
    /// Gets or sets the indices.
    /// </summary>
    /// <value>
    /// The indices.
    /// </value>
    public byte[] Indices { get; set; } = indices;

    /// <summary>
    /// Reads from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>
    /// The resulting FrameIndices object
    /// </returns>
    public static FrameIndices Read(Stream stream, ImageDescriptor descriptor)
    {
        var dataSize = stream.ReadByte();
        var decoder = new LzwDecoder(stream);
        return new FrameIndices(decoder.Decode(descriptor.Width, descriptor.Height, dataSize), 0);
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
        var encoder = new LzwEncoder(Indices, BitDepth);
        encoder.Encode(writer.BaseStream);
        return true;
    }
}