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

using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Formats;
using ChromaFx.Colors;
using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace ChromaFx;

/// <summary>
/// Represents an image
/// </summary>
public partial class Image
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Image(int width, int height)
        : this(width, height, new Color[width * height])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data.</param>
    public Image(int width, int height, Color[] data)
    {
        ReCreate(width, height, data);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data.</param>
    public Image(int width, int height, byte[] data)
    {
        if (data == null)
        {
            ReCreate(width, height, null);
            return;
        }
        var returnValues = new Color[width * height];
        for (var x = 0; x < returnValues.Length; ++x)
        {
            returnValues[x].Red = data[x * 4];
            returnValues[x].Green = data[x * 4 + 1];
            returnValues[x].Blue = data[x * 4 + 2];
            returnValues[x].Alpha = data[x * 4 + 3];
        }
        ReCreate(width, height, returnValues);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public Image(string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var tempImage = new Manager().Decode(stream);
        ReCreate(tempImage.Width, tempImage.Height, tempImage.Pixels);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="stream">The stream to copy the data from.</param>
    public Image(Stream stream)
        : this(new Manager().Decode(stream))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="image">The image to copy the data from.</param>
    public Image(Image image)
        : this(image.Width, image.Height, image.Pixels)
    {
    }

    /// <summary>
    /// Gets the center.
    /// </summary>
    /// <value>The center.</value>
    public Vector2 Center { get; private set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the pixel ratio.
    /// </summary>
    /// <value>The pixel ratio.</value>
    public double PixelRatio { get; private set; }

    /// <summary>
    /// Gets or sets the pixels.
    /// </summary>
    /// <value>The pixels.</value>
    public Color[] Pixels { get; private set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; private set; }

    /// <summary>
    /// The ASCII characters used
    /// </summary>
    private static readonly string[] AsciiCharacters = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };

    /// <summary>
    /// Makes a copy of this image.
    /// </summary>
    /// <returns>A copy of this image.</returns>
    public Image Copy()
    {
        var data = new Color[Width * Height];
        Array.Copy(Pixels, data, data.Length);
        return new Image(Width, Height, data);
    }

    /// <summary>
    /// Recreates the image object using the new data.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="data">The new pixel data.</param>
    /// <returns>this</returns>
    public Image ReCreate(int width, int height, Color[] data)
    {
        Width = width < 1 ? 1 : width;
        Height = height < 1 ? 1 : height;
        PixelRatio = (double)Width / Height;
        Center = new Vector2(Width >> 1, Height >> 1);
        if (data == null)
            return this;
        Pixels = new Color[width * height];
        Array.Copy(data, Pixels, Pixels.Length);
        return this;
    }

    /// <summary>
    /// Recreates the image object using the new width and height.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <returns>this</returns>
    public Image ReCreate(int width, int height)
    {
        Width = width < 1 ? 1 : width;
        Height = height < 1 ? 1 : height;
        PixelRatio = (double)Width / Height;
        Center = new Vector2(Width >> 1, Height >> 1);
        Pixels = new Color[width * height];
        return this;
    }

    /// <summary>
    /// Saves the image to the specified file name.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>True if it saves successfully, false otherwise</returns>
    public bool Save(string fileName)
    {
        return new Manager().Encode(fileName, this);
    }

    /// <summary>
    /// Saves the image to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="format">The format.</param>
    /// <returns>True if it saves successfully, false otherwise</returns>
    public bool Save(Stream stream, FileFormats format)
    {
        return new Manager().Encode(stream, this, format);
    }

    /// <summary>
    /// Converts the image to ASCII art.
    /// </summary>
    /// <returns>The image as ASCII art</returns>
    public string ToAsciiArt()
    {
        var showLine = true;
        var tempImage = new Greyscale601().Apply(Copy());
        var builder = new StringBuilder();
        for (var y = 0; y < tempImage.Height; ++y)
        {
            for (var x = 0; x < tempImage.Width; ++x)
            {
                if (!showLine) continue;
                var rValue = tempImage.Pixels[y * tempImage.Width + x].Red / 255f;
                builder.Append(AsciiCharacters[(int)(rValue * AsciiCharacters.Length)]);
            }
            if (showLine)
            {
                builder.AppendLine();
                showLine = false;
            }
            else
            {
                showLine = true;
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Returns a base64 <see cref="string"/> that represents this instance.
    /// </summary>
    /// <param name="desiredFormat">The desired format.</param>
    /// <returns>A <see cref="string"/> that represents this instance as a base64 instance.</returns>
    public string ToString(FileFormats desiredFormat)
    {
        using var stream = new MemoryStream();
        if (!Save(stream, desiredFormat)) return string.Empty;
        var tempArray = stream.ToArray();
        return Convert.ToBase64String(tempArray, 0, tempArray.Length);
    }
}