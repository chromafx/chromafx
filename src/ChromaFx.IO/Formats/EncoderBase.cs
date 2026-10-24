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
/// Encoder base class
/// </summary>
/// <typeparam name="TFile">The type of the file.</typeparam>
/// <seealso cref="IEncoder" />
internal abstract class EncoderBase<TFile> : IEncoder
    where TFile : FileBase, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EncoderBase{TFile}"/> class.
    /// </summary>
    protected EncoderBase() { }

    /// <summary>
    /// Gets the file extensions.
    /// </summary>
    protected abstract string[] FileExtensions { get; }

    /// <inheritdoc />
    public bool CanEncode(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        return FileExtensions.Any(x => fileName.EndsWith(x, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Animation animation)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(animation);
        new TFile().Write(writer, animation);
    }

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Image image)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(image);
        new TFile().Write(writer, image);
    }
}
