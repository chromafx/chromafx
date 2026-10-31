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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChromaFx.Core;

namespace ChromaFx.IO;

/// <summary>
/// Module initializer that wires Load/Save into Core.
/// </summary>
internal static class ImageFormatsBootstrap
{
    [ModuleInitializer]
    [UnconditionalSuppressMessage(
        "AOT",
        "CA2255",
        Justification = "Library entry bootstrap for Image.Load/Save registration."
    )]
    internal static void Initialize()
    {
        ImageFormats.EnsureBuiltIns();
        Image.RegisterLoaders(LoadImageFromPath, LoadImageFromStream, LoadImageFromMemory);
        Animation.RegisterLoaders(
            LoadAnimationFromPath,
            LoadAnimationFromStream,
            LoadAnimationFromMemory
        );
    }

    private static Image LoadImageFromPath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return LoadImageFromStream(stream);
    }

    private static Image LoadImageFromStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return ImageFormats.Decode(stream);
    }

    private static Image LoadImageFromMemory(ReadOnlyMemory<byte> data)
    {
        if (MemoryMarshal.TryGetArray(data, out var segment) && segment.Array is not null)
        {
            using var stream = new MemoryStream(
                segment.Array,
                segment.Offset,
                segment.Count,
                writable: false
            );
            return LoadImageFromStream(stream);
        }

        using var copy = new MemoryStream(data.ToArray(), writable: false);
        return LoadImageFromStream(copy);
    }

    private static Animation LoadAnimationFromPath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return LoadAnimationFromStream(stream);
    }

    private static Animation LoadAnimationFromStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return ImageFormats.DecodeAnimation(stream);
    }

    private static Animation LoadAnimationFromMemory(ReadOnlyMemory<byte> data)
    {
        if (MemoryMarshal.TryGetArray(data, out var segment) && segment.Array is not null)
        {
            using var stream = new MemoryStream(
                segment.Array,
                segment.Offset,
                segment.Count,
                writable: false
            );
            return LoadAnimationFromStream(stream);
        }

        using var copy = new MemoryStream(data.ToArray(), writable: false);
        return LoadAnimationFromStream(copy);
    }
}
