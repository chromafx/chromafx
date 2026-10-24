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

using System.Collections;

namespace ChromaFx.Core;

/// <summary>
/// A series of images used in an animation.
/// </summary>
/// <seealso cref="IList{Image}" />
public partial class Animation : IList<Image>
{
    private readonly List<Image> _frames;
    private readonly List<short> _frameDelays;
    private short _defaultDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    public Animation()
        : this([], (short)0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class with a uniform frame delay.
    /// </summary>
    /// <param name="images">The images.</param>
    /// <param name="delay">Delay applied to every frame (and used for frames added later).</param>
    public Animation(IEnumerable<Image> images, short delay)
    {
        ArgumentNullException.ThrowIfNull(images);
        _frames = images.ToList();
        _defaultDelay = delay;
        _frameDelays = new List<short>(_frames.Count);
        for (var i = 0; i < _frames.Count; i++)
            _frameDelays.Add(delay);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class with per-frame delays.
    /// </summary>
    /// <param name="images">The images.</param>
    /// <param name="delays">Per-frame delays; must match <paramref name="images"/> count.</param>
    public Animation(IEnumerable<Image> images, IEnumerable<short> delays)
    {
        ArgumentNullException.ThrowIfNull(images);
        ArgumentNullException.ThrowIfNull(delays);
        _frames = images.ToList();
        _frameDelays = delays.ToList();
        if (_frameDelays.Count != _frames.Count)
        {
            throw new ArgumentException(
                "Frame delay count must match image count.",
                nameof(delays)
            );
        }

        _defaultDelay = _frameDelays.Count > 0 ? _frameDelays[0] : (short)0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class by copying another animation.
    /// </summary>
    /// <param name="animation">The animation.</param>
    public Animation(Animation animation)
        : this(
            animation ?? throw new ArgumentNullException(nameof(animation)),
            animation._frameDelays
        )
    {
        RepeatCount = animation.RepeatCount;
    }

    /// <inheritdoc />
    public int Count => _frames.Count;

    /// <summary>
    /// Gets or sets a uniform delay applied to all frames.
    /// Getting returns the first frame's delay (or the default when empty).
    /// Setting applies the value to every frame and the default for future adds.
    /// </summary>
    public short Delay
    {
        get => _frameDelays.Count > 0 ? _frameDelays[0] : _defaultDelay;
        set => SetAllFrameDelays(value);
    }

    /// <summary>
    /// Gets per-frame delays (same order and length as frames).
    /// </summary>
    public IReadOnlyList<short> FrameDelays => _frameDelays;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the repeat count. (0 means loop indefinitely, otherwise loop N+1 times)
    /// </summary>
    public ushort RepeatCount { get; set; }

    /// <inheritdoc />
    public Image this[int index]
    {
        get => _frames[index];
        set => _frames[index] = value;
    }

    /// <summary>
    /// Gets the delay for the frame at <paramref name="index"/>.
    /// </summary>
    public short GetFrameDelay(int index) => _frameDelays[index];

    /// <summary>
    /// Sets the delay for the frame at <paramref name="index"/>.
    /// </summary>
    public void SetFrameDelay(int index, short delay) => _frameDelays[index] = delay;

    /// <summary>
    /// Sets every frame's delay (and the default for frames added later).
    /// </summary>
    public void SetAllFrameDelays(short delay)
    {
        _defaultDelay = delay;
        for (var i = 0; i < _frameDelays.Count; i++)
            _frameDelays[i] = delay;
    }

    /// <inheritdoc />
    public void Add(Image item)
    {
        _frames.Add(item);
        _frameDelays.Add(_defaultDelay);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _frames.Clear();
        _frameDelays.Clear();
    }

    /// <inheritdoc />
    public bool Contains(Image item) => _frames.Contains(item);

    /// <inheritdoc />
    public void CopyTo(Image[] array, int arrayIndex) => _frames.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<Image> GetEnumerator() => _frames.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _frames.GetEnumerator();

    /// <inheritdoc />
    public int IndexOf(Image item) => _frames.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, Image item)
    {
        _frames.Insert(index, item);
        _frameDelays.Insert(index, _defaultDelay);
    }

    /// <inheritdoc />
    public bool Remove(Image item)
    {
        var index = _frames.IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        _frames.RemoveAt(index);
        _frameDelays.RemoveAt(index);
    }
}
