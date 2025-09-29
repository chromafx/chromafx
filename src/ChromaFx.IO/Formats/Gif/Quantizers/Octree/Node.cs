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

using ChromaFx.Core.Colors.ColorSpaces;
using ChromaFx.Core.ExtensionMethods;

namespace ChromaFx.IO.Formats.Gif.Quantizers.Octree;

/// <summary>
/// Node class
/// </summary>
public class Node
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="level">
    /// The level in the tree = 0 - 7
    /// </param>
    /// <param name="colorBits">
    /// The number of significant color bits in the image
    /// </param>
    /// <param name="octree">
    /// The tree to which this node belongs
    /// </param>
    public Node(int level, int colorBits, Octree octree)
    {
        // Construct the new node
        _leaf = level == colorBits;

        _red = _green = _blue = 0;
        _pixelCount = 0;

        // If a leaf, increment the leaf count
        if (_leaf)
        {
            octree.Leaves++;
            NextReducible = null;
            _children = null;
        }
        else
        {
            // Otherwise add this to the reducible nodes
            NextReducible = octree.ReducibleNodes[level];
            octree.ReducibleNodes[level] = this;
            _children = new Node[8];
        }
    }

    /// <summary>
    /// Gets the next reducible node
    /// </summary>
    public Node NextReducible { get; }

    private static readonly int[] Mask = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// Pointers to any child nodes
    /// </summary>
    private readonly Node[] _children;

    /// <summary>
    /// Blue component
    /// </summary>
    private int _blue;

    /// <summary>
    /// Green Component
    /// </summary>
    private int _green;

    /// <summary>
    /// Flag indicating that this is a leaf node
    /// </summary>
    private bool _leaf;

    /// <summary>
    /// The index of this node in the palette
    /// </summary>
    private int _paletteIndex;

    /// <summary>
    /// Number of pixels in this node
    /// </summary>
    private int _pixelCount;

    /// <summary>
    /// Red component
    /// </summary>
    private int _red;

    /// <summary>
    /// Add a color into the tree
    /// </summary>
    /// <param name="pixel">
    /// The color
    /// </param>
    /// <param name="colorBits">
    /// The number of significant color bits
    /// </param>
    /// <param name="level">
    /// The level in the tree
    /// </param>
    /// <param name="octree">
    /// The tree to which this node belongs
    /// </param>
    public void AddColor(Bgra pixel, int colorBits, int level, Octree octree)
    {
        // Update the color information if this is a leaf
        if (_leaf)
        {
            Increment(pixel);

            // Setup the previous node
            octree.TrackPrevious(this);
        }
        else
        {
            // Go to the next level down in the tree
            var shift = 7 - level;
            var index =
                ((pixel.Red & Mask[level]) >> (shift - 2))
                | ((pixel.Green & Mask[level]) >> (shift - 1))
                | ((pixel.Blue & Mask[level]) >> shift);

            var child = _children[index];

            if (child == null)
            {
                // Create a new child node and store it in the array
                child = new Node(level + 1, colorBits, octree);
                _children[index] = child;
            }

            // Add the color to the child node
            child.AddColor(pixel, colorBits, level + 1, octree);
        }
    }

    /// <summary>
    /// Traverse the tree, building up the color palette
    /// </summary>
    /// <param name="palette">
    /// The palette
    /// </param>
    /// <param name="index">
    /// The current palette index
    /// </param>
    public void ConstructPalette(List<Bgra> palette, ref int index)
    {
        if (_leaf)
        {
            // Consume the next palette index
            _paletteIndex = index++;

            var r = (_red / _pixelCount).ToByte();
            var g = (_green / _pixelCount).ToByte();
            var b = (_blue / _pixelCount).ToByte();

            // And set the color of the palette entry
            palette.Add(new Bgra(b, g, r));
        }
        else
        {
            // Loop through children looking for leaves
            for (var i = 0; i < 8; i++)
            {
                _children[i]?.ConstructPalette(palette, ref index);
            }
        }
    }

    /// <summary>
    /// Return the palette index for the passed color
    /// </summary>
    /// <param name="pixel">
    /// The <see cref="Bgra"/> representing the pixel.
    /// </param>
    /// <param name="level">
    /// The level.
    /// </param>
    /// <returns>
    /// The <see cref="int"/> representing the index of the pixel in the palette.
    /// </returns>
    public int GetPaletteIndex(Bgra pixel, int level)
    {
        var index = _paletteIndex;

        if (_leaf) return index;
        var shift = 7 - level;
        var pixelIndex =
            ((pixel.Red & Mask[level]) >> (shift - 2))
            | ((pixel.Green & Mask[level]) >> (shift - 1))
            | ((pixel.Blue & Mask[level]) >> shift);

        if (_children[pixelIndex] != null)
        {
            index = _children[pixelIndex].GetPaletteIndex(pixel, level + 1);
        }
        else
        {
            throw new Exception("Didn't expect this!");
        }

        return index;
    }

    /// <summary>
    /// Increment the pixel count and add to the color information
    /// </summary>
    /// <param name="pixel">
    /// The pixel to add.
    /// </param>
    public void Increment(Bgra pixel)
    {
        _pixelCount++;
        _red += pixel.Red;
        _green += pixel.Green;
        _blue += pixel.Blue;
    }

    /// <summary>
    /// Reduce this node by removing all of its children
    /// </summary>
    /// <returns>The number of leaves removed</returns>
    public int Reduce()
    {
        _red = _green = _blue = 0;
        var childNodes = 0;

        // Loop through all children and add their information to this node
        for (var index = 0; index < 8; index++)
        {
            if (_children[index] == null) continue;
            _red += _children[index]._red;
            _green += _children[index]._green;
            _blue += _children[index]._blue;
            _pixelCount += _children[index]._pixelCount;
            ++childNodes;
            _children[index] = null;
        }

        // Now change this to a leaf node
        _leaf = true;

        // Return the number of nodes to decrement the leaf count by
        return childNodes - 1;
    }
}