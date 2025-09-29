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
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats.Jpeg.Format.Enums;
using ChromaFx.IO.Formats.Jpeg.Format.HelperClasses;
using ChromaFx.IO.Formats.Jpeg.Format.Segments.BaseClasses;

namespace ChromaFx.IO.Formats.Jpeg.Format.Segments;

/// <summary>
/// Start of frame segment holder
/// </summary>
public class StartOfFrame : SegmentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartOfFrame" /> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="buffer">The buffer.</param>
    public StartOfFrame(SegmentTypes type, ByteBuffer buffer)
        : base(type, buffer)
    {
        Components = new Component[MaxComponents];
        for (var x = 0; x < Components.Length; ++x)
            Components[x] = new Component();
        Progressive = type == SegmentTypes.StartOfFrame2;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StartOfFrame"/> class.
    /// </summary>
    /// <param name="image">The image.</param>
    public StartOfFrame(Image image)
        : base(SegmentTypes.StartOfFrame0, null)
    {
        Height = image.Height;
        Width = image.Width;
        Components = new Component[3];
        TypeOfImage = ImageType.Rgb;
    }

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    /// <value>
    /// The components.
    /// </value>
    public Component[] Components { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="StartOfFrame"/> is progressive.
    /// </summary>
    /// <value>
    ///   <c>true</c> if progressive; otherwise, <c>false</c>.
    /// </value>
    public bool Progressive { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public ImageType TypeOfImage { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public int Width { get; set; }

    /// <summary>
    /// The maximum number of components
    /// </summary>
    private const int MaxComponents = 4;

    /// <summary>
    /// The maximum tq
    /// </summary>
    private const int MaximumTq = 3;

    /// <summary>
    /// Setups the specified segments.
    /// </summary>
    /// <param name="segments">The segments.</param>
    /// <exception cref="ImageException">
    /// Image type unknown
    /// or
    /// Precision not supported
    /// or
    /// SOF has wrong length
    /// or
    /// Repeated component identifier
    /// or
    /// Bad Tq value
    /// or
    /// Unsupported Luma/chroma subsampling ratio
    /// or
    /// Lnsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// or
    /// unsupported subsampling ratio
    /// </exception>
    public override void Setup(IEnumerable<SegmentBase> segments)
    {
        Length = GetLength(Bytes);
        switch (Length)
        {
            case 6 + 3 * 1:
                TypeOfImage = ImageType.GreyScale;
                break;

            case 6 + 3 * 3:
                TypeOfImage = ImageType.Rgb;
                break;

            case 6 + 3 * 4:
                TypeOfImage = ImageType.Cmyk;
                break;

            default:
                throw new ImageException("Image type unknown");
        }

        Bytes.ReadFull(TempData, 0, Length);
        if (TempData[0] != 8)
            throw new ImageException("Precision not supported");

        Height = (TempData[1] << 8) + TempData[2];
        Width = (TempData[3] << 8) + TempData[4];
        if (TempData[5] != (int)TypeOfImage)
            throw new ImageException("SOF has wrong length");

        for (var x = 0; x < (int)TypeOfImage; ++x)
        {
            Components[x].ComponentIdentifier = TempData[6 + 3 * x];
            for (var y = 0; y < x; ++y)
            {
                if (Components[x].ComponentIdentifier == Components[y].ComponentIdentifier)
                    throw new ImageException("Repeated component identifier");
            }

            Components[x].QuatizationTableDestSelector = TempData[8 + 3 * x];
            if (Components[x].QuatizationTableDestSelector > MaximumTq)
                throw new ImageException("Bad Tq value");

            var hv = TempData[7 + 3 * x];
            var h = hv >> 4;
            var v = hv & 0x0f;
            if (h < 1 || h > 4 || v < 1 || v > 4)
                throw new ImageException("Unsupported Luma/chroma subsampling ratio");
            if (h == 3 || v == 3)
                throw new ImageException("Lnsupported subsampling ratio");

            switch (TypeOfImage)
            {
                case ImageType.GreyScale:
                    h = 1;
                    v = 1;
                    break;

                case ImageType.Rgb:
                    switch (x)
                    {
                        case 0:
                        {
                            if (v == 4)
                                throw new ImageException("unsupported subsampling ratio");
                            break;
                        }
                        case 1:
                        {
                            if (Components[0].HorizontalSamplingFactor % h != 0 || Components[0].VerticalSamplingFactor % v != 0)
                                throw new ImageException("unsupported subsampling ratio");
                            break;
                        }
                        case 2:
                        {
                            if (Components[1].HorizontalSamplingFactor != h || Components[1].VerticalSamplingFactor != v)
                                throw new ImageException("unsupported subsampling ratio");
                            break;
                        }
                    }
                    break;

                case ImageType.Cmyk:
                    switch (x)
                    {
                        case 0:
                            if (hv != 0x11 && hv != 0x22)
                                throw new ImageException("unsupported subsampling ratio");
                            break;

                        case 1:
                        case 2:
                            if (hv != 0x11)
                                throw new ImageException("unsupported subsampling ratio");
                            break;

                        case 3:
                            if (Components[0].HorizontalSamplingFactor != h || Components[0].VerticalSamplingFactor != v)
                                throw new ImageException("unsupported subsampling ratio");
                            break;
                    }
                    break;
            }

            Components[x].HorizontalSamplingFactor = h;
            Components[x].VerticalSamplingFactor = v;
        }
    }

    /// <summary>
    /// Writes the information to the specified writer.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    public override void Write(BinaryWriter writer)
    {
        byte[] subsamples = [0x11, 0x11, 0x11];
        byte[] chroma = [0x00, 0x01, 0x01];

        Length = 8 + 3 * Components.Length;
        WriteSegmentHeader(writer);
        var buffer = new byte[16];
        buffer[0] = 8;
        buffer[1] = (byte)(Height >> 8);
        buffer[2] = (byte)(Height & 0xff);
        buffer[3] = (byte)(Width >> 8);
        buffer[4] = (byte)(Width & 0xff);
        buffer[5] = 3;
        for (var i = 0; i < 3; i++)
        {
            buffer[3 * i + 6] = (byte)(i + 1);
            buffer[3 * i + 7] = subsamples[i];
            buffer[3 * i + 8] = chroma[i];
        }
        writer.Write(buffer, 0, 3 * 2 + 9);
    }
}