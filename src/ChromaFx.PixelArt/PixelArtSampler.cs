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

using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>Edge-clamped neighborhood reads for pixel-art upscalers.</summary>
internal static class PixelArtSampler
{
    public static Color At(Image image, int x, int y)
    {
        if (x < 0)
            x = 0;
        else if (x >= image.Width)
            x = image.Width - 1;
        if (y < 0)
            y = 0;
        else if (y >= image.Height)
            y = image.Height - 1;
        return image.GetRowSpan(y)[x];
    }
}
