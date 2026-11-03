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

using ChromaFx.PixelArt.Filters;
using ChromaFx.Processing;

namespace ChromaFx.PixelArt;

/// <summary>
/// Fluent Scale2x / Scale4x / Scale8x operations for <see cref="IImageProcessingContext"/>.
/// </summary>
public static class ImageProcessingContextExtensions
{
    /// <summary>
    /// Applies classic Scale2x (EPX), doubling width and height.
    /// </summary>
    public static IImageProcessingContext Scale2x(this IImageProcessingContext context) =>
        context.Apply(new Scale2x());

    /// <summary>
    /// Applies Scale2x twice (4× width and height).
    /// </summary>
    public static IImageProcessingContext Scale4x(this IImageProcessingContext context) =>
        context.Apply(new Scale4x());

    /// <summary>
    /// Applies Scale2x three times (8× width and height).
    /// </summary>
    public static IImageProcessingContext Scale8x(this IImageProcessingContext context) =>
        context.Apply(new Scale8x());
}
