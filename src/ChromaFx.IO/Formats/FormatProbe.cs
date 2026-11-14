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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Shared sniff helpers for managed codec formats (TGA / ICO / TIFF).
/// </summary>
internal static class FormatProbe
{
    /// <summary>
    /// Reads <paramref name="headerBytes"/> from the start of <paramref name="stream"/> and
    /// tests <paramref name="looksLike"/>, restoring position to 0 afterward.
    /// </summary>
    internal static bool LooksLikeAtStart(
        Stream stream,
        int headerBytes,
        Func<ReadOnlySpan<byte>, bool> looksLike
    )
    {
        if (stream == null || !stream.CanSeek || stream.Length < headerBytes)
            return false;
        stream.Seek(0, SeekOrigin.Begin);
        var header = new byte[headerBytes];
        try
        {
            stream.ReadExactly(header);
        }
        catch (EndOfStreamException)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return false;
        }

        var ok = looksLike(header);
        stream.Seek(0, SeekOrigin.Begin);
        return ok;
    }

    /// <summary>
    /// True when <paramref name="fileName"/> ends with any of <paramref name="extensions"/> (ordinal ignore-case).
    /// </summary>
    internal static bool HasExtension(string fileName, params string[] extensions)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        foreach (var ext in extensions)
        {
            if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
