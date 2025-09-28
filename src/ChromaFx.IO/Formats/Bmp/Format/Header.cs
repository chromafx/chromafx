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

namespace ChromaFx.IO.Formats.Bmp.Format;

/// <summary>
/// BMP Header
/// </summary>
public class Header
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class.
    /// </summary>
    /// <param name="data">The header data.</param>
    public Header(byte[] data)
        : this(
            BitConverter.ToInt32(data, 4),
            BitConverter.ToInt32(data, 8),
            BitConverter.ToInt16(data, 14),
            BitConverter.ToInt32(data, 20),
            BitConverter.ToInt32(data, 24),
            BitConverter.ToInt32(data, 28),
            BitConverter.ToInt32(data, 32),
            BitConverter.ToInt32(data, 36),
            (Compression)BitConverter.ToInt32(data, 16),
            BitConverter.ToInt32(data, 0),
            BitConverter.ToInt32(data, 16) == 3 ? BitConverter.ToInt32(data, 40) : 0,
            BitConverter.ToInt32(data, 16) == 3 ? BitConverter.ToInt32(data, 44) : 0,
            BitConverter.ToInt32(data, 16) == 3 ? BitConverter.ToInt32(data, 48) : 0,
            BitConverter.ToInt32(data, 16) == 3 ? BitConverter.ToInt32(data, 52) : 0
        )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Header" /> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="bpp">The BPP.</param>
    /// <param name="imageSize">Size of the image.</param>
    /// <param name="xppm">The XPPM.</param>
    /// <param name="yppm">The yppm.</param>
    /// <param name="colorsUsed">The number of colors used.</param>
    /// <param name="colorsImportant">The number of important colors.</param>
    /// <param name="compression">The compression.</param>
    /// <param name="size">The size.</param>
    /// <param name="redMask">The red mask.</param>
    /// <param name="greenMask">The green mask.</param>
    /// <param name="blueMask">The blue mask.</param>
    /// <param name="alphaMask">The alpha mask.</param>
    public Header(int width, int height, short bpp, int imageSize, int xppm, int yppm, int colorsUsed,
        int colorsImportant, Compression compression, int size = 40,
        int redMask = 0, int greenMask = 0, int blueMask = 0, int alphaMask = 0)
    {
        AlphaMask = alphaMask;
        BlueMask = blueMask;
        GreenMask = greenMask;
        RedMask = redMask;
        Size = size;
        Width = width;
        Height = height;
        Bpp = bpp;
        ImageSize = imageSize;
        Xppm = xppm;
        Yppm = yppm;
        ColorsUsed = colorsUsed;
        ColorsImportant = colorsImportant;
        Compression = compression;

        AlphaOffset = GetOffset(alphaMask);
        RedOffset = GetOffset(redMask);
        GreenOffset = GetOffset(greenMask);
        BlueOffset = GetOffset(blueMask);

        AlphaMultiplier = GetMultiplier(alphaMask, AlphaOffset);
        RedMultiplier = GetMultiplier(redMask, RedOffset);
        GreenMultiplier = GetMultiplier(greenMask, GreenOffset);
        BlueMultiplier = GetMultiplier(blueMask, BlueOffset);
    }

    /// <summary>
    /// Gets or sets the alpha mask.
    /// </summary>
    /// <value>
    /// The alpha mask.
    /// </value>
    public int AlphaMask { get; set; }

    /// <summary>
    /// Gets or sets the alpha multiplier.
    /// </summary>
    /// <value>
    /// The alpha multiplier.
    /// </value>
    public double AlphaMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the alpha offset.
    /// </summary>
    /// <value>
    /// The alpha offset.
    /// </value>
    public int AlphaOffset { get; set; }

    /// <summary>
    /// Gets or sets the blue mask.
    /// </summary>
    /// <value>
    /// The blue mask.
    /// </value>
    public int BlueMask { get; set; }

    /// <summary>
    /// Gets or sets the blue multiplier.
    /// </summary>
    /// <value>
    /// The blue multiplier.
    /// </value>
    public double BlueMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the blue offset.
    /// </summary>
    /// <value>
    /// The blue offset.
    /// </value>
    public int BlueOffset { get; set; }

    /// <summary>
    /// The bits per pixel
    /// </summary>
    /// <value>
    /// The bits per pixel
    /// </value>
    public short Bpp { get; }

    /// <summary>
    /// Gets the number of important colors.
    /// </summary>
    /// <value>The number of important colors.</value>
    public int ColorsImportant { get; }

    /// <summary>
    /// Gets the number of colors used.
    /// </summary>
    /// <value>The number of colors used.</value>
    public int ColorsUsed { get; }

    /// <summary>
    /// Gets or sets the compression.
    /// </summary>
    /// <value>The compression.</value>
    public Compression Compression { get; }

    /// <summary>
    /// Gets or sets the green mask.
    /// </summary>
    /// <value>
    /// The green mask.
    /// </value>
    public int GreenMask { get; set; }

    /// <summary>
    /// Gets or sets the green multiplier.
    /// </summary>
    /// <value>
    /// The green multiplier.
    /// </value>
    public double GreenMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the green offset.
    /// </summary>
    /// <value>
    /// The green offset.
    /// </value>
    public int GreenOffset { get; set; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; }

    /// <summary>
    /// Gets the size of the image.
    /// </summary>
    /// <value>The size of the image.</value>
    public int ImageSize { get; }

    /// <summary>
    /// Gets the number of planes.
    /// </summary>
    /// <value>The number of planes.</value>
    public static short Planes => 1;

    /// <summary>
    /// Gets or sets the red mask.
    /// </summary>
    /// <value>
    /// The red mask.
    /// </value>
    public int RedMask { get; set; }

    /// <summary>
    /// Gets or sets the red multiplier.
    /// </summary>
    /// <value>
    /// The red multiplier.
    /// </value>
    public double RedMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the red offset.
    /// </summary>
    /// <value>
    /// The red offset.
    /// </value>
    public int RedOffset { get; set; }

    /// <summary>
    /// Gets the size of the header
    /// </summary>
    /// <value>The size of the header</value>
    public int Size { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; }

    /// <summary>
    /// Pels Per Meter in the X direction
    /// </summary>
    /// <value>The XPPM.</value>
    public int Xppm { get; }

    /// <summary>
    /// Pels Per Meter in the Y direction
    /// </summary>
    /// <value>The yppm.</value>
    public int Yppm { get; }

    /// <summary>
    /// Reads the header information from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The header information.</returns>
    public static Header Read(Stream stream)
    {
        var headerSize = new byte[4];
        stream.Read(headerSize, 0, 4);
        var size = BitConverter.ToInt32(headerSize, 0);
        var data = new byte[size];
        stream.Read(data, 4, size - 4);
        data[0] = headerSize[0];
        data[1] = headerSize[1];
        data[2] = headerSize[2];
        data[3] = headerSize[3];
        return new Header(data);
    }

    /// <summary>
    /// Writes the data to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public void Write(BinaryWriter writer)
    {
        writer.Write(Size);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(Planes);
        writer.Write(Bpp);
        writer.Write((int)Compression);
        writer.Write(ImageSize);
        writer.Write(Xppm);
        writer.Write(Yppm);
        writer.Write(ColorsUsed);
        writer.Write(ColorsImportant);
    }

    /// <summary>
    /// Gets the multiplier.
    /// </summary>
    /// <param name="mask">The mask.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>
    /// The multiplier
    /// </returns>
    private static double GetMultiplier(int mask, int offset)
    {
        if (mask != 0)
        {
            mask >>= offset;
            return 255d / mask;
        }
        return 1;
    }

    /// <summary>
    /// Gets the offset.
    /// </summary>
    /// <param name="mask">The mask.</param>
    /// <returns>The offset</returns>
    private static int GetOffset(int mask)
    {
        var offset = 0;
        if (mask != 0)
        {
            while ((mask & 1) == 0)
            {
                ++offset;
                mask >>= 1;
            }
        }
        return offset;
    }
}