/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Core;
using ChromaFx.IO.Formats.Bmp;
using ChromaFx.IO.Formats.Gif;
using ChromaFx.IO.Formats.Interfaces;
using ChromaFx.IO.Formats.Jpeg;
using ChromaFx.IO.Formats.Png;

namespace ChromaFx.IO.Formats;

/// <summary>
/// Format manager
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Manager" /> class.
/// </remarks>
/// <param name="formats">The formats.</param>
/// <param name="animationFormats">The animation formats.</param>
public class Manager(IEnumerable<IFormat> formats, IEnumerable<IAnimationFormat> animationFormats)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manager"/> class.
    /// </summary>
    public Manager()
        : this(
            [new BmpFormat(), new PngFormat(), new JpegFormat(), new GifFormat()],
            [new GifFormat()]
        ) { }

    /// <summary>
    /// Gets or sets the animation formats.
    /// </summary>
    /// <value>
    /// The animation formats.
    /// </value>
    private List<IAnimationFormat> AnimationFormats { get; } = [.. animationFormats];

    /// <summary>
    /// Gets or sets the formats.
    /// </summary>
    /// <value>The formats.</value>
    private List<IFormat> Formats { get; } = [.. formats];

    /// <summary>
    /// Decodes the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>An image object of the stream</returns>
    public Image? Decode(Stream stream)
    {
        foreach (var format in Formats)
        {
            if (format.CanDecode(stream))
            {
                return format.Decode(stream);
            }
        }
        return null;
    }

    /// <summary>
    /// Decodes the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>An animation object of the stream</returns>
    public Animation? DecodeAnimation(Stream stream)
    {
        foreach (var format in AnimationFormats)
        {
            if (format.CanDecode(stream))
            {
                return format.DecodeAnimation(stream);
            }
        }
        return null;
    }

    /// <summary>
    /// Encodes the image and saves it to the specified file name
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="image">The image.</param>
    /// <returns>True if it is encoded successfully, false otherwise</returns>
    public bool Encode(string fileName, Image image)
    {
        foreach (var format in Formats)
        {
            if (format.CanEncode(fileName))
            {
                new FileInfo(fileName).Directory.Create();
                using var imageFile = File.OpenWrite(fileName);
                return Encode(imageFile, image, format.Format);
            }
        }
        return false;
    }

    /// <summary>
    /// Encodes the animation and saves it to the specified file name
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="animation">The animation.</param>
    /// <returns>
    /// True if it is encoded successfully, false otherwise
    /// </returns>
    public bool Encode(string fileName, Animation animation)
    {
        foreach (var format in AnimationFormats)
        {
            if (format.CanEncode(fileName))
            {
                new FileInfo(fileName).Directory.Create();
                using var imageFile = File.OpenWrite(fileName);
                return Encode(imageFile, animation, format.Format);
            }
        }
        return Encode(fileName, animation[0]);
    }

    /// <summary>
    /// Encodes the animation and saves it to the specified file name
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="format">The format.</param>
    /// <returns>
    /// True if it is encoded successfully, false otherwise
    /// </returns>
    public bool Encode(Stream stream, Animation animation, FileFormats format)
    {
        using var tempWriter = new BinaryWriter(stream);
        return AnimationFormats.First(x => x.Format == format).Encode(tempWriter, animation);
    }

    /// <summary>
    /// Encodes the image and saves it to the specified stream
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="image">The image.</param>
    /// <param name="format">The format.</param>
    /// <returns>True if it is encoded successfully, false otherwise</returns>
    public bool Encode(Stream stream, Image image, FileFormats format)
    {
        using var tempWriter = new BinaryWriter(stream);
        return Formats.First(x => x.Format == format).Encode(tempWriter, image);
    }
}
