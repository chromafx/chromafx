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

using ChromaFx.Core;

namespace ChromaFx.IO.Formats;

/// <summary>
/// Animation format base class
/// </summary>
/// <seealso cref="FormatBase" />
/// <seealso cref="IAnimationFormat" />
public abstract class AnimationFormatBase : FormatBase, IAnimationFormat
{
    /// <inheritdoc />
    public Animation DecodeAnimation(Stream stream)
    {
        return Decoder.DecodeAnimation(stream);
    }

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Animation image)
    {
        Encoder.Encode(writer, image);
    }
}
