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

namespace ChromaFx.IO;

/// <summary>
/// Bit reader
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BitReader"/> class.
/// </remarks>
/// <param name="stream">The stream.</param>
public class BitReader(Stream stream) : IDisposable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BitReader"/> class.
    /// </summary>
    /// <param name="byteArray">The byte array.</param>
    public BitReader(byte[] byteArray)
        : this(new MemoryStream(byteArray))
    {
    }

    /// <summary>
    /// Gets the current bit.
    /// </summary>
    /// <value>The current bit.</value>
    private int CurrentBit { get; set; } = 8;

    /// <summary>
    /// Gets or sets the current byte.
    /// </summary>
    /// <value>The current byte.</value>
    private byte CurrentByte { get; set; } = 0;

    /// <summary>
    /// Gets or sets the internal stream.
    /// </summary>
    /// <value>The internal stream.</value>
    private Stream InternalStream { get; set; } = stream;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (InternalStream != null)
        {
            InternalStream.Dispose();
            InternalStream = null;
        }
    }

    /// <summary>
    /// Reads the next bit.
    /// </summary>
    /// <param name="bigEndian">if set to <c>true</c> [big endian].</param>
    /// <returns>The next bit value in the stream</returns>
    public bool? ReadBit(bool bigEndian = false)
    {
        if (CurrentBit == 8)
        {
            var tempByte = InternalStream.ReadByte();
            if (tempByte == -1)
                return null;
            CurrentBit = 0;
            CurrentByte = (byte)tempByte;
        }
        bool tempValue;
        if (bigEndian)
            tempValue = (CurrentByte & (1 << CurrentBit)) > 0;
        else
            tempValue = (CurrentByte & (1 << (7 - CurrentBit))) > 0;
        ++CurrentBit;
        return tempValue;
    }

    /// <summary>
    /// Skips the specified bit count.
    /// </summary>
    /// <param name="bitCount">The bit count.</param>
    public void Skip(int bitCount)
    {
        if (CurrentBit == 8)
        {
            var tempByte = InternalStream.ReadByte();
            if (tempByte == -1)
                return;
            CurrentBit = 0;
            CurrentByte = (byte)tempByte;
        }
        for (var x = 0; x < bitCount; ++x)
        {
            ++CurrentBit;
            if (CurrentBit == 8)
            {
                var tempByte = InternalStream.ReadByte();
                if (tempByte == -1)
                    return;
                CurrentBit = 0;
                CurrentByte = (byte)tempByte;
            }
        }
    }
}