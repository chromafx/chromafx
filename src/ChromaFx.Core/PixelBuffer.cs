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

using System.Runtime.InteropServices;
using ChromaFx.Core.Colors;

namespace ChromaFx.Core;

/// <summary>
/// Owned packed RGBA8 pixel storage with SIMD-friendly row stride.
/// Prefer <see cref="GetRowSpan"/> over flat <c>y * Width + x</c> indexing;
/// use <c>y * Stride + x</c> when indexing <see cref="Span"/>.
/// </summary>
public sealed class PixelBuffer
{
    /// <summary>
    /// Row alignment in pixels (8 × 4 bytes = 32-byte rows when width is unaligned).
    /// </summary>
    public const int StrideAlignment = 8;

    private readonly Color[] _pixels;

    /// <summary>
    /// Creates an empty buffer of the given dimensions (pixels default to zero).
    /// </summary>
    public PixelBuffer(int width, int height)
    {
        Width = width < 1 ? 1 : width;
        Height = height < 1 ? 1 : height;
        Stride = AlignStride(Width);
        _pixels = new Color[Height * Stride];
    }

    /// <summary>
    /// Creates a buffer and copies <paramref name="source"/> into it.
    /// Tight sources (<c>Width * Height</c>) are copied row-by-row; full storage
    /// sources (<c>Height * Stride</c>) are copied contiguously.
    /// </summary>
    public PixelBuffer(int width, int height, ReadOnlySpan<Color> source)
        : this(width, height)
    {
        CopyFrom(source);
    }

    /// <summary>
    /// Pixel width.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Pixel height.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Number of pixels per row in storage (≥ <see cref="Width"/>, SIMD-aligned).
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// Logical pixel count (<see cref="Width"/> × <see cref="Height"/>).
    /// </summary>
    public int PixelCount => Width * Height;

    /// <summary>
    /// Storage length including row padding (<see cref="Height"/> × <see cref="Stride"/>).
    /// </summary>
    public int Length => Height * Stride;

    /// <summary>
    /// True when rows have no padding.
    /// </summary>
    public bool IsContiguous => Stride == Width;

    /// <summary>
    /// Full storage span (includes row padding). Index as <c>y * Stride + x</c>.
    /// </summary>
    public Span<Color> Span => _pixels.AsSpan(0, Length);

    /// <summary>
    /// Full storage memory (includes row padding).
    /// </summary>
    public Memory<Color> Memory => _pixels.AsMemory(0, Length);

    /// <summary>
    /// Returns the active pixels for row <paramref name="y"/> (length = <see cref="Width"/>).
    /// </summary>
    public Span<Color> GetRowSpan(int y)
    {
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));
        return _pixels.AsSpan(y * Stride, Width);
    }

    /// <summary>
    /// Returns the full storage row including padding (length = <see cref="Stride"/>).
    /// </summary>
    public Span<Color> GetRowSpanPadded(int y)
    {
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));
        return _pixels.AsSpan(y * Stride, Stride);
    }

    /// <summary>
    /// Reinterprets full storage as raw bytes (includes row padding).
    /// Prefer <see cref="GetRowSpan"/> + <see cref="MemoryMarshal.AsBytes{T}(Span{T})"/> for IO.
    /// </summary>
    public Span<byte> AsBytes() => MemoryMarshal.AsBytes(Span);

    /// <summary>
    /// Reinterprets full storage as packed <see cref="uint"/> values.
    /// </summary>
    public Span<uint> AsUInt32() => MemoryMarshal.Cast<Color, uint>(Span);

    /// <summary>
    /// Copies logical pixels into a tight destination (<see cref="PixelCount"/> elements).
    /// </summary>
    public void CopyTo(Span<Color> destination)
    {
        if (destination.Length < PixelCount)
            throw new ArgumentException("Destination is too short.", nameof(destination));

        if (IsContiguous)
        {
            Span.CopyTo(destination);
            return;
        }

        for (var y = 0; y < Height; y++)
            GetRowSpan(y).CopyTo(destination.Slice(y * Width, Width));
    }

    /// <summary>
    /// Sets all storage (including padding) to zero.
    /// </summary>
    public void Clear() => Span.Clear();

    /// <summary>
    /// Fills logical pixels with <paramref name="color"/> (padding left zero).
    /// </summary>
    public void Fill(Color color)
    {
        if (IsContiguous)
        {
            Span.Fill(color);
            return;
        }

        for (var y = 0; y < Height; y++)
            GetRowSpan(y).Fill(color);
    }

    /// <summary>
    /// Aligns a width up to <see cref="StrideAlignment"/> pixels.
    /// </summary>
    public static int AlignStride(int width)
    {
        if (width < 1)
            width = 1;
        return (width + StrideAlignment - 1) & ~(StrideAlignment - 1);
    }

    private void CopyFrom(ReadOnlySpan<Color> source)
    {
        if (source.IsEmpty)
            return;

        if (source.Length == Length)
        {
            source.CopyTo(Span);
            return;
        }

        // Tight packed source (or shorter): copy row-by-row into active width.
        var rows = Math.Min(Height, source.Length / Width);
        for (var y = 0; y < rows; y++)
        {
            var available = Math.Min(Width, source.Length - y * Width);
            if (available <= 0)
                break;
            source.Slice(y * Width, available).CopyTo(GetRowSpan(y));
        }

        // Remainder when source.Length is not a multiple of Width (partial last row).
        var consumed = rows * Width;
        if (consumed < source.Length && rows < Height)
        {
            var available = Math.Min(Width, source.Length - consumed);
            source.Slice(consumed, available).CopyTo(GetRowSpan(rows)[..available]);
        }
    }
}
