/*
 * Copyright 2025–2026 Ho Tzin Mein
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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

/// <summary>
/// Extension methods for mutating and cloning images.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Mutates the image in place using the processing context.
    /// </summary>
    public static Image Mutate(
        this Image image,
        Action<IImageProcessingContext> operation,
        Rectangle targetLocation = default
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(operation);
        var context = new ImageProcessingContext(image, targetLocation);
        operation(context);
        return image;
    }

    /// <summary>
    /// Creates a deep copy of the image and optionally mutates the copy.
    /// </summary>
    public static Image Clone(this Image image, Action<IImageProcessingContext> operation)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(operation);
        var copy = image.Clone();
        return copy.Mutate(operation);
    }
}
