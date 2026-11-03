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

using System.Collections;

namespace ChromaFx.Processing.Terminal;

/// <summary>
/// Heuristic terminal image capabilities from environment variables.
/// </summary>
public sealed class TerminalImageCapabilities(
    bool supportsKitty,
    bool supportsITerm2,
    bool supportsTrueColor
    )
{

    /// <summary>Kitty graphics protocol appears available.</summary>
    public bool SupportsKitty { get; } = supportsKitty;

    /// <summary>iTerm2 (or WezTerm) inline images appear available.</summary>
    public bool SupportsITerm2 { get; } = supportsITerm2;

    /// <summary>24-bit color SGR appears available.</summary>
    public bool SupportsTrueColor { get; } = supportsTrueColor;

    /// <summary>
    /// Suggested protocol: Kitty → iTerm2 → ANSI art → ASCII art.
    /// </summary>
    public TerminalImageProtocol Preferred { get; } = supportsKitty
            ? TerminalImageProtocol.Kitty
            : supportsITerm2
                ? TerminalImageProtocol.ITerm2
                : supportsTrueColor
                    ? TerminalImageProtocol.AnsiArt
                    : TerminalImageProtocol.AsciiArt;

    /// <summary>Detect from the current process environment.</summary>
    public static TerminalImageCapabilities Detect() =>
        Detect(Environment.GetEnvironmentVariables());

    /// <summary>Detect from an environment variable map (for tests).</summary>
    public static TerminalImageCapabilities Detect(IDictionary variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var term = Get(variables, "TERM");
        var termProgram = Get(variables, "TERM_PROGRAM");
        var colorTerm = Get(variables, "COLORTERM");
        var kittyWindow = Get(variables, "KITTY_WINDOW_ID");
        var termFeatures = Get(variables, "TERM_FEATURES");

        var isWezTerm =
            string.Equals(termProgram, "WezTerm", StringComparison.OrdinalIgnoreCase)
            || ContainsIgnoreCase(term, "wezterm");
        var isGhostty = string.Equals(termProgram, "ghostty", StringComparison.OrdinalIgnoreCase);

        // WezTerm and Ghostty speak Kitty's graphics protocol; Prefer ranks Kitty first.
        var supportsKitty =
            !string.IsNullOrEmpty(kittyWindow)
            || ContainsIgnoreCase(term, "kitty")
            || ContainsIgnoreCase(termFeatures, "kitty")
            || isWezTerm
            || isGhostty;

        // WezTerm also implements the iTerm2 inline image protocol.
        var supportsITerm2 =
            string.Equals(termProgram, "iTerm.app", StringComparison.OrdinalIgnoreCase)
            || isWezTerm;

        var supportsTrueColor =
            string.Equals(colorTerm, "truecolor", StringComparison.OrdinalIgnoreCase)
            || string.Equals(colorTerm, "24bit", StringComparison.OrdinalIgnoreCase)
            || supportsKitty
            || supportsITerm2
            || isGhostty
            || !string.IsNullOrEmpty(Get(variables, "WT_SESSION"));

        return new TerminalImageCapabilities(supportsKitty, supportsITerm2, supportsTrueColor);
    }

    private static string Get(IDictionary variables, string key)
    {
        if (!variables.Contains(key))
            return null;
        return variables[key]?.ToString();
    }

    private static bool ContainsIgnoreCase(string value, string fragment) =>
        !string.IsNullOrEmpty(value)
        && value.Contains(fragment, StringComparison.OrdinalIgnoreCase);
}
