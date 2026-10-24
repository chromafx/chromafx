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
using ChromaFx.IO.Encoders;

namespace ChromaFx.IO;

/// <summary>
/// Animation save extensions. Prefer <see cref="Animation.Load"/>.
/// </summary>
public static class AnimationIOExtensions
{
    /// <summary>
    /// Saves the animation to a file path (format chosen by extension).
    /// </summary>
    public static void Save(this Animation animation, string fileName)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ImageFormats.Encode(fileName, animation);
    }

    /// <summary>
    /// Saves the animation to a file path using the given encoder.
    /// </summary>
    public static void Save(this Animation animation, string fileName, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(encoder);
        new FileInfo(fileName).Directory?.Create();
        using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        ImageFormats.Encode(stream, animation, encoder);
    }

    /// <summary>
    /// Saves the animation to a stream using the given encoder.
    /// </summary>
    public static void Save(this Animation animation, Stream stream, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoder);
        ImageFormats.Encode(stream, animation, encoder);
    }
}
