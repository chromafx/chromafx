/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

using ChromaFx.Core.ExtensionMethods;

namespace ChromaFx.IO.Formats.Gif.Format.Helpers;

/// <summary>
/// LZW Decoder
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LzwDecoder"/> class.
/// </remarks>
/// <param name="stream">The stream.</param>
public class LzwDecoder(Stream stream)
{

    /// <summary>
    /// Gets or sets the stream.
    /// </summary>
    /// <value>
    /// The stream.
    /// </value>
    public Stream Stream { get; set; } = stream;

    /// <summary>
    /// Decodes the stream to a byte array.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="dataSize">Size of the data.</param>
    /// <returns>The resulting byte array.</returns>
    public byte[] Decode(int width, int height, int dataSize)
    {
        var pixels = new byte[width * height];
        var clearCode = 1 << dataSize;
        var codeSize = dataSize + 1;
        var endCode = clearCode + 1;
        var availableCode = clearCode + 2;
        var oldCode = -1;
        var codeMask = (1 << codeSize) - 1;
        var bits = 0;

        var prefix = new int[4096];
        var suffix = new int[4096];
        var pixelStack = new int[4097];

        var top = 0;
        var count = 0;
        var bi = 0;
        var xyz = 0;

        var data = 0;
        var first = 0;

        for (var x = 0; x < clearCode; ++x)
        {
            prefix[x] = 0;
            suffix[x] = (byte)x;
        }

        byte[] buffer = null;
        while (xyz < pixels.Length)
        {
            if (top == 0)
            {
                if (bits < codeSize)
                {
                    if (count == 0)
                    {
                        buffer = ReadBlock();
                        count = buffer.Length;
                        if (count == 0)
                        {
                            break;
                        }

                        bi = 0;
                    }

                    if (buffer != null)
                    {
                        data += buffer[bi] << bits;
                    }

                    bits += 8;
                    bi++;
                    count--;
                    continue;
                }
                var code = data & codeMask;
                data >>= codeSize;
                bits -= codeSize;

                if (code > availableCode || code == endCode)
                {
                    break;
                }

                if (code == clearCode)
                {
                    codeSize = dataSize + 1;
                    codeMask = (1 << codeSize) - 1;
                    availableCode = clearCode + 2;
                    oldCode = -1;
                    continue;
                }

                if (oldCode == -1)
                {
                    pixelStack[top++] = suffix[code];
                    oldCode = code;
                    first = code;
                    continue;
                }

                var inCode = code;
                if (code == availableCode)
                {
                    pixelStack[top++] = (byte)first;

                    code = oldCode;
                }

                while (code > clearCode)
                {
                    pixelStack[top++] = suffix[code];
                    code = prefix[code];
                }

                first = suffix[code];
                pixelStack[top++] = suffix[code];

                if (availableCode < 4096)
                {
                    prefix[availableCode] = oldCode;
                    suffix[availableCode] = first;
                    availableCode++;
                    if (availableCode == codeMask + 1 && availableCode < 4096)
                    {
                        codeSize++;
                        codeMask = (1 << codeSize) - 1;
                    }
                }

                oldCode = inCode;
            }

            top--;
            pixels[xyz++] = (byte)pixelStack[top];
        }

        return pixels;
    }

    /// <summary>
    /// Reads the next block.
    /// </summary>
    /// <returns>The next block</returns>
    private byte[] ReadBlock()
    {
        var size = Stream.ReadByte();
        return Stream.ReadBytes(size);
    }
}