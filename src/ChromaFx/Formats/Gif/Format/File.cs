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

using ChromaFx.Formats.BaseClasses;
using ChromaFx.IO;
using ChromaFx.IO.Converters.BaseClasses;
using ChromaFx.Quantizers;
using ChromaFx.Quantizers.Interfaces;
using ChromaFx.Quantizers.Octree;

namespace ChromaFx.Formats.Gif.Format;

/// <summary>
/// Gif file class
/// </summary>
/// <seealso cref="FileBase"/>
public class File : FileBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class.
    /// </summary>
    public File()
    {
        Frames = [];
        Quantizer = new OctreeQuantizer { TransparencyThreshold = TransparencyThreshold };
        BitDepth = (int)Math.Ceiling(Math.Log(Quality, 2));
    }

    /// <summary>
    /// Gets the application extension.
    /// </summary>
    /// <value>The application extension.</value>
    public ApplicationExtension AppExtension { get; private set; }

    /// <summary>
    /// Gets the bit depth.
    /// </summary>
    /// <value>The bit depth.</value>
    public int BitDepth { get; }

    /// <summary>
    /// Gets the color table.
    /// </summary>
    /// <value>The color table.</value>
    public ColorTable ColorTable { get; private set; }

    /// <summary>
    /// Gets the frames.
    /// </summary>
    /// <value>The frames.</value>
    public List<Frame> Frames { get; }

    /// <summary>
    /// Gets the graphics control extension.
    /// </summary>
    /// <value>The graphics control extension.</value>
    public GraphicsControl GraphicsControlExtension { get; private set; }

    /// <summary>
    /// Gets the header.
    /// </summary>
    /// <value>The header.</value>
    public FileHeader Header { get; private set; }

    /// <summary>
    /// Gets or sets the quality.
    /// </summary>
    /// <value>The quality.</value>
    public int Quality { get; set; } = 256;

    /// <summary>
    /// Gets or sets the quantizer.
    /// </summary>
    /// <value>The quantizer.</value>
    public IQuantizer Quantizer { get; set; }

    /// <summary>
    /// Gets the screen descriptor.
    /// </summary>
    /// <value>The screen descriptor.</value>
    public LogicalScreenDescriptor ScreenDescriptor { get; private set; }

    /// <summary>
    /// Gets or sets the transparency threshold.
    /// </summary>
    /// <value>The transparency threshold.</value>
    public byte TransparencyThreshold { get; set; } = 128;

    /// <summary>
    /// Decodes the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>This.</returns>
    public override FileBase Decode(Stream stream)
    {
        Header = FileHeader.Read(stream);
        ScreenDescriptor = LogicalScreenDescriptor.Read(stream);
        if (ScreenDescriptor.GlobalColorTablePresent)
        {
            ColorTable = ColorTable.Read(stream, ScreenDescriptor.GlobalColorTableSize);
        }
        var flag = stream.ReadByte();
        while (flag != SectionTypes.Terminator)
        {
            if (flag == SectionTypes.ImageLabel)
            {
                Frames.Add(Frame.Read(stream, ColorTable, GraphicsControlExtension, ScreenDescriptor, Frames));
            }
            else if (flag == SectionTypes.ExtensionIntroducer)
            {
                var label = (SectionTypes)stream.ReadByte();
                if (label == SectionTypes.GraphicControlLabel)
                {
                    GraphicsControlExtension = GraphicsControl.Read(stream);
                }
                else if (label == SectionTypes.CommentLabel)
                {
                    Comment.Read(stream);
                }
                else if (label == SectionTypes.ApplicationExtensionLabel)
                {
                    ApplicationExtension.Read(stream);
                }
                else if (label == SectionTypes.PlainTextLabel)
                {
                    PlainText.Read(stream);
                }
            }
            else if (flag == SectionTypes.EndIntroducer)
            {
                break;
            }

            flag = stream.ReadByte();
        }
        return this;
    }

    /// <summary>
    /// Writes to the specified stream.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="animation">The animation.</param>
    /// <returns>True if it writes successfully, false otherwise.</returns>
    public override bool Write(BinaryWriter writer, Animation animation)
    {
        var quantized = Quantizer.Quantize(animation[0], Quality);
        LoadAnimation(animation, quantized);
        WriteToFile(writer);
        return true;
    }

    /// <summary>
    /// Writes to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="image">The image.</param>
    /// <returns>True if it writes successfully, false otherwise.</returns>
    public override bool Write(BinaryWriter stream, Image image)
    {
        var quantized = Quantizer.Quantize(image, Quality);
        LoadImage(image, quantized);
        WriteToFile(stream);
        return true;
    }

    /// <summary>
    /// Converts the file to an animation
    /// </summary>
    /// <returns>The animation version of the file</returns>
    protected override Animation ToAnimation()
    {
        var delay = (GraphicsControlExtension ?? new GraphicsControl(0, 0, false, Enums.DisposalMethod.Undefined)).Delay;
        return new Animation(Frames.Select(x => new Image(ScreenDescriptor.Width, ScreenDescriptor.Height, x.Data)), delay);
    }

    /// <summary>
    /// Converts the file to an image.
    /// </summary>
    /// <returns>The image version of the file.</returns>
    protected override Image ToImage()
    {
        return new Image(ScreenDescriptor.Width, ScreenDescriptor.Height, Frames[0].Data);
    }

    /// <summary>
    /// Loads the animation.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="quantizedImage">The quantized image.</param>
    private void LoadAnimation(Animation animation, QuantizedImage quantizedImage)
    {
        var tempImage = animation[0];
        var transparencyIndex = quantizedImage.TransparentIndex;

        Header = new FileHeader();
        ScreenDescriptor = new LogicalScreenDescriptor(tempImage, transparencyIndex, BitDepth);
        Frames.Add(new Frame(tempImage, quantizedImage, BitDepth, animation.Delay));
        if (animation.Count > 1)
        {
            AppExtension = new ApplicationExtension(animation.RepeatCount, animation.Count);
            for (var x = 1; x < animation.Count; ++x)
            {
                quantizedImage = Quantizer.Quantize(animation[x], Quality);
                tempImage = animation[x];
                Frames.Add(new Frame(tempImage, quantizedImage, BitDepth, animation.Delay));
            }
        }
    }

    /// <summary>
    /// Loads the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="quantizedImage">The quantized image.</param>
    private void LoadImage(Image image, QuantizedImage quantizedImage)
    {
        Header = new FileHeader();
        ScreenDescriptor = new LogicalScreenDescriptor(image, quantizedImage.TransparentIndex, BitDepth);
        Frames.Add(new Frame(image, quantizedImage, BitDepth, 150));
    }

    private void WriteToFile(BinaryWriter writer)
    {
        using var writer2 = new EndianBinaryWriter(EndianBitConverterBase.LittleEndian, writer.BaseStream);
        Header.Write(writer2);
        ScreenDescriptor.Write(writer2);
        Frames[0].Write(writer2);
        if (Frames.Count > 1)
        {
            AppExtension.Write(writer2);
            for (var x = 0; x < Frames.Count; ++x)
            {
                Frames[x].Write(writer2);
            }
        }
        writer.Write(SectionTypes.EndIntroducer);
    }
}