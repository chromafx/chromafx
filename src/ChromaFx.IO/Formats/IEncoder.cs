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
/// Encoder interface
/// </summary>
internal interface IEncoder
{
    /// <summary>
    /// Determines whether this instance can encode the specified file name.
    /// </summary>
    bool CanEncode(string fileName);

    /// <summary>
    /// Encodes an image into the specified writer. Throws on failure.
    /// </summary>
    void Encode(BinaryWriter writer, Image image);

    /// <summary>
    /// Encodes an animation into the specified writer. Throws on failure.
    /// </summary>
    void Encode(BinaryWriter writer, Animation animation);
}
