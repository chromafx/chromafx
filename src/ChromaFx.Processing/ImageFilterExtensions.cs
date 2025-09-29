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


using ChromaFx.Core;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

public static class ImageFilterExtensions
{
    public static Image ApplyFilter(this Image image, IFilter filter, Rectangle targetLocation = default)
    {
        return filter.Apply(image, targetLocation);
    }

    public static Image ApplyFilter<TFilter>(this Image image, Rectangle targetLocation = default)
        where TFilter : IFilter, new()
    {
        return image.ApplyFilter(new TFilter(), targetLocation);
    }
}
