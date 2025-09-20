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
using ChromaFx.Filters.Resampling.Enums;
using ChromaFx.Filters.Resampling.ResamplingFilters;
using ChromaFx.Filters.Resampling.ResamplingFilters.Interfaces;

namespace ChromaFx.Filters.Resampling;

/// <summary>
/// List of filters
/// </summary>
public class FilterList
{
    /// <summary>
    /// Dictionary of available resampling filters
    /// </summary>
    public static Dictionary<ResamplingFiltersAvailable, IResamplingFilter> Filters => new()
    {
        { ResamplingFiltersAvailable.Bell, new BellFilter() },
        { ResamplingFiltersAvailable.CatmullRom, new CatmullRomFilter() },
        { ResamplingFiltersAvailable.Cosine, new CosineFilter() },
        { ResamplingFiltersAvailable.CubicBSpline, new CubicBSplineFilter() },
        { ResamplingFiltersAvailable.CubicConvolution, new CubicConvolutionFilter() },
        { ResamplingFiltersAvailable.Hermite, new HermiteFilter() },
        { ResamplingFiltersAvailable.Lanczos3, new Lanczos3Filter() },
        { ResamplingFiltersAvailable.Lanczos8, new Lanczos8Filter() },
        { ResamplingFiltersAvailable.Mitchell, new MitchellFilter() },
        { ResamplingFiltersAvailable.Quadratic, new QuadraticFilter() },
        { ResamplingFiltersAvailable.QuadraticBSpline, new QuadraticBSplineFilter() },
        { ResamplingFiltersAvailable.Triangle, new TriangleFilter() },
        { ResamplingFiltersAvailable.Box, new BoxFilter() },
        { ResamplingFiltersAvailable.Bilinear, new BilinearFilter() },
        { ResamplingFiltersAvailable.NearestNeighbor, new NearestNeighborFilter() },
        { ResamplingFiltersAvailable.Robidoux, new RobidouxFilter() },
        { ResamplingFiltersAvailable.RobidouxSharp, new RobidouxSharpFilter() },
        { ResamplingFiltersAvailable.RobidouxSoft, new RobidouxSoftFilter() },
        { ResamplingFiltersAvailable.Bicubic, new BicubicFilter() }
    };
}