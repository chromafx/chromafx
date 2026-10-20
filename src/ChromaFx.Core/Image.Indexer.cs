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

using ChromaFx.Core.Colors;

namespace ChromaFx.Core;

public partial class Image
{
    /// <summary>
    /// Gets or sets the pixel at the specified coordinates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates are outside the image.</exception>
    public Color this[int x, int y]
    {
        get
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
                throw new ArgumentOutOfRangeException(
                    $"Pixel ({x}, {y}) is outside {Width}x{Height}."
                );
            return Pixels[y * Width + x];
        }
        set
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
                throw new ArgumentOutOfRangeException(
                    $"Pixel ({x}, {y}) is outside {Width}x{Height}."
                );
            Pixels[y * Width + x] = value;
        }
    }

    /// <summary>
    /// Gets the pixels as a span.
    /// </summary>
    public Span<Color> PixelSpan => Pixels.AsSpan();

    /// <summary>
    /// Gets the pixels as memory.
    /// </summary>
    public Memory<Color> PixelMemory => Pixels.AsMemory();

    /// <summary>
    /// Creates a deep copy of this image.
    /// </summary>
    public Image Clone()
    {
        var data = new Color[Width * Height];
        Pixels.AsSpan().CopyTo(data);
        return new Image(Width, Height, data);
    }
}
