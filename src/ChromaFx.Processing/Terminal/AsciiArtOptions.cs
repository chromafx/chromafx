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

namespace ChromaFx.Processing.Terminal;

/// <summary>
/// Options for rendering an image as ASCII art.
/// </summary>
public sealed class AsciiArtOptions : TerminalArtOptions
{
    /// <summary>
    /// Character ramp from dark to light. Null or empty uses the default ramp.
    /// </summary>
    public string Characters { get; set; }

    /// <summary>
    /// When true, the character ramp is reversed (suited to dark terminals).
    /// When <see cref="BackgroundColor"/> is left at default (transparent),
    /// Invert also selects a black composite background; otherwise white.
    /// </summary>
    public bool Invert { get; set; }

    /// <summary>
    /// Color under transparent pixels and letterbox padding. Default (all zeros)
    /// means automatic: white when <see cref="Invert"/> is false, black when true.
    /// Set an opaque color to match TUI chrome.
    /// </summary>
    public Color BackgroundColor { get; set; }
}
