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
 *
 * Adapted from torcado's cleanEdge (Copyright 2022 torcado), MIT license.
 */

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// cleanEdge (torcado) pixel-art upscaler — edge/line reconstruction for clean
/// rotations and scaling. May prioritize colors toward <see cref="HighestColor"/>.
/// </summary>
/// <seealso cref="IFilter"/>
public class CleanEdge : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CleanEdge"/> class.
    /// </summary>
    /// <param name="scale">Integer upscale factor (≥ 2). Default 4.</param>
    /// <param name="lineWidth">Edge spread (shader default 1). Clamped internally.</param>
    /// <param name="similarThreshold">RGBA distance for “similar” (default 0 = exact).</param>
    /// <param name="highestColor">Priority color for overlaps (default white).</param>
    public CleanEdge(
        int scale = 4,
        float lineWidth = 1f,
        float similarThreshold = 0f,
        Color highestColor = default
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
        Scale = scale;
        LineWidth = lineWidth;
        SimilarThreshold = similarThreshold;
        HighestColor =
            highestColor.IntData == 0 && highestColor.Alpha == 0
                ? Color.White
                : highestColor;
    }

    /// <summary>Integer upscale factor.</summary>
    public int Scale { get; }

    /// <summary>How far edges spread into neighbors.</summary>
    public float LineWidth { get; }

    /// <summary>Distance threshold for treating colors as similar.</summary>
    public float SimilarThreshold { get; }

    /// <summary>Color preferred in overlap priority (closer colors win).</summary>
    public Color HighestColor { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var srcW = image.Width;
        var srcH = image.Height;
        var src = new Color[srcW * srcH];
        for (var y = 0; y < srcH; ++y)
            image.GetRowSpan(y).CopyTo(src.AsSpan(y * srcW, srcW));

        var dstW = srcW * Scale;
        var dstH = srcH * Scale;
        var output = new Color[dstW * dstH];
        var highest = new Vector3(
            HighestColor.Red / 255f,
            HighestColor.Green / 255f,
            HighestColor.Blue / 255f
        );

        CleanEdgeCore.Upscale(
            src,
            srcW,
            srcH,
            output,
            Scale,
            highest,
            SimilarThreshold,
            LineWidth
        );

        image.ReCreate(dstW, dstH, output);
        return image;
    }
}
