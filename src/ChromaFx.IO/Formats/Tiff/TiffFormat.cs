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

namespace ChromaFx.IO.Formats.Tiff;

/// <summary>
/// Baseline TIFF still image. The first IFD is decoded; later pages are ignored.
/// </summary>
/// <seealso cref="FormatBase"/>
public sealed class TiffFormat : FormatBase
{
    /// <inheritdoc />
    public override FileFormats Format => FileFormats.Tiff;

    /// <inheritdoc />
    private protected override IDecoder Decoder => new Decoder();

    /// <inheritdoc />
    private protected override IEncoder Encoder => new Encoder();
}
