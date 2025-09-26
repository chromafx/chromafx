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

namespace ChromaFx.ExtensionMethods;

/// <summary>
/// Byte array extension methods
/// </summary>
public static class ByteExtensions
{
    /// <summary>
    /// Expands the array.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="bits">The bits.</param>
    /// <returns>The expanded array of bytes</returns>
    public static byte[] ExpandArray(this byte[] bytes, int bits)
    {
        bytes ??= [];
        bits = Math.Clamp(bits, 0, int.MaxValue);
        if (bits >= 8)
        {
            return bytes;
        }

        var result = new byte[bytes.Length * 8 / bits];
        var mask = 0xFF >> (8 - bits);
        var offset = 0;
        foreach (var tempByte in bytes)
        {
            for (var x = 0; x < 8; x += bits)
            {
                result[offset] = (byte)((tempByte >> (8 - bits - x)) & mask);
                ++offset;
            }
        }

        return result;
    }

    /// <summary>
    /// Linearly interpolates a value to the destination based on the amount specified
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The resulting byte</returns>
    public static byte Lerp(this byte value1, byte value2, float amount)
    {
        return (byte)(value1 + (value2 - value1) * amount);
    }

    /// <summary>
    /// Converts an int to a byte value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static byte ToByte(this int value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }

    /// <summary>
    /// Converts a float to a byte value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static byte ToByte(this float value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }

    /// <summary>
    /// Converts a double to a byte value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static byte ToByte(this double value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }
}
