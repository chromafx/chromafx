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

using System.Numerics;
using ChromaFx.Core.Colors;

namespace ChromaFx.Core;

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
        : this(width, height, new Color[width * height]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data.</param>
    public Image(int width, int height, Color[] data) => ReCreate(width, height, data);

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data.</param>
    public Image(int width, int height, byte[] data)
    {
        if (data is null)
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
    /// <param name="image">The image to copy the data from.</param>
    public Image(Image image)
        : this(image.Width, image.Height, image.Pixels) { }

    /// <summary>
    /// Gets the center.
    /// </summary>
    public Vector2 Center { get; private set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the pixel ratio.
    /// </summary>
    public double PixelRatio { get; private set; }

    /// <summary>
    /// Gets or sets the pixels.
    /// </summary>
    public Color[] Pixels { get; private set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int Width { get; private set; }

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
        if (data is null)
            return this;
        Pixels = new Color[width * height];
        data.AsSpan().CopyTo(Pixels);
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
    /// Makes a copy of this image.
    /// </summary>
    /// <returns>A copy of this image.</returns>
    public Image Copy()
    {
        var data = new Color[Width * Height];
        Pixels.AsSpan().CopyTo(data);
        return new Image(Width, Height, data);
    }
}
