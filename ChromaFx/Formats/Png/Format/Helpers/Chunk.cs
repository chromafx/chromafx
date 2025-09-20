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

using ChromaFx.Exceptions;
using System;
using System.IO;
using System.Text;

namespace ChromaFx.Formats.Png.Format.Helpers;

/// <summary>
/// An individual chunk of data
/// </summary>
public class Chunk
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Chunk"/> class.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="type">The type.</param>
    /// <param name="data">The data.</param>
    /// <param name="crc">The CRC.</param>
    public Chunk(int length, string type, byte[] data, uint crc)
    {
        Length = length;
        Type = type;
        Data = data;
        Crc = crc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Chunk"/> class.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="type">The type.</param>
    /// <param name="data">The data.</param>
    public Chunk(int length, string type, byte[] data)
    {
        Length = length;
        Type = type;
        Data = data;
        Crc = CalculateCrc();
    }

    /// <summary>
    /// The CRC data.
    /// </summary>
    /// <value>
    /// The CRC.
    /// </value>
    public uint Crc { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public byte[] Data { get; set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    /// <value>
    /// The length.
    /// </value>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public ChunkTypes Type { get; set; }

    /// <summary>
    /// Reads the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The chunk information</returns>
    public static Chunk Read(Stream stream)
    {
        var length = ReadLength(stream);
        if (length == 0)
            return null;
        var typeBuffer = new byte[4];
        var type = ReadType(stream, typeBuffer);
        var data = ReadData(stream, length);
        var crc = ReadCrc(stream, data, typeBuffer);
        return new Chunk(length, type, data, crc);
    }

    /// <summary>
    /// Writes the chunk to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Write(BinaryWriter stream)
    {
        stream.Write(GetValueAsArray(Length));
        stream.Write(
            new[]
            {
                (byte)((string)Type)[0],
                (byte)((string)Type)[1],
                (byte)((string)Type)[2],
                (byte)((string)Type)[3]
            }
        );
        stream.Write(Data);
        stream.Write(GetValueAsArray(Crc));
    }

    /// <summary>
    /// Reads the CRC.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="data">The data.</param>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    /// <exception cref="ImageException">image is corrupt</exception>
    private static uint ReadCrc(Stream stream, byte[] data, byte[] type)
    {
        var tempBuffer = new byte[4];
        var numberOfBytes = stream.Read(tempBuffer, 0, 4);
        if (numberOfBytes != 4)
            return 0;
        Array.Reverse(tempBuffer);
        var tempValue = BitConverter.ToUInt32(tempBuffer, 0);
        var crc = new Crc32();
        crc.Update(type);
        if (tempValue != (uint)crc.Update(data))
            throw new ImageException("image is corrupt");
        return tempValue;
    }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="length">The length.</param>
    /// <returns>
    /// The data of the chunk
    /// </returns>
    private static byte[] ReadData(Stream stream, int length)
    {
        var returnValue = new byte[length];
        stream.Read(returnValue, 0, length);
        return returnValue;
    }

    /// <summary>
    /// Reads the length.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The length of the chunk</returns>
    private static int ReadLength(Stream stream)
    {
        var tempBuffer = new byte[4];
        var numberOfBytes = stream.Read(tempBuffer, 0, 4);
        if (numberOfBytes != 4)
            return 0;
        Array.Reverse(tempBuffer);
        return BitConverter.ToInt32(tempBuffer, 0);
    }

    /// <summary>
    /// Reads the type.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="typeBuffer">The type buffer.</param>
    /// <returns>
    /// The type of the chunk
    /// </returns>
    private static string ReadType(Stream stream, byte[] typeBuffer)
    {
        var numberOfBytes = stream.Read(typeBuffer, 0, 4);
        return numberOfBytes != 4
            ? string.Empty
            : new string(
                new[]
                {
                    (char)typeBuffer[0],
                    (char)typeBuffer[1],
                    (char)typeBuffer[2],
                    (char)typeBuffer[3]
                }
            );
    }

    private uint CalculateCrc()
    {
        var typeByteArray = Encoding.UTF8.GetBytes(Type);
        var crc = new Crc32();
        crc.Update(typeByteArray);
        return (uint)crc.Update(Data);
    }

    /// <summary>
    /// Gets the value as array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The value as an array</returns>
    private static byte[] GetValueAsArray(int value)
    {
        var tempBuffer = BitConverter.GetBytes(value);
        Array.Reverse(tempBuffer);
        return tempBuffer;
    }

    /// <summary>
    /// Gets the value as array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The value as an array</returns>
    private static byte[] GetValueAsArray(uint value)
    {
        var tempBuffer = BitConverter.GetBytes(value);
        Array.Reverse(tempBuffer);
        return tempBuffer;
    }
}
