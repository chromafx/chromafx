/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

using ChromaFx.IO.Formats.BaseClasses;
using File = ChromaFx.IO.Formats.Bmp.Format.File;

namespace ChromaFx.IO.Formats.Bmp;

/// <summary>
/// BMP encoder
/// </summary>
/// <seealso cref="EncoderBase{File}" />
public class Encoder : EncoderBase<File>
{
    /// <summary>
    /// Gets the file extensions.
    /// </summary>
    /// <value>
    /// The file extensions.
    /// </value>
    protected override string[] FileExtensions => [".BMP", ".DIB"];
}