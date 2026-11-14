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

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Classic auto white-balance estimators used by <see cref="AutoWhiteBalance"/>.
/// </summary>
public enum AutoWhiteBalanceMethod
{
    /// <summary>
    /// Scale channels so the mean RGB is neutral gray.
    /// </summary>
    GrayWorld,

    /// <summary>
    /// Scale channels so a bright percentile approaches white (white-patch / Retinex-style).
    /// </summary>
    WhitePatch,
}
