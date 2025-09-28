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

using ChromaFx.Processing.Filters.Convolution.BaseClasses;
using ChromaFx.Processing.Filters.Pipelines.BaseClasses;

namespace ChromaFx.Processing.Filters.Convolution;

/// <summary>
/// Scharr edge detection
/// </summary>
/// <seealso cref="ProcessingPipelineBaseClass"/>
public class Scharr : Convolution2DBaseClass
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="ConvolutionBaseClass"/> is absolute.
    /// </summary>
    /// <value><c>true</c> if absolute; otherwise, <c>false</c>.</value>
    public override bool Absolute => false;

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public override int Height => 3;

    /// <summary>
    /// Gets the offset.
    /// </summary>
    /// <value>The offset.</value>
    public override float Offset => 0;

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public override int Width => 3;

    /// <summary>
    /// Gets the x matrix.
    /// </summary>
    /// <value>The x matrix.</value>
    public override float[] XMatrix =>
    [
        -3, 0, 3,
        -10, 0, 10,
        -3, 0, 3
    ];

    /// <summary>
    /// Gets the y matrix.
    /// </summary>
    /// <value>The y matrix.</value>
    public override float[] YMatrix =>
    [
        3, 10, 3,
        0, 0, 0,
        -3, -10, -3
    ];
}