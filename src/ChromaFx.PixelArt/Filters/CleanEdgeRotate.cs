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
 * Rotation sampling adapted from torcado's cleanEdge shader (Copyright 2022 torcado),
 * MIT license — sample-through Evaluate after inverse-rotating destination UVs.
 */

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// cleanEdge rotation (torcado): sample-through edge reconstruction after inverse-rotating
/// each destination pixel into source space. Output size is the 1× axis-aligned bounds of
/// the rotated sprite (same canvas sizing as <see cref="RotateSprite"/>). Cardinal angles
/// (90 / 180 / 270) use exact pixel remaps. Uncovered samples use <see cref="EmptyColor"/>.
/// Positive <see cref="Degrees"/> is counterclockwise. Cleanup is off (rotation mode).
/// </summary>
/// <seealso cref="IFilter"/>
public class CleanEdgeRotate : IFilter
{
    private static readonly Color OpaqueBlack = new(0, 0, 0, 255);

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanEdgeRotate"/> class.
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees (counterclockwise).</param>
    /// <param name="emptyColor">
    /// Fill for samples outside the sprite. Default resolves to opaque black.
    /// </param>
    /// <param name="lineWidth">Edge spread (shader default 1). Clamped internally.</param>
    /// <param name="similarThreshold">RGBA distance for “similar” (default 0 = exact).</param>
    /// <param name="highestColor">Priority color for overlaps (default white).</param>
    public CleanEdgeRotate(
        float degrees,
        Color emptyColor = default,
        float lineWidth = 1f,
        float similarThreshold = 0f,
        Color highestColor = default
    )
    {
        Degrees = degrees;
        EmptyColor = emptyColor.IntData == 0 ? OpaqueBlack : emptyColor;
        LineWidth = lineWidth;
        SimilarThreshold = similarThreshold;
        HighestColor =
            highestColor.IntData == 0 && highestColor.Alpha == 0
                ? Color.White
                : highestColor;
    }

    /// <summary>Rotation angle in degrees (counterclockwise).</summary>
    public float Degrees { get; }

    /// <summary>Fill for out-of-bounds samples (opaque black when ctor default).</summary>
    public Color EmptyColor { get; }

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

        var angle = PixelArtRotation.NormalizeDegrees(Degrees);
        if (angle is 0f)
            return image;

        var srcW = image.Width;
        var srcH = image.Height;
        var source = PixelArtSampler.CopyTight(image);

        if (PixelArtRotation.IsCardinal(angle))
        {
            var (cardW, cardH, cardinal) = PixelArtRotation.ApplyCardinal(
                source,
                srcW,
                srcH,
                angle
            );
            image.ReCreate(cardW, cardH, cardinal);
            return image;
        }

        var radians = angle * (MathF.PI / 180f);
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);
        var (minX, minY, outW, outH) = PixelArtRotation.ComputeRotatedBounds(
            srcW,
            srcH,
            sin,
            cos
        );

        PixelArtRotation.InverseCosSin(radians, out var invCos, out var invSin);
        var srcCx = srcW / 2.0;
        var srcCy = srcH / 2.0;
        var highest = new Vector3(
            HighestColor.Red / 255f,
            HighestColor.Green / 255f,
            HighestColor.Blue / 255f
        );
        var empty = EmptyColor;
        var lineWidth = LineWidth;
        var similar = SimilarThreshold;
        var output = new Color[outW * outH];

        Parallel.For(
            0,
            outH,
            y =>
            {
                var row = y * outW;
                for (var x = 0; x < outW; ++x)
                {
                    PixelArtRotation.MapToSourceCentered(
                        x,
                        y,
                        minX,
                        minY,
                        srcCx,
                        srcCy,
                        invCos,
                        invSin,
                        out var sx,
                        out var sy
                    );

                    if (sx < 0.0 || sy < 0.0 || sx >= srcW || sy >= srcH)
                    {
                        output[row + x] = empty;
                        continue;
                    }

                    output[row + x] = CleanEdgeCore.SampleAt(
                        source,
                        srcW,
                        srcH,
                        (float)sx,
                        (float)sy,
                        highest,
                        similar,
                        lineWidth,
                        enableCleanup: false
                    );
                }
            }
        );

        image.ReCreate(outW, outH, output);
        return image;
    }
}
