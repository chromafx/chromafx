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
/// Terminal image output style suggested by <see cref="TerminalImageCapabilities"/>.
/// </summary>
public enum TerminalImageProtocol
{
    /// <summary>Kitty graphics protocol (PNG over APC).</summary>
    Kitty = 0,

    /// <summary>iTerm2 inline image protocol (OSC 1337).</summary>
    ITerm2 = 1,

    /// <summary>ANSI truecolor / 256-color half-block art.</summary>
    AnsiArt = 2,

    /// <summary>Grayscale character-ramp art.</summary>
    AsciiArt = 3,
}
