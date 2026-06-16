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

using System.Buffers;
using ChromaFx.Core.Colors;

namespace ChromaFx.Processing;

internal static class FilterBufferPool
{
    internal static Color[] RentCopy(Color[] source)
    {
        var buffer = ArrayPool<Color>.Shared.Rent(source.Length);
        Array.Copy(source, buffer, source.Length);
        return buffer;
    }

    internal static void Return(Color[] buffer) => ArrayPool<Color>.Shared.Return(buffer);
}
