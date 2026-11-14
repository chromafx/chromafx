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
/// Encode with <c>image.ToByteArray(ManagedFormats.Png)</c> (ChromaFx.IO), then call
/// <see cref="FromPng"/>.
/// </summary>
public static class ITerm2InlineImage
{
    /// <summary>
    /// Builds an iTerm2 inline-image OSC sequence for a PNG payload.
    /// </summary>
    /// <param name="pngBytes">Encoded PNG bytes.</param>
    /// <param name="columns">Optional width in cells; 0 omits the argument.</param>
    /// <param name="rows">Optional height in cells; 0 omits the argument.</param>
    public static string FromPng(ReadOnlySpan<byte> pngBytes, int columns = 0, int rows = 0)
    {
        var builder = new StringBuilder();
        AppendFromPng(builder, pngBytes, columns, rows);
        return builder.ToString();
    }

    /// <summary>
    /// Appends an iTerm2 inline-image sequence to <paramref name="destination"/>.
    /// </summary>
    public static void AppendFromPng(
        StringBuilder destination,
        ReadOnlySpan<byte> pngBytes,
        int columns = 0,
        int rows = 0
    )
    {
        ArgumentNullException.ThrowIfNull(destination);
        ValidateCellSize(columns, rows);
        if (pngBytes.IsEmpty)
            return;

        var base64 = Convert.ToBase64String(pngBytes);
        destination.EnsureCapacity(destination.Length + base64.Length + 64);
        destination.Append("\u001b]1337;File=inline=1");
        destination.Append(";size=");
        destination.Append(pngBytes.Length);
        if (columns > 0)
        {
            destination.Append(";width=");
            destination.Append(columns);
        }

        if (rows > 0)
        {
            destination.Append(";height=");
            destination.Append(rows);
        }

        destination.Append(':');
        destination.Append(base64);
        destination.Append('\u0007');
    }

    private static void ValidateCellSize(int columns, int rows)
    {
        if (columns < 0)
            throw new ArgumentOutOfRangeException(nameof(columns), columns, "Must be >= 0.");
        if (rows < 0)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "Must be >= 0.");
    }
}
