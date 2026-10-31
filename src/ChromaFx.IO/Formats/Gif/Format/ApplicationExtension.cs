/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

namespace ChromaFx.IO.Formats.Gif.Format;

/// <summary>
/// Application extension (NETSCAPE 2.0 loop count).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationExtension"/> class.
/// </remarks>
/// <param name="repeatCount">
/// Animation repeat count: 0 = infinite, 1 = play once, N = play N times.
/// </param>
/// <param name="frameCount">The frame count.</param>
internal class ApplicationExtension(ushort repeatCount, int frameCount) : SectionBase
{
    private const byte NetscapeIdentifierLength = 11;
    private static readonly byte[] NetscapeIdentifier = "NETSCAPE2.0"u8.ToArray();

    /// <summary>
    /// Gets or sets the frame count.
    /// </summary>
    public int FrameCount { get; set; } = frameCount;

    /// <summary>
    /// Gets or sets the repeat count (0 = infinite, 1 = play once, N = play N times).
    /// </summary>
    public int RepeatCount { get; set; } = repeatCount;

    /// <summary>
    /// True when this extension was a NETSCAPE 2.0 loop block.
    /// </summary>
    public bool IsNetscapeLoop { get; private set; }

    /// <summary>
    /// Reads the application extension at the current stream position (after the label byte).
    /// Non-NETSCAPE app blocks are skipped; <see cref="IsNetscapeLoop"/> is false.
    /// </summary>
    public static ApplicationExtension Read(Stream stream)
    {
        var blockSize = stream.ReadByte();
        if (blockSize <= 0)
            return new ApplicationExtension(1, 0);

        var identifier = new byte[blockSize];
        stream.ReadExactly(identifier);
        var isNetscape =
            blockSize == NetscapeIdentifierLength
            && identifier.AsSpan().SequenceEqual(NetscapeIdentifier);

        ushort? wireLoops = null;
        var subSize = stream.ReadByte();
        while (subSize > 0)
        {
            var data = new byte[subSize];
            stream.ReadExactly(data);
            if (isNetscape && subSize >= 3 && data[0] == 1)
                wireLoops = (ushort)(data[1] | (data[2] << 8));
            subSize = stream.ReadByte();
        }

        if (!isNetscape || wireLoops is null)
            return new ApplicationExtension(1, 0);

        // Wire 0 = infinite → Animation 0; wire N → Animation N+1 (play count).
        var animationRepeat =
            wireLoops.Value == 0 ? (ushort)0 : (ushort)(wireLoops.Value + 1);
        return new ApplicationExtension(animationRepeat, 0) { IsNetscapeLoop = true };
    }

    /// <summary>
    /// Writes the NETSCAPE 2.0 loop block when the animation should not play exactly once.
    /// </summary>
    public override bool Write(EndianBinaryWriter writer)
    {
        if (RepeatCount == 1 || FrameCount <= 0)
            return true;

        writer.Write(
            [
                SectionTypes.ExtensionIntroducer,
                SectionTypes.ApplicationExtensionLabel,
                NetscapeIdentifierLength,
            ]
        );
        writer.Write(NetscapeIdentifier);
        writer.Write((byte)3);
        writer.Write((byte)1);

        // Animation 0 (infinite) → wire 0; Animation N → wire N-1.
        var wire = RepeatCount == 0 ? (ushort)0 : (ushort)(RepeatCount - 1);
        writer.Write(wire);
        writer.Write(SectionTypes.Terminator);
        return true;
    }
}
