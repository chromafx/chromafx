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
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO;

public static class ImageIOExtensions
{
    static ImageIOExtensions()
    {
        Image.RegisterLoaders(LoadFromPath, LoadFromStream);
    }

    public static bool Save(this Image image, string fileName)
        => FormatManager.Instance.Encode(fileName, image);

    public static bool Save(this Image image, Stream stream, FileFormats format)
        => FormatManager.Instance.Encode(stream, image, format);

    public static string ToBase64String(this Image image, FileFormats desiredFormat)
    {
        using var stream = new MemoryStream();
        if (!image.Save(stream, desiredFormat))
            return string.Empty;
        var tempArray = stream.ToArray();
        return Convert.ToBase64String(tempArray, 0, tempArray.Length);
    }

    private static Image LoadFromPath(string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return LoadFromStream(stream);
    }

    private static Image LoadFromStream(Stream stream)
    {
        var image = FormatManager.Instance.Decode(stream);
        return image ?? throw new ImageException("Unable to decode image from stream.");
    }
}
