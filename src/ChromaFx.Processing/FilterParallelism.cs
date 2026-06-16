/*
 * Copyright 2025 Ho Tzin Mein
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

using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

internal static class FilterParallelism
{
    internal const int ParallelPixelThreshold = 65536;

    internal static void ForRows(Rectangle region, Action<int> processRow)
    {
        var pixelCount = (region.Top - region.Bottom) * (region.Right - region.Left);
        if (pixelCount < ParallelPixelThreshold)
        {
            for (var y = region.Bottom; y < region.Top; ++y)
                processRow(y);
        }
        else
        {
            Parallel.For(region.Bottom, region.Top, processRow);
        }
    }
}
