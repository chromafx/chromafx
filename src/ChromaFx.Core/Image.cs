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

using System.Numerics;
using ChromaFx.Core.Colors;

namespace ChromaFx.Core;

/// <summary>
/// Represents an image
/// </summary>
public partial class Image
{
    private PixelBuffer _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Image(int width, int height) => ReCreate(width, height);

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data (copied into owned storage).</param>
    public Image(int width, int height, Color[] data) => ReCreate(width, height, data);

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">The data (copied into owned storage).</param>
    public Image(int width, int height, ReadOnlySpan<Color> data) => ReCreate(width, height, data);

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="data">RGBA byte data (copied into owned storage).</param>
    public Image(int width, int height, byte[] data)
    {
        if (data is null)
        {
            ReCreate(width, height);
            return;
        }

        ReCreate(width, height);
        var count = Math.Min(PixelCount, data.Length / 4);
        for (var i = 0; i < count; ++i)
        {
            var y = i / Width;
            var x = i % Width;
            ref var pixel = ref GetRowSpan(y)[x];
            pixel.Red = data[i * 4];
            pixel.Green = data[i * 4 + 1];
            pixel.Blue = data[i * 4 + 2];
            pixel.Alpha = data[i * 4 + 3];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="image">The image to copy the data from.</param>
    public Image(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        ReCreate(image.Width, image.Height);
        CopyPixelsFrom(image);
    }

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
    /// Gets the number of logical pixels (<see cref="Width"/> × <see cref="Height"/>).
    /// </summary>
    public int PixelCount => _buffer.PixelCount;

    /// <summary>
    /// Gets the storage stride in pixels (≥ <see cref="Width"/>).
    /// Index flat storage as <c>y * Stride + x</c>.
    /// </summary>
    public int Stride => _buffer.Stride;

    /// <summary>
    /// Gets the owned pixel buffer.
    /// </summary>
    public PixelBuffer Buffer => _buffer;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Recreates the image object using the new data (copied into owned storage).
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="data">The new pixel data.</param>
    /// <returns>this</returns>
    public Image ReCreate(int width, int height, Color[] data) =>
        ReCreate(width, height, data is null ? default : data.AsSpan());

    /// <summary>
    /// Recreates the image object using the new data (copied into owned storage).
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="data">The new pixel data.</param>
    /// <returns>this</returns>
    public Image ReCreate(int width, int height, ReadOnlySpan<Color> data)
    {
        Width = width < 1 ? 1 : width;
        Height = height < 1 ? 1 : height;
        PixelRatio = (double)Width / Height;
        Center = new Vector2(Width >> 1, Height >> 1);
        _buffer = data.IsEmpty
            ? new PixelBuffer(Width, Height)
            : new PixelBuffer(Width, Height, data);
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
        _buffer = new PixelBuffer(Width, Height);
        return this;
    }

    /// <summary>
    /// Copies logical pixels from <paramref name="source"/> into this image (same dimensions).
    /// </summary>
    internal void CopyPixelsFrom(Image source)
    {
        if (source.Width != Width || source.Height != Height)
            throw new ArgumentException("Source dimensions must match.", nameof(source));

        if (source.Stride == Stride)
        {
            source.PixelSpan.CopyTo(PixelSpan);
            return;
        }

        for (var y = 0; y < Height; y++)
            source.GetRowSpan(y).CopyTo(GetRowSpan(y));
    }
}
