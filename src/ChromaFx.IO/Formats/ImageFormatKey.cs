/*
 * Copyright 2026 Ho Tzin Mein
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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Open format id. Equality is the name compared ordinal-ignore-case.
/// Well-known managed ids live on <see cref="ManagedFormats"/>; other packages
/// declare their own <see cref="ImageFormatKey"/> values.
/// </summary>
public readonly struct ImageFormatKey : IEquatable<ImageFormatKey>
{
    /// <summary>
    /// Creates a key for <paramref name="name"/>.
    /// </summary>
    public ImageFormatKey(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the canonical format name. Extensions are not part of the key.
    /// </summary>
    public string Name { get; } = "";

    /// <inheritdoc />
    public bool Equals(ImageFormatKey other) =>
        string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is ImageFormatKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        Name.Length == 0 ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// Compares two keys by name, ordinal-ignore-case.
    /// </summary>
    public static bool operator ==(ImageFormatKey left, ImageFormatKey right) => left.Equals(right);

    /// <summary>
    /// Compares two keys by name, ordinal-ignore-case.
    /// </summary>
    public static bool operator !=(ImageFormatKey left, ImageFormatKey right) => !left.Equals(right);
}
