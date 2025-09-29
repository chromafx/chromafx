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

namespace ChromaFx.IO.Formats.Gif.Format.Helpers;

/// <summary>
/// Packed field
/// </summary>
/// <seealso cref="IEquatable{PackedField}" />
public readonly struct PackedField : IEquatable<PackedField>
{
    /// <summary>
    /// Gets the byte which represents the data items held in this instance.
    /// </summary>
    public byte Byte
    {
        get
        {
            var returnValue = 0;
            var bitShift = 7;
            foreach (var bit in Bits)
            {
                int bitValue;
                if (bit)
                {
                    bitValue = 1 << bitShift;
                }
                else
                {
                    bitValue = 0;
                }
                returnValue |= bitValue;
                bitShift--;
            }
            return Convert.ToByte(returnValue & 0xFF);
        }
    }

    /// <summary>
    /// The individual bits representing the packed byte.
    /// </summary>
    private static readonly bool[] Bits = new bool[8];

    /// <summary>
    /// Returns a new <see cref="PackedField"/>  with the bits in the packed fields to
    /// the corresponding bits from the supplied byte.
    /// </summary>
    /// <param name="value">The value to pack.</param>
    /// <returns>The <see cref="PackedField"/></returns>
    public static PackedField FromInt(byte value)
    {
        var packed = new PackedField();
        packed.SetBits(0, 8, value);
        return packed;
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="field1">The field1.</param>
    /// <param name="field2">The field2.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(PackedField field1, PackedField field2)
    {
        return !(field1 == field2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="field1">The field1.</param>
    /// <param name="field2">The field2.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(PackedField field1, PackedField field2)
    {
        return field1.Byte == field2.Byte;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        var field = obj as PackedField?;

        return Byte == field?.Byte;
    }

    /// <inheritdoc/>
    public bool Equals(PackedField other)
    {
        return Byte.Equals(other.Byte);
    }

    /// <summary>
    /// Gets the value of the specified bit within the byte.
    /// </summary>
    /// <param name="index">The zero-based index of the bit to get.</param>
    /// <returns>
    /// The value of the specified bit within the byte.
    /// </returns>
    public bool GetBit(int index)
    {
        if (index is >= 0 and <= 7) return Bits[index];
        var message = $"Index must be between 0 and 7. Supplied index: {index}";
        throw new ArgumentOutOfRangeException(nameof(index), message);
    }

    /// <summary>
    /// Gets the value of the specified bits within the byte.
    /// </summary>
    /// <param name="startIndex">The zero-based index of the first bit to get.</param>
    /// <param name="length">The number of bits to get.</param>
    /// <returns>
    /// The value of the specified bits within the byte.
    /// </returns>
    public int GetBits(int startIndex, int length)
    {
        if (startIndex is < 0 or > 7)
        {
            var message = $"Start index must be between 0 and 7. Supplied index: {startIndex}";
            throw new ArgumentOutOfRangeException(nameof(startIndex), message);
        }

        if (length < 1 || startIndex + length > 8)
        {
            var message = "Length must be greater than zero and the sum of length and start index must be less than 8. "
                          + $"Supplied length: {length}. Supplied start index: {startIndex}";

            throw new ArgumentOutOfRangeException(nameof(length), message);
        }

        var returnValue = 0;
        var bitShift = length - 1;
        for (var i = startIndex; i < startIndex + length; i++)
        {
            var bitValue = (Bits[i] ? 1 : 0) << bitShift;
            returnValue += bitValue;
            bitShift--;
        }
        return returnValue;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Byte.GetHashCode();
    }

    /// <summary>
    /// Sets the specified bit within the packed fields to the supplied
    /// value.
    /// </summary>
    /// <param name="index">
    /// The zero-based index within the packed fields of the bit to set.
    /// </param>
    /// <param name="valueToSet">
    /// The value to set the bit to.
    /// </param>
    public void SetBit(int index, bool valueToSet)
    {
        if (index is < 0 or > 7)
        {
            var message
                = "Index must be between 0 and 7. Supplied index: "
                  + index;
            throw new ArgumentOutOfRangeException(nameof(index), message);
        }
        Bits[index] = valueToSet;
    }

    /// <summary>
    /// Sets the specified bits within the packed fields to the supplied
    /// value.
    /// </summary>
    /// <param name="startIndex">The zero-based index within the packed fields of the first bit to  set.</param>
    /// <param name="length">The number of bits to set.</param>
    /// <param name="valueToSet">The value to set the bits to.</param>
    public void SetBits(int startIndex, int length, int valueToSet)
    {
        if (startIndex is < 0 or > 7)
        {
            var message = $"Start index must be between 0 and 7. Supplied index: {startIndex}";
            throw new ArgumentOutOfRangeException(nameof(startIndex), message);
        }

        if (length < 1 || startIndex + length > 8)
        {
            var message = "Length must be greater than zero and the sum of length and start index must be less than 8. "
                          + $"Supplied length: {length}. Supplied start index: {startIndex}";
            throw new ArgumentOutOfRangeException(nameof(length), message);
        }

        var bitShift = length - 1;
        for (var i = startIndex; i < startIndex + length; i++)
        {
            var bitValueIfSet = 1 << bitShift;
            var bitValue = valueToSet & bitValueIfSet;
            var bitIsSet = bitValue >> bitShift;
            Bits[i] = bitIsSet == 1;
            bitShift--;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"PackedField [ Byte={Byte} ]";
    }
}