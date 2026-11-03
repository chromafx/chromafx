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
using ChromaFx.Core.Colors;

namespace ChromaFx.Processing.Terminal;

/// <summary>ANSI SGR truecolor / 256-color helpers.</summary>
internal static class AnsiSgr
{
    public const string Reset = "\u001b[0m";

    public static void AppendColor(
        StringBuilder builder,
        ref int? lastKey,
        bool isForeground,
        Color color,
        AnsiColorMode mode
    )
    {
        int key;
        if (mode == AnsiColorMode.Ansi256)
        {
            var index = ToAnsi256Index(color);
            key = unchecked(0x1000000) | index;
            if (lastKey == key)
                return;
            lastKey = key;
            builder.Append(isForeground ? "\u001b[38;5;" : "\u001b[48;5;");
            builder.Append(index);
            builder.Append('m');
            return;
        }

        key = color.Red << 16 | color.Green << 8 | color.Blue;
        if (lastKey == key)
            return;
        lastKey = key;
        builder.Append(isForeground ? "\u001b[38;2;" : "\u001b[48;2;");
        builder.Append(color.Red);
        builder.Append(';');
        builder.Append(color.Green);
        builder.Append(';');
        builder.Append(color.Blue);
        builder.Append('m');
    }

    public static int ToAnsi256Index(Color color)
    {
        var r = color.Red;
        var g = color.Green;
        var b = color.Blue;

        if (r == g && g == b)
        {
            if (r < 8)
                return 16;
            if (r > 248)
                return 231;
            return 232 + (r - 8) * 24 / 247;
        }

        return 16 + 36 * ScaleToCube(r) + 6 * ScaleToCube(g) + ScaleToCube(b);
    }

    private static int ScaleToCube(byte value) => (value * 5 + 127) / 255;
}
