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
/// Fluent pixel-art upscale and RotSprite operations for <see cref="IImageProcessingContext"/>.
/// </summary>
public static class ImageProcessingContextExtensions
{
    /// <summary>
    /// Applies classic Scale2x (EPX), doubling width and height.
    /// </summary>
    public static IImageProcessingContext Scale2x(this IImageProcessingContext context) =>
        context.Apply(new Scale2x());

    /// <summary>
    /// Applies classic Scale3x (EPX), tripling width and height.
    /// </summary>
    public static IImageProcessingContext Scale3x(this IImageProcessingContext context) =>
        context.Apply(new Scale3x());

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

    /// <summary>
    /// Applies HQ2x upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext Hq2x(this IImageProcessingContext context) =>
        context.Apply(new Hq2x());

    /// <summary>
    /// Applies HQ3x upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext Hq3x(this IImageProcessingContext context) =>
        context.Apply(new Hq3x());

    /// <summary>
    /// Applies HQ4x upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext Hq4x(this IImageProcessingContext context) =>
        context.Apply(new Hq4x());

    /// <summary>
    /// Applies classic Eagle 2× upscaling.
    /// </summary>
    public static IImageProcessingContext Eagle(this IImageProcessingContext context) =>
        context.Apply(new Eagle());

    /// <summary>
    /// Applies SuperEagle 2× upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext SuperEagle(this IImageProcessingContext context) =>
        context.Apply(new SuperEagle());

    /// <summary>
    /// Applies classic 2×SaI upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext Scale2xSaI(this IImageProcessingContext context) =>
        context.Apply(new Scale2xSaI());

    /// <summary>
    /// Applies Super2xSaI upscaling (may blend edge colors).
    /// </summary>
    public static IImageProcessingContext Super2xSaI(this IImageProcessingContext context) =>
        context.Apply(new Super2xSaI());

    /// <summary>
    /// Applies RotSprite pixel-art rotation (counterclockwise degrees; AABB canvas).
    /// </summary>
    public static IImageProcessingContext RotateSprite(
        this IImageProcessingContext context,
        float degrees
    ) => context.Apply(new RotateSprite(degrees));

    /// <summary>
    /// Applies cleanEdge upscaling (default 4×; may prioritize edges toward white).
    /// </summary>
    public static IImageProcessingContext CleanEdge(
        this IImageProcessingContext context,
        int scale = 4
    ) => context.Apply(new CleanEdge(scale));

    /// <summary>
    /// Applies cleanEdge sample-through rotation (counterclockwise degrees; 1× AABB canvas).
    /// </summary>
    public static IImageProcessingContext CleanEdgeRotate(
        this IImageProcessingContext context,
        float degrees
    ) => context.Apply(new CleanEdgeRotate(degrees));
}
