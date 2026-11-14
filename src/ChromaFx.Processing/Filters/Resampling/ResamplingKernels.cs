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

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Factory for resampling kernels. Always returns a fresh instance
/// (kernels mutate during <see cref="IResamplingFilter.Precompute"/>).
/// </summary>
public static class ResamplingKernels
{
    /// <summary>
    /// Creates a new kernel for <paramref name="filter"/>.
    /// </summary>
    public static IResamplingFilter Create(ResamplingFilter filter) =>
        filter switch
        {
            ResamplingFilter.Bell => new BellFilter(),
            ResamplingFilter.CatmullRom => new CatmullRomFilter(),
            ResamplingFilter.Cosine => new CosineFilter(),
            ResamplingFilter.CubicBSpline => new CubicBSplineFilter(),
            ResamplingFilter.CubicConvolution => new CubicConvolutionFilter(),
            ResamplingFilter.Hermite => new HermiteFilter(),
            ResamplingFilter.Lanczos3 => new Lanczos3Filter(),
            ResamplingFilter.Lanczos8 => new Lanczos8Filter(),
            ResamplingFilter.Mitchell => new MitchellFilter(),
            ResamplingFilter.Quadratic => new QuadraticFilter(),
            ResamplingFilter.QuadraticBSpline => new QuadraticBSplineFilter(),
            ResamplingFilter.Triangle => new TriangleFilter(),
            ResamplingFilter.Box => new BoxFilter(),
            ResamplingFilter.Bilinear => new BilinearFilter(),
            ResamplingFilter.NearestNeighbor => new NearestNeighborFilter(),
            ResamplingFilter.Robidoux => new RobidouxFilter(),
            ResamplingFilter.RobidouxSharp => new RobidouxSharpFilter(),
            ResamplingFilter.RobidouxSoft => new RobidouxSoftFilter(),
            ResamplingFilter.Bicubic => new BicubicFilter(),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null),
        };
}
