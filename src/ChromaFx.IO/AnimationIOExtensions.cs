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
using ChromaFx.IO.Formats;

namespace ChromaFx.IO;

public static class AnimationIOExtensions
{
    public static bool Save(this Animation animation, string fileName)
        => new Manager().Encode(fileName, animation);

    public static Animation LoadAnimation(string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
        return new Manager().DecodeAnimation(stream);
    }

    public static Animation LoadAnimation(Stream stream)
        => new Manager().DecodeAnimation(stream);
}
