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
/// Kitty graphics protocol (PNG over APC). Pure managed; no shared memory.
/// Pass already-encoded PNG bytes; use the ChromaFx meta-package helpers to
/// encode an <c>Image</c>.
/// </summary>
public static class KittyGraphics
{
    /// <summary>Max base64 payload bytes per APC chunk (protocol limit).</summary>
    public const int MaxChunkLength = 4096;

    /// <summary>
    /// Builds a Kitty transmit-and-display sequence for a PNG payload.
    /// </summary>
    /// <param name="pngBytes">Encoded PNG bytes.</param>
    /// <param name="columns">Optional display width in cells (<c>c</c>).</param>
    /// <param name="rows">Optional display height in cells (<c>r</c>).</param>
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
        AppendChunks(builder, base64, columns, rows);
        return builder.ToString();
    }

    internal static void AppendChunks(
        StringBuilder builder,
        string base64,
        int columns,
        int rows
    )
    {
        var offset = 0;
        var first = true;
        while (offset < base64.Length)
        {
            var remaining = base64.Length - offset;
            var take = Math.Min(MaxChunkLength, remaining);
            // Non-final chunks must be a multiple of 4 (protocol).
            if (take < remaining)
                take -= take % 4;
            if (take == 0)
                take = Math.Min(4, remaining);

            var isLast = offset + take >= base64.Length;
            builder.Append("\u001b_G");
            if (first)
            {
                builder.Append("a=T,f=100");
                if (columns > 0)
                {
                    builder.Append(",c=");
                    builder.Append(columns);
                }

                if (rows > 0)
                {
                    builder.Append(",r=");
                    builder.Append(rows);
                }

                builder.Append(',');
                first = false;
            }

            builder.Append(isLast ? "m=0;" : "m=1;");
            builder.Append(base64, offset, take);
            builder.Append("\u001b\\");
            offset += take;
        }
    }
}
