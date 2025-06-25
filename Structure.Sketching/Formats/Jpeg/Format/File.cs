﻿/*
Copyright 2016 James Craig
Copyright 2023 Ho Tzin Mein

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

using Structure.Sketching.Exceptions;
using Structure.Sketching.Formats.BaseClasses;
using Structure.Sketching.Formats.Jpeg.Format.HelperClasses;
using Structure.Sketching.Formats.Jpeg.Format.Segments;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Structure.Sketching.Formats.Jpeg.Format.Segments.BaseClasses;

namespace Structure.Sketching.Formats.Jpeg.Format;

/// <summary>
/// JPEG file class
/// </summary>
/// <seealso cref="Structure.Sketching.Formats.BaseClasses.FileBase" />
public class File : FileBase
{
    /// <summary>
    /// Gets the header.
    /// </summary>
    /// <value>
    /// The header.
    /// </value>
    public FileHeader Header { get; }

    /// <summary>
    /// Gets the segments.
    /// </summary>
    /// <value>
    /// The segments.
    /// </value>
    public List<SegmentBase> Segments { get; private set; }

    /// <summary>
    /// Gets the sof segment.
    /// </summary>
    /// <value>
    /// The sof segment.
    /// </value>
    public StartOfFrame SofSegment { get; }

    private Image _returnValue = new(1, 1);

    /// <summary>
    /// Decodes the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>
    /// This.
    /// </returns>
    public override FileBase Decode(Stream stream)
    {
        _returnValue = ReadSegments(stream);
        return this;
    }

    /// <summary>
    /// Writes to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="image">The image.</param>
    /// <returns>
    /// True if it writes successfully, false otherwise.
    /// </returns>
    public override bool Write(BinaryWriter stream, Image image)
    {
        new StartOfImage().Write(stream);
        var quantizationTableSegment = new DefineQuantizationTable(90);
        quantizationTableSegment.Write(stream);
        new StartOfFrame(image).Write(stream);
        var huffmanTableSegment = new DefineHuffmanTable(null);
        huffmanTableSegment.Write(stream);
        new StartOfScan(image, huffmanTableSegment, quantizationTableSegment).Write(stream);
        new EndOfImage(null).Write(stream);
        return true;
    }

    /// <summary>
    /// Writes to the specified stream.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="animation">The animation.</param>
    /// <returns>
    /// True if it writes successfully, false otherwise.
    /// </returns>
    public override bool Write(BinaryWriter writer, Animation animation)
    {
        return Write(writer, animation[0]);
    }

    /// <summary>
    /// Converts the file to an animation.
    /// </summary>
    /// <returns>
    /// The animation version of the file.
    /// </returns>
    protected override Animation ToAnimation()
    {
        return new Animation(new[] { ToImage() }, 0);
    }

    /// <summary>
    /// Converts the file to an image.
    /// </summary>
    /// <returns>
    /// The image version of the file.
    /// </returns>
    protected override Image ToImage()
    {
        return _returnValue;
    }

    /// <summary>
    /// Loads the image.
    /// </summary>
    /// <param name="image">The image.</param>
    private static void LoadImage(Image image)
    {
    }

    private Image ReadSegments(Stream stream)
    {
        var bytes = new ByteBuffer(stream);
        Segments = new List<SegmentBase>();
        while (true)
        {
            var tempSegment = SegmentBase.Read(bytes, Segments);
            if (tempSegment == null)
                continue;
            tempSegment.Setup(Segments);
            if (!Segments.Contains(tempSegment))
                Segments.Add(tempSegment);
            if (tempSegment.Type == SegmentTypes.EndOfImage)
                break;
        }

        var startOfScanSegment = Segments.OfType<StartOfScan>().FirstOrDefault();
        return startOfScanSegment == null
            ? throw new ImageException("No scan information found.")
            : startOfScanSegment.Convert(_returnValue, Segments);
    }
}
