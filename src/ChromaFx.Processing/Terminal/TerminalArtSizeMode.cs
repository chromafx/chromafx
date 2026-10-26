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
/// How terminal art output dimensions are derived from
/// <c>MaxWidth</c> / <c>MaxHeight</c>.
/// </summary>
public enum TerminalArtSizeMode
{
    /// <summary>
    /// Fit inside the max box while preserving aspect ratio. Output may be
    /// smaller than <c>MaxWidth</c>×<c>MaxHeight</c>.
    /// </summary>
    Fit = 0,

    /// <summary>
    /// Emit exactly <c>MaxWidth</c>×<c>MaxHeight</c> characters, letterboxing
    /// with the background when the image is smaller than the box. Both maxima
    /// must be greater than zero.
    /// </summary>
    Exact = 1,
}
