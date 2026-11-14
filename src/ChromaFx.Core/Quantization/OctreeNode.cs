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
/// Node in a <see cref="Octree"/>.
/// </summary>
public sealed class OctreeNode
{
    private readonly OctreeNode[] _children;
    private bool _leaf;
    private int _paletteIndex;
    private int _pixelCount;
    private int _red;
    private int _green;
    private int _blue;

    /// <summary>
    /// Initializes a new instance of the <see cref="OctreeNode"/> class.
    /// </summary>
    public OctreeNode(int level, int colorBits, Octree octree)
    {
        _leaf = level == colorBits;
        if (_leaf)
        {
            octree.Leaves++;
            NextReducible = null;
            _children = null;
        }
        else
        {
            NextReducible = octree.ReducibleNodes[level];
            octree.ReducibleNodes[level] = this;
            _children = new OctreeNode[8];
        }
    }

    /// <summary>Gets the next reducible node at this level.</summary>
    public OctreeNode NextReducible { get; }

    /// <summary>Adds a color into this subtree.</summary>
    public void AddColor(Color pixel, int colorBits, int level, Octree octree)
    {
        if (_leaf)
        {
            Increment(pixel);
            octree.TrackPrevious(this);
            return;
        }

        var index = Octree.ChildIndex(pixel, level);
        var child = _children[index];
        if (child == null)
        {
            child = new OctreeNode(level + 1, colorBits, octree);
            _children[index] = child;
        }

        child.AddColor(pixel, colorBits, level + 1, octree);
    }

    /// <summary>Walks leaves to fill <paramref name="palette"/>.</summary>
    public void ConstructPalette(List<Color> palette, ref int index)
    {
        if (_leaf)
        {
            _paletteIndex = index++;
            palette.Add(
                new Color(
                    (_red / _pixelCount).ToByte(),
                    (_green / _pixelCount).ToByte(),
                    (_blue / _pixelCount).ToByte()
                )
            );
            return;
        }

        for (var i = 0; i < 8; i++)
            _children[i]?.ConstructPalette(palette, ref index);
    }

    /// <summary>Returns the palette index for <paramref name="pixel"/>.</summary>
    public int GetPaletteIndex(Color pixel, int level)
    {
        if (!TryGetPaletteIndex(pixel, level, out var index))
            throw new InvalidOperationException("Octree path missing a child for palette lookup.");
        return index;
    }

    /// <summary>Tries to resolve the palette index without throwing on a missing tree path.</summary>
    public bool TryGetPaletteIndex(Color pixel, int level, out int index)
    {
        if (_leaf)
        {
            index = _paletteIndex;
            return true;
        }

        var child = _children[Octree.ChildIndex(pixel, level)];
        if (child == null)
        {
            index = 0;
            return false;
        }

        return child.TryGetPaletteIndex(pixel, level + 1, out index);
    }

    /// <summary>Accumulates <paramref name="pixel"/> into this leaf.</summary>
    public void Increment(Color pixel)
    {
        _pixelCount++;
        _red += pixel.Red;
        _green += pixel.Green;
        _blue += pixel.Blue;
    }

    /// <summary>Collapses children into this node and returns the leaf-count delta.</summary>
    public int Reduce()
    {
        _red = _green = _blue = 0;
        var childNodes = 0;
        for (var index = 0; index < 8; index++)
        {
            var child = _children[index];
            if (child == null)
                continue;
            _red += child._red;
            _green += child._green;
            _blue += child._blue;
            _pixelCount += child._pixelCount;
            ++childNodes;
            _children[index] = null;
        }

        _leaf = true;
        return childNodes - 1;
    }
}
