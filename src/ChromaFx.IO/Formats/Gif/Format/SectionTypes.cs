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

namespace ChromaFx.IO.Formats.Gif.Format;

/// <summary>
/// Section types class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SectionTypes"/> class.
/// </remarks>
/// <param name="value">The value.</param>
public class SectionTypes(byte value)
{

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public byte Value { get; } = value;

    /// <summary>
    /// The application extension label
    /// </summary>
    public static SectionTypes ApplicationExtensionLabel = 0xFF;

    /// <summary>
    /// The comment label
    /// </summary>
    public static SectionTypes CommentLabel = 0xFE;

    /// <summary>
    /// The end introducer
    /// </summary>
    public static SectionTypes EndIntroducer = 0x3B;

    /// <summary>
    /// The extension introducer
    /// </summary>
    public static SectionTypes ExtensionIntroducer = 0x21;

    /// <summary>
    /// The graphic control label
    /// </summary>
    public static SectionTypes GraphicControlLabel = 0xF9;

    /// <summary>
    /// The image label
    /// </summary>
    public static SectionTypes ImageLabel = 0x2C;

    /// <summary>
    /// The plain text label
    /// </summary>
    public static SectionTypes PlainTextLabel = 0x01;

    /// <summary>
    /// The terminator
    /// </summary>
    public static SectionTypes Terminator = 0;

    /// <summary>
    /// Performs an implicit conversion from <see cref="SectionTypes"/> to <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator byte(SectionTypes value)
    {
        return value.Value;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="byte"/> to <see cref="SectionTypes"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator SectionTypes(byte value)
    {
        return new SectionTypes(value);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(SectionTypes value1, SectionTypes value2)
    {
        return !(value1 == value2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(SectionTypes value1, SectionTypes value2)
    {
        if (value1 is null || value2 is null)
            return false;
        return value1.Value == value2.Value;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (!(obj is SectionTypes value2))
            return false;
        return this == value2;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return Value.ToString();
    }
}