/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

using ChromaFx.Core.Colors;

namespace ChromaFx.Core.Quantization;

/// <summary>
/// Octree palette builder operating on <see cref="Color"/> (shared by GIF encode and Processing Quantize).
/// </summary>
public sealed class Octree
{
    private static readonly int[] Mask = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// Initializes a new instance of the <see cref="Octree"/> class.
    /// </summary>
    /// <param name="maxColorBits">Significant color bits (tree depth).</param>
    public Octree(int maxColorBits)
    {
        MaxColorBits = maxColorBits;
        Leaves = 0;
        ReducibleNodes = new OctreeNode[9];
        Root = new OctreeNode(0, MaxColorBits, this);
        PreviousPacked = -1;
        PreviousNode = null;
    }

    /// <summary>Gets or sets the leaf count.</summary>
    public int Leaves { get; set; }

    /// <summary>Gets the maximum color bits.</summary>
    public int MaxColorBits { get; }

    /// <summary>Gets the reducible-node lists by level.</summary>
    public OctreeNode[] ReducibleNodes { get; }

    /// <summary>Gets the root node.</summary>
    public OctreeNode Root { get; }

    /// <summary>Gets the previous packed RGB for the add-color cache.</summary>
    public int PreviousPacked { get; private set; }

    /// <summary>Gets the previous leaf for the add-color cache.</summary>
    public OctreeNode PreviousNode { get; private set; }

    /// <summary>Adds a color into the tree (RGB only; alpha ignored).</summary>
    public void AddColor(Color pixel)
    {
        var packed = Pack(pixel);
        if (PreviousPacked == packed && PreviousNode != null)
        {
            PreviousNode.Increment(pixel);
            return;
        }

        PreviousPacked = packed;
        Root.AddColor(pixel, MaxColorBits, 0, this);
    }

    /// <summary>Returns the palette index for <paramref name="pixel"/> after <see cref="Palettize"/>.</summary>
    public int GetPaletteIndex(Color pixel) => Root.GetPaletteIndex(pixel, 0);

    /// <summary>Tries to resolve the palette index without throwing on a missing tree path.</summary>
    public bool TryGetPaletteIndex(Color pixel, out int index) =>
        Root.TryGetPaletteIndex(pixel, 0, out index);

    /// <summary>Reduces the tree to at most <paramref name="colorCount"/> leaves and builds the palette.</summary>
    public List<Color> Palettize(int colorCount)
    {
        while (Leaves > colorCount)
            Reduce();

        var palette = new List<Color>(Leaves);
        var paletteIndex = 0;
        Root.ConstructPalette(palette, ref paletteIndex);
        return palette;
    }

    /// <summary>Tracks the previous leaf for the add-color cache.</summary>
    public void TrackPrevious(OctreeNode node) => PreviousNode = node;

    private void Reduce()
    {
        var index = MaxColorBits - 1;
        while (index > 0 && ReducibleNodes[index] == null)
            index--;

        var node = ReducibleNodes[index];
        ReducibleNodes[index] = node.NextReducible;
        Leaves -= node.Reduce();
        PreviousNode = null;
    }


    /// <summary>Bits of color depth needed for up to <paramref name="colorCount"/> colors.</summary>
    public static int BitsNeeded(int colorCount) =>
        (int)Math.Ceiling(Math.Log(Math.Max(colorCount, 1), 2));

    internal static int ChildIndex(Color pixel, int level)
    {
        var shift = 7 - level;
        return ((pixel.Red & Mask[level]) >> (shift - 2))
            | ((pixel.Green & Mask[level]) >> (shift - 1))
            | ((pixel.Blue & Mask[level]) >> shift);
    }

    private static int Pack(Color c) => (c.Red << 16) | (c.Green << 8) | c.Blue;
}
