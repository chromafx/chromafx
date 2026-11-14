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

namespace ChromaFx.Processing.Filters.Overlays;

/// <summary>
/// Photoshop-style blend modes for <see cref="Blend"/>.
/// </summary>
public enum BlendMode
{
    /// <summary>
    /// Standard alpha composite (existing <see cref="Blend"/> behavior).
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Multiply channels (darken).
    /// </summary>
    Multiply,

    /// <summary>
    /// Screen channels (lighten).
    /// </summary>
    Screen,

    /// <summary>
    /// Overlay (multiply or screen depending on base).
    /// </summary>
    Overlay,

    /// <summary>
    /// Soft light (gentler contrast).
    /// </summary>
    SoftLight,
}
