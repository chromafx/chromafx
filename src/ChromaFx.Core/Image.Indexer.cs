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
            return _buffer.Span[y * Stride + x];
        }
        set
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
                throw new ArgumentOutOfRangeException(
                    $"Pixel ({x}, {y}) is outside {Width}x{Height}."
                );
            _buffer.Span[y * Stride + x] = value;
        }
    }

    /// <summary>
    /// Gets the full storage span (includes row padding). Index as <c>y * Stride + x</c>.
    /// Prefer <see cref="GetRowSpan"/> for row work.
    /// </summary>
    public Span<Color> PixelSpan => _buffer.Span;

    /// <summary>
    /// Gets the full storage memory (includes row padding).
    /// </summary>
    public Memory<Color> PixelMemory => _buffer.Memory;

    /// <summary>
    /// Gets the active pixels for row <paramref name="y"/>.
    /// </summary>
    public Span<Color> GetRowSpan(int y) => _buffer.GetRowSpan(y);

    /// <summary>
    /// Reinterprets full storage as raw bytes (includes row padding).
    /// For encoders, prefer <c>MemoryMarshal.AsBytes(GetRowSpan(y))</c>.
    /// </summary>
    public Span<byte> AsBytes() => _buffer.AsBytes();

    /// <summary>
    /// Reinterprets full storage as packed <see cref="uint"/> values.
    /// </summary>
    public Span<uint> AsUInt32() => _buffer.AsUInt32();

    /// <summary>
    /// Copies logical pixels into a tightly packed destination (<see cref="PixelCount"/> elements).
    /// </summary>
    public void CopyPixelsTo(Span<Color> destination) => _buffer.CopyTo(destination);

    /// <summary>
    /// Copies logical pixels as tightly packed RGBA bytes (length = <see cref="PixelCount"/> × 4).
    /// </summary>
    public void CopyRgbaBytesTo(Span<byte> destination)
    {
        var needed = PixelCount * 4;
        if (destination.Length < needed)
            throw new ArgumentException("Destination is too short.", nameof(destination));

        var offset = 0;
        for (var y = 0; y < Height; y++)
        {
            System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowSpan(y))
                .CopyTo(destination.Slice(offset, Width * 4));
            offset += Width * 4;
        }
    }

    /// <summary>
    /// Creates a deep copy of this image.
    /// </summary>
    public Image Clone() => new(this);
}
