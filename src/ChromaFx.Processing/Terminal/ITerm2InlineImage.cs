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

using System.Text;

namespace ChromaFx.Processing.Terminal;

/// <summary>
/// iTerm2 inline image protocol (OSC 1337). Also used by WezTerm.
/// Pass already-encoded PNG bytes; use the ChromaFx meta-package helpers to
/// encode an <c>Image</c>.
/// </summary>
public static class ITerm2InlineImage
{
    /// <summary>
    /// Builds an iTerm2 inline-image OSC sequence for a PNG payload.
    /// </summary>
    /// <param name="pngBytes">Encoded PNG bytes.</param>
    /// <param name="columns">Optional width in cells; 0 omits the argument.</param>
    /// <param name="rows">Optional height in cells; 0 omits the argument.</param>
    public static string FromPng(
        ReadOnlySpan<byte> pngBytes,
        int columns = 0,
        int rows = 0
    )
    {
        if (pngBytes.IsEmpty)
            return string.Empty;

        var base64 = Convert.ToBase64String(pngBytes);
        var builder = new StringBuilder(base64.Length + 64);
        builder.Append("\u001b]1337;File=inline=1");
        builder.Append(";size=");
        builder.Append(pngBytes.Length);
        if (columns > 0)
        {
            builder.Append(";width=");
            builder.Append(columns);
        }

        if (rows > 0)
        {
            builder.Append(";height=");
            builder.Append(rows);
        }

        builder.Append(':');
        builder.Append(base64);
        builder.Append('\u0007');
        return builder.ToString();
    }
}
