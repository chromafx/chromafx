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

using ChromaFx.Core.Colors;
using ChromaFx.Processing.Terminal;

namespace ChromaFx.Processing.HtmlArt;

/// <summary>
/// Options for rendering an image as fixed-width colored HTML art.
/// </summary>
public sealed class HtmlArtOptions : TerminalArtOptions
{
    /// <summary>
    /// When true (default), each character is an upper half block with
    /// CSS <c>color</c> (top) and <c>background-color</c> (bottom), doubling
    /// vertical resolution. When false, each character is a full block
    /// (<c>█</c>) with foreground color only.
    /// </summary>
    public bool UseHalfBlocks { get; set; } = true;

    /// <summary>
    /// Color under transparent pixels and letterbox padding. Defaults to black.
    /// </summary>
    public Color BackgroundColor { get; set; } = new(0, 0, 0, 255);

    /// <summary>
    /// When true (default), wrap the grid in a monospace <c>&lt;pre&gt;</c>.
    /// </summary>
    public bool WrapInPre { get; set; } = true;
}
