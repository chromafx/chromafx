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

using System.Text;
using ChromaFx.Core;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// Shared stream → <see cref="BinaryWriter"/> boilerplate for typed encoders.
/// </summary>
internal static class EncoderIO
{
    internal static void Write(Stream stream, Image image, Action<BinaryWriter, Image> encode)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        encode(writer, image);
    }

    internal static void Write(Stream stream, Animation animation, Action<BinaryWriter, Animation> encode)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(animation);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        encode(writer, animation);
    }

    internal static void Write(
        Stream stream,
        IReadOnlyList<Image> images,
        Action<BinaryWriter, IReadOnlyList<Image>> encode
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(images);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        encode(writer, images);
    }
}
