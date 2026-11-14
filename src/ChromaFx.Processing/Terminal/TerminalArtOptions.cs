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

namespace ChromaFx.Processing.Terminal;

/// <summary>
/// Shared sizing options for fixed-width character art (terminal ASCII/ANSI and HTML).
/// </summary>
public abstract class TerminalArtOptions
{
    /// <summary>
    /// Maximum output width in characters. When zero (and
    /// <see cref="SizeMode"/> is <see cref="TerminalArtSizeMode.Fit"/>), the
    /// image width is used before height/aspect fitting.
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// Maximum output height in characters. When zero under Fit mode, height is
    /// unconstrained aside from <see cref="CharacterAspect"/>. Under Exact mode
    /// both maxima must be set.
    /// </summary>
    public int MaxHeight { get; set; }

    /// <summary>
    /// Fit inside the max box, or emit exactly <see cref="MaxWidth"/>×
    /// <see cref="MaxHeight"/> with letterboxing.
    /// </summary>
    public TerminalArtSizeMode SizeMode { get; set; } = TerminalArtSizeMode.Fit;

    /// <summary>
    /// Approximate height-to-width ratio of a terminal glyph. When zero (default),
    /// uses 2 (typical monospace cells are ~twice as tall as wide). Half-block
    /// ANSI/HTML still use 2 — two vertical samples share one tall cell. Set
    /// explicitly to override.
    /// </summary>
    public float CharacterAspect { get; set; }
}
