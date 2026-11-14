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

using ChromaFx.Core;
using ChromaFx.Core.Quantization;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Classic "bad web" scaling: nearest-neighbor downsample, optional web-safe quantize on the
/// small image, then nearest-neighbor upsample back to the original size.
/// </summary>
/// <seealso cref="IFilter"/>
/// <param name="factor">
/// Downsample factor in (0, 1). Default 0.25 (quarter size, then blow back up). Values outside
/// that range are a no-op (optional web-safe alone is not applied).
/// </param>
/// <param name="webSafe">When true, snaps the small image with <see cref="WebSafeQuantize"/>.</param>
/// <param name="dither">Dither for the optional web-safe pass.</param>
/// <param name="strength">Dither strength for the optional web-safe pass.</param>
public sealed class BadWebScale(
    float factor = 0.25f,
    bool webSafe = false,
    OrderedDitherMethod dither = OrderedDitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    /// <summary>
    /// Gets the downsample factor.
    /// </summary>
    public float Factor { get; } = factor;

    /// <summary>
    /// Gets whether web-safe quantize runs on the small image.
    /// </summary>
    public bool WebSafe { get; } = webSafe;

    /// <summary>
    /// Gets the dither method for the optional web-safe pass.
    /// </summary>
    public OrderedDitherMethod Dither { get; } = dither;

    /// <summary>
    /// Gets the dither strength for the optional web-safe pass.
    /// </summary>
    public float Strength { get; } = strength;

    /// <summary>
    /// Applies bad-web scaling to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        if (Factor <= 0f || Factor >= 1f)
            return image;

        var origW = image.Width;
        var origH = image.Height;
        var smallW = Math.Max(1, (int)MathF.Round(origW * Factor));
        var smallH = Math.Max(1, (int)MathF.Round(origH * Factor));

        if (smallW == origW && smallH == origH)
        {
            if (WebSafe)
                new WebSafeQuantize(Dither, Strength).Apply(image);
            return image;
        }

        new Resize(smallW, smallH, ResamplingFilter.NearestNeighbor).Apply(image);
        if (WebSafe)
            new WebSafeQuantize(Dither, Strength).Apply(image);
        new Resize(origW, origH, ResamplingFilter.NearestNeighbor).Apply(image);
        return image;
    }
}
