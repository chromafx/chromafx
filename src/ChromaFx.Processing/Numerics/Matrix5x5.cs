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

using ChromaFx.Core.Colors;
using System.Globalization;
using System.Numerics;

namespace ChromaFx.Processing.Numerics;

/// <summary>
/// 5x5 matrix
/// </summary>
public readonly struct Matrix5X5 : IEquatable<Matrix5X5>
{
    /// <summary>
    /// Constructs a Matrix5x5 from the given components.
    /// </summary>
    /// <param name="m11">The M11.</param>
    /// <param name="m12">The M12.</param>
    /// <param name="m13">The M13.</param>
    /// <param name="m14">The M14.</param>
    /// <param name="m15">The M15.</param>
    /// <param name="m21">The M21.</param>
    /// <param name="m22">The M22.</param>
    /// <param name="m23">The M23.</param>
    /// <param name="m24">The M24.</param>
    /// <param name="m25">The M25.</param>
    /// <param name="m31">The M31.</param>
    /// <param name="m32">The M32.</param>
    /// <param name="m33">The M33.</param>
    /// <param name="m34">The M34.</param>
    /// <param name="m35">The M35.</param>
    /// <param name="m41">The M41.</param>
    /// <param name="m42">The M42.</param>
    /// <param name="m43">The M43.</param>
    /// <param name="m44">The M44.</param>
    /// <param name="m45">The M45.</param>
    /// <param name="m51">The M51.</param>
    /// <param name="m52">The M52.</param>
    /// <param name="m53">The M53.</param>
    /// <param name="m54">The M54.</param>
    /// <param name="m55">The M55.</param>
    public Matrix5X5(float m11, float m12, float m13, float m14, float m15,
        float m21, float m22, float m23, float m24, float m25,
        float m31, float m32, float m33, float m34, float m35,
        float m41, float m42, float m43, float m44, float m45,
        float m51, float m52, float m53, float m54, float m55)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M15 = m15;

        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M25 = m25;

        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
        M35 = m35;

        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
        M45 = m45;

        M51 = m51;
        M52 = m52;
        M53 = m53;
        M54 = m54;
        M55 = m55;
    }

    /// <summary>
    /// Constructs a Matrix5x5 from the given Matrix4x4.
    /// </summary>
    /// <param name="value">The source Matrix4x4.</param>
    public Matrix5X5(Matrix4x4 value)
    {
        M11 = value.M11;
        M12 = value.M12;
        M13 = value.M13;
        M14 = value.M14;
        M15 = 0f;
        M21 = value.M21;
        M22 = value.M22;
        M23 = value.M23;
        M24 = value.M24;
        M25 = 0f;
        M31 = value.M31;
        M32 = value.M32;
        M33 = value.M33;
        M34 = value.M34;
        M35 = 0f;
        M41 = value.M41;
        M42 = value.M42;
        M43 = value.M43;
        M44 = value.M44;
        M45 = 0f;
        M51 = 0f;
        M52 = 0f;
        M53 = 0f;
        M54 = 0f;
        M55 = 1f;
    }

    /// <summary>
    /// Gets the identity.
    /// </summary>
    /// <value>The identity.</value>
    public static Matrix5X5 Identity { get; } = new(
        1f, 0f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f, 0f,
        0f, 0f, 1f, 0f, 0f,
        0f, 0f, 0f, 1f, 0f,
        0f, 0f, 0f, 0f, 1f
    );

    /// <summary>
    /// Returns whether the matrix is the identity matrix.
    /// </summary>
    public readonly bool IsIdentity =>
        this == new Matrix5X5(1f, 0f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f, 0f,
            0f, 0f, 1f, 0f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 0f, 1f);

    /// <summary>
    /// Gets or sets the translation component of this matrix.
    /// </summary>
    public Vector4 Translation => new(M51, M52, M53, M54);

    /// <summary>
    /// Value at row 1, column 1 of the matrix.
    /// </summary>
    public readonly float M11;

    /// <summary>
    /// Value at row 1, column 2 of the matrix.
    /// </summary>
    public readonly float M12;

    /// <summary>
    /// Value at row 1, column 3 of the matrix.
    /// </summary>
    public readonly float M13;

    /// <summary>
    /// Value at row 1, column 4 of the matrix.
    /// </summary>
    public readonly float M14;

    /// <summary>
    /// Value at row 3, column 5 of the matrix.
    /// </summary>
    public readonly float M15;

    /// <summary>
    /// Value at row 2, column 1 of the matrix.
    /// </summary>
    public readonly float M21;

    /// <summary>
    /// Value at row 2, column 2 of the matrix.
    /// </summary>
    public readonly float M22;

    /// <summary>
    /// Value at row 2, column 3 of the matrix.
    /// </summary>
    public readonly float M23;

    /// <summary>
    /// Value at row 2, column 4 of the matrix.
    /// </summary>
    public readonly float M24;

    /// <summary>
    /// Value at row 2, column 5 of the matrix.
    /// </summary>
    public readonly float M25;

    /// <summary>
    /// Value at row 3, column 1 of the matrix.
    /// </summary>
    public readonly float M31;

    /// <summary>
    /// Value at row 3, column 2 of the matrix.
    /// </summary>
    public readonly float M32;

    /// <summary>
    /// Value at row 3, column 3 of the matrix.
    /// </summary>
    public readonly float M33;

    /// <summary>
    /// Value at row 3, column 4 of the matrix.
    /// </summary>
    public readonly float M34;

    /// <summary>
    /// Value at row 3, column 5 of the matrix.
    /// </summary>
    public readonly float M35;

    /// <summary>
    /// Value at row 4, column 1 of the matrix.
    /// </summary>
    public readonly float M41;

    /// <summary>
    /// Value at row 4, column 2 of the matrix.
    /// </summary>
    public readonly float M42;

    /// <summary>
    /// Value at row 4, column 3 of the matrix.
    /// </summary>
    public readonly float M43;

    /// <summary>
    /// Value at row 4, column 4 of the matrix.
    /// </summary>
    public readonly float M44;

    /// <summary>
    /// Value at row 4, column 5 of the matrix.
    /// </summary>
    public readonly float M45;

    /// <summary>
    /// Value at row 5, column 1 of the matrix.
    /// </summary>
    public readonly float M51;

    /// <summary>
    /// Value at row 5, column 2 of the matrix.
    /// </summary>
    public readonly float M52;

    /// <summary>
    /// Value at row 5, column 3 of the matrix.
    /// </summary>
    public readonly float M53;

    /// <summary>
    /// Value at row 5, column 4 of the matrix.
    /// </summary>
    public readonly float M54;

    /// <summary>
    /// Value at row 5, column 5 of the matrix.
    /// </summary>
    public readonly float M55;

    /// <summary>
    /// Returns a new matrix with the negated elements of the given matrix.
    /// </summary>
    /// <param name="value">The source matrix.</param>
    /// <returns>The negated matrix.</returns>
    public static Matrix5X5 operator -(Matrix5X5 value) => new(
        -value.M11, -value.M12, -value.M13, -value.M14, -value.M15,
        -value.M21, -value.M22, -value.M23, -value.M24, -value.M25,
        -value.M31, -value.M32, -value.M33, -value.M34, -value.M35,
        -value.M41, -value.M42, -value.M43, -value.M44, -value.M45,
        -value.M51, -value.M52, -value.M53, -value.M54, -value.M55
    );

    /// <summary>
    /// Subtracts the second matrix from the first.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Matrix5X5 operator -(Matrix5X5 value1, Matrix5X5 value2) => new(
        value1.M11 - value2.M11, value1.M12 - value2.M12, value1.M13 - value2.M13, value1.M14 - value2.M14, value1.M15 - value2.M15,
        value1.M21 - value2.M21, value1.M22 - value2.M22, value1.M23 - value2.M23, value1.M24 - value2.M24, value1.M25 - value2.M25,
        value1.M31 - value2.M31, value1.M32 - value2.M32, value1.M33 - value2.M33, value1.M34 - value2.M34, value1.M35 - value2.M35,
        value1.M41 - value2.M41, value1.M42 - value2.M42, value1.M43 - value2.M43, value1.M44 - value2.M44, value1.M45 - value2.M45,
        value1.M51 - value2.M51, value1.M52 - value2.M52, value1.M53 - value2.M53, value1.M54 - value2.M54, value1.M55 - value2.M55
    );

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are not equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are not equal; False if they are equal.</returns>
    public static bool operator !=(Matrix5X5 value1, Matrix5X5 value2) => value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 || value1.M14 != value2.M14 || value1.M15 != value2.M15 ||
           value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 || value1.M24 != value2.M24 || value1.M25 != value2.M25 ||
           value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33 || value1.M34 != value2.M34 || value1.M35 != value2.M35 ||
           value1.M41 != value2.M41 || value1.M42 != value2.M42 || value1.M43 != value2.M43 || value1.M44 != value2.M44 || value1.M45 != value2.M45 ||
           value1.M51 != value2.M51 || value1.M52 != value2.M52 || value1.M53 != value2.M53 || value1.M54 != value2.M54 || value1.M55 != value2.M55;

    /// <summary>
    /// Multiplies a matrix by another matrix.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Matrix5X5 operator *(Matrix5X5 value1, Matrix5X5 value2) =>
        new(
            // First row
            value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41 + value1.M15 * value2.M51,
            value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42 + value1.M15 * value2.M52,
            value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43 + value1.M15 * value2.M53,
            value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44 + value1.M15 * value2.M54,
            value1.M11 * value2.M15 + value1.M12 * value2.M25 + value1.M13 * value2.M35 + value1.M14 * value2.M45 + value1.M15 * value2.M55,
            // Second row
            value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41 + value1.M25 * value2.M51,
            value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42 + value1.M25 * value2.M52,
            value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43 + value1.M25 * value2.M53,
            value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44 + value1.M25 * value2.M54,
            value1.M21 * value2.M15 + value1.M22 * value2.M25 + value1.M23 * value2.M35 + value1.M24 * value2.M45 + value1.M25 * value2.M55,
            // Third row
            value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41 + value1.M35 * value2.M51,
            value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42 + value1.M35 * value2.M52,
            value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43 + value1.M35 * value2.M53,
            value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44 + value1.M35 * value2.M54,
            value1.M31 * value2.M15 + value1.M32 * value2.M25 + value1.M33 * value2.M35 + value1.M34 * value2.M45 + value1.M35 * value2.M55,
            // Fourth row
            value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41 + value1.M45 * value2.M51,
            value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42 + value1.M45 * value2.M52,
            value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43 + value1.M45 * value2.M53,
            value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44 + value1.M45 * value2.M54,
            value1.M41 * value2.M15 + value1.M42 * value2.M25 + value1.M43 * value2.M35 + value1.M44 * value2.M45 + value1.M45 * value2.M55,
            // Fifth row
            value1.M51 * value2.M11 + value1.M52 * value2.M21 + value1.M53 * value2.M31 + value1.M54 * value2.M41 + value1.M55 * value2.M51,
            value1.M51 * value2.M12 + value1.M52 * value2.M22 + value1.M53 * value2.M32 + value1.M54 * value2.M42 + value1.M55 * value2.M52,
            value1.M51 * value2.M13 + value1.M52 * value2.M23 + value1.M53 * value2.M33 + value1.M54 * value2.M43 + value1.M55 * value2.M53,
            value1.M51 * value2.M14 + value1.M52 * value2.M24 + value1.M53 * value2.M34 + value1.M54 * value2.M44 + value1.M55 * value2.M54,
            value1.M51 * value2.M15 + value1.M52 * value2.M25 + value1.M53 * value2.M35 + value1.M54 * value2.M45 + value1.M55 * value2.M55
        );

    /// <summary>
    /// Multiplies a matrix by a scalar value.
    /// </summary>
    /// <param name="value1">The source matrix.</param>
    /// <param name="value2">The scaling factor.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix5X5 operator *(Matrix5X5 value1, float value2) =>
        new(
            value1.M11 * value2, value1.M12 * value2, value1.M13 * value2, value1.M14 * value2, value1.M15 * value2,
            value1.M21 * value2, value1.M22 * value2, value1.M23 * value2, value1.M24 * value2, value1.M25 * value2,
            value1.M31 * value2, value1.M32 * value2, value1.M33 * value2, value1.M34 * value2, value1.M35 * value2,
            value1.M41 * value2, value1.M42 * value2, value1.M43 * value2, value1.M44 * value2, value1.M45 * value2,
            value1.M51 * value2, value1.M52 * value2, value1.M53 * value2, value1.M54 * value2, value1.M55 * value2
        );

    /// <summary>
    /// Multiplies a matrix by a float value.
    /// </summary>
    /// <param name="value1">The source matrix</param>
    /// <param name="value2">The vector</param>
    /// <returns>The resulting vector</returns>
    public static Color operator *(Matrix5X5 value1, Color value2)
    {
        var r2 = value2.Red / 255f;
        var g2 = value2.Green / 255f;
        var b2 = value2.Blue / 255f;
        var a2 = value2.Alpha / 255f;
        var v4 = new Vector4(
            r2 * value1.M11 + g2 * value1.M21 + b2 * value1.M31 + a2 * value1.M41 + value1.M51,
            r2 * value1.M12 + g2 * value1.M22 + b2 * value1.M32 + a2 * value1.M42 + value1.M52,
            r2 * value1.M13 + g2 * value1.M23 + b2 * value1.M33 + a2 * value1.M43 + value1.M53,
            r2 * value1.M14 + g2 * value1.M24 + b2 * value1.M34 + a2 * value1.M44 + value1.M54
        );
        // Clamp to [0,1] and convert to byte
        byte Rf = (byte)Math.Clamp(v4.X * 255f, 0, 255);
        byte Gf = (byte)Math.Clamp(v4.Y * 255f, 0, 255);
        byte Bf = (byte)Math.Clamp(v4.Z * 255f, 0, 255);
        byte Af = (byte)Math.Clamp(v4.W * 255f, 0, 255);
        return new Color(Rf, Gf, Bf, Af);
    }

    /// <summary>
    /// Adds two matrices together.
    /// </summary>
    /// <param name="value1">The first source matrix.</param>
    /// <param name="value2">The second source matrix.</param>
    /// <returns>The resulting matrix.</returns>
        public static Matrix5X5 operator +(Matrix5X5 value1, Matrix5X5 value2) =>
        new(
            value1.M11 + value2.M11, value1.M12 + value2.M12, value1.M13 + value2.M13, value1.M14 + value2.M14, value1.M15 + value2.M15,
            value1.M21 + value2.M21, value1.M22 + value2.M22, value1.M23 + value2.M23, value1.M24 + value2.M24, value1.M25 + value2.M25,
            value1.M31 + value2.M31, value1.M32 + value2.M32, value1.M33 + value2.M33, value1.M34 + value2.M34, value1.M35 + value2.M35,
            value1.M41 + value2.M41, value1.M42 + value2.M42, value1.M43 + value2.M43, value1.M44 + value2.M44, value1.M45 + value2.M45,
            value1.M51 + value2.M51, value1.M52 + value2.M52, value1.M53 + value2.M53, value1.M54 + value2.M54, value1.M55 + value2.M55
        );

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are equal; False otherwise.</returns>
    public static bool operator ==(Matrix5X5 value1, Matrix5X5 value2) => value1.M11 == value2.M11 && value1.M22 == value2.M22 && value1.M33 == value2.M33 && value1.M44 == value2.M44 && value1.M55 == value2.M55 && // Check diagonal element first for early out.
           value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M14 == value2.M14 && value1.M15 == value2.M15 &&
           value1.M21 == value2.M21 && value1.M23 == value2.M23 && value1.M24 == value2.M24 && value1.M25 == value2.M25 &&
           value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M34 == value2.M34 && value1.M35 == value2.M35 &&
           value1.M41 == value2.M41 && value1.M42 == value2.M42 && value1.M43 == value2.M43 && value1.M45 == value2.M45 &&
           value1.M51 == value2.M51 && value1.M52 == value2.M52 && value1.M53 == value2.M53 && value1.M54 == value2.M54;

    /// <summary>
    /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
    /// </summary>
    /// <param name="other">The matrix to compare this instance to.</param>
    /// <returns>True if the matrices are equal; False otherwise.</returns>
    public readonly bool Equals(Matrix5X5 other) => this == other;


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
    public readonly override bool Equals(object obj) => obj is Matrix5X5 x5 && Equals(x5);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public readonly override int GetHashCode()
    {
        int hash1 = HashCode.Combine(M11, M12, M13, M14, M15, M21, M22, M23);
        int hash2 = HashCode.Combine(M24, M25, M31, M32, M33, M34, M35, M41);
        int hash3 = HashCode.Combine(M42, M43, M44, M45, M51, M52, M53, M54);
        int hash4 = HashCode.Combine(M55);
        return HashCode.Combine(hash1, hash2, hash3, hash4);
    }

    /// <summary>
    /// Returns a String representing this matrix instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public readonly override string ToString()
    {
        var ci = CultureInfo.CurrentCulture;

        return string.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2} M14:{3} M15:{4}}} {{M21:{5} M22:{6} M23:{7} M24:{8} M25:{9}}} {{M31:{10} M32:{11} M33:{12} M34:{13} M35:{14}}} {{M41:{15} M42:{16} M43:{17} M44:{18} M45:{19}}} {{M51:{20} M52:{21} M53:{22} M54:{23} M55:{24}}} }}",
            M11.ToString(ci), M12.ToString(ci), M13.ToString(ci), M14.ToString(ci), M15.ToString(ci),
            M21.ToString(ci), M22.ToString(ci), M23.ToString(ci), M24.ToString(ci), M25.ToString(ci),
            M31.ToString(ci), M32.ToString(ci), M33.ToString(ci), M34.ToString(ci), M35.ToString(ci),
            M41.ToString(ci), M42.ToString(ci), M43.ToString(ci), M44.ToString(ci), M45.ToString(ci),
            M51.ToString(ci), M52.ToString(ci), M53.ToString(ci), M54.ToString(ci), M55.ToString(ci));
    }
}