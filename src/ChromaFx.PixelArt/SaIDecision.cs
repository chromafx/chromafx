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

namespace ChromaFx.PixelArt;

/// <summary>
/// Diagonal preference scoring used by SuperEagle / Super2xSaI / 2xSaI
/// (clean-room from published SaI-family decision rules).
/// </summary>
internal static class SaIDecision
{
    /// <summary>
    /// Scores whether context colors prefer <paramref name="a"/> (positive) or <paramref name="b"/> (negative).
    /// </summary>
    public static int Score(Color a, Color b, Color c, Color d)
    {
        var favorA = 0;
        var favorB = 0;
        if (a == c)
            favorA++;
        else if (b == c)
            favorB++;
        if (a == d)
            favorA++;
        else if (b == d)
            favorB++;

        var r = 0;
        if (favorA <= 1)
            r++;
        if (favorB <= 1)
            r--;
        return r;
    }

    /// <summary>Opposite polarity of <see cref="Score"/> (classic 2xSaI GetResult2).</summary>
    public static int ScoreOpposite(Color a, Color b, Color c, Color d) => -Score(a, b, c, d);
}
