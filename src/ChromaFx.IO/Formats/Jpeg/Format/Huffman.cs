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

using ChromaFx.IO.Formats.Jpeg.Format.Exceptions;

namespace ChromaFx.IO.Formats.Jpeg.Format;

/// <summary>
/// Huffman class
/// </summary>
internal class Huffman
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Huffman"/> class.
    /// </summary>
    public Huffman()
    {
        LookUpTable = new ushort[256];
        DecodedValues = new byte[256];
        MinimumCode = new int[16];
        MaximumCode = new int[16];
        ValueIndices = new int[16];
        NumberOfCodes = 0;
    }

    /// <summary>
    /// Gets the decoded values.
    /// </summary>
    /// <value>
    /// The decoded values.
    /// </value>
    public byte[] DecodedValues { get; }

    /// <summary>
    /// Gets the look up table.
    /// </summary>
    /// <value>
    /// The look up table.
    /// </value>
    public ushort[] LookUpTable { get; }

    /// <summary>
    /// Gets the maximum code.
    /// </summary>
    /// <value>
    /// The maximum code.
    /// </value>
    public int[] MaximumCode { get; }

    /// <summary>
    /// Gets the minimum code.
    /// </summary>
    /// <value>
    /// The minimum code.
    /// </value>
    public int[] MinimumCode { get; }

    /// <summary>
    /// Gets the number of codes.
    /// </summary>
    /// <value>
    /// The number of codes.
    /// </value>
    public int NumberOfCodes { get; set; }

    /// <summary>
    /// Gets the value indices.
    /// </summary>
    /// <value>
    /// The value indices.
    /// </value>
    public int[] ValueIndices { get; }

    /// <summary>
    /// Decodes the byte buffer using the huffman class.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns></returns>
    /// <exception cref="Exception">uninitialized Huffman table
    /// or
    /// bad Huffman code</exception>
    public byte DecodeHuffman(ByteBuffer bytes)
    {
        if (NumberOfCodes == 0)
            throw new Exception("uninitialized Huffman table");

        var v = LookUpTable[bytes.PeekByte()];
        if (v != 0)
        {
            bytes.SkipBits((v & 0xff) - 1);
            return (byte)(v >> 8);
        }

        var code = 0;
        for (var i = 0; i < 16; i++)
        {
            if (bytes.DecodeBit())
                code |= 1;
            if (code <= MaximumCode[i])
                return DecodedValues[ValueIndices[i] + code - MinimumCode[i]];
            code <<= 1;
        }

        throw new Exception("bad Huffman code");
    }
}
