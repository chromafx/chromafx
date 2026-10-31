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

using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChromaFx.Core;

public partial class Animation
{
    private static Func<string, Animation> _loadFromPath;
    private static Func<Stream, Animation> _loadFromStream;
    private static Func<ReadOnlyMemory<byte>, Animation> _loadFromMemory;

    /// <summary>
    /// Registers animation loaders. Called automatically when ChromaFx.IO is referenced.
    /// </summary>
    internal static void RegisterLoaders(
        Func<string, Animation> fromPath,
        Func<Stream, Animation> fromStream,
        Func<ReadOnlyMemory<byte>, Animation> fromMemory = null
    )
    {
        _loadFromPath = fromPath;
        _loadFromStream = fromStream;
        _loadFromMemory = fromMemory;
    }

    /// <summary>
    /// Loads an animation from a file path.
    /// </summary>
    public static Animation Load(string fileName) => GetLoader(ref _loadFromPath).Invoke(fileName);

    /// <summary>
    /// Loads an animation from a stream.
    /// </summary>
    public static Animation Load(Stream stream) => GetLoader(ref _loadFromStream).Invoke(stream);

    /// <summary>
    /// Loads an animation from encoded bytes. Prefers a zero-copy path when the memory
    /// already references a managed array.
    /// </summary>
    public static Animation Load(ReadOnlyMemory<byte> data) => GetMemoryLoader().Invoke(data);

    /// <summary>
    /// Loads an animation from an encoded byte array (zero-copy into the decoder stream).
    /// </summary>
    public static Animation Load(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Load(data.AsMemory());
    }

    /// <summary>
    /// Loads an animation from encoded bytes in a span. Uses a pooled buffer (one copy);
    /// prefer <see cref="Load(ReadOnlyMemory{byte})"/> or <see cref="Load(byte[])"/> when
    /// you already own heap memory.
    /// </summary>
    public static Animation Load(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return GetMemoryLoader().Invoke(ReadOnlyMemory<byte>.Empty);

        var rented = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            data.CopyTo(rented.AsSpan(0, data.Length));
            return GetMemoryLoader().Invoke(rented.AsMemory(0, data.Length));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Asynchronously loads an animation from a file path (async read, sync decode).
    /// </summary>
    public static async Task<Animation> LoadAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var bytes = await File.ReadAllBytesAsync(fileName, cancellationToken).ConfigureAwait(false);
        return Load(bytes.AsMemory());
    }

    /// <summary>
    /// Asynchronously loads an animation from a stream (async buffer, sync decode).
    /// </summary>
    public static async Task<Animation> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = (int)(memoryStream.Length - memoryStream.Position);
            var loaded = Load(segment.AsMemory((int)memoryStream.Position, length));
            memoryStream.Position = memoryStream.Length;
            return loaded;
        }

        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (buffer.TryGetBuffer(out var buffered))
            return Load(buffered.AsMemory(0, (int)buffer.Length));
        return Load(buffer.ToArray().AsMemory());
    }

    private static Func<ReadOnlyMemory<byte>, Animation> GetMemoryLoader()
    {
        if (_loadFromMemory != null)
            return _loadFromMemory;

        TryInitializeIo();
        if (_loadFromMemory != null)
            return _loadFromMemory;

        return static data =>
        {
            if (MemoryMarshal.TryGetArray(data, out var segment) && segment.Array is not null)
            {
                using var stream = new MemoryStream(
                    segment.Array,
                    segment.Offset,
                    segment.Count,
                    writable: false
                );
                return GetLoader(ref _loadFromStream).Invoke(stream);
            }

            using var copy = new MemoryStream(data.ToArray(), writable: false);
            return GetLoader(ref _loadFromStream).Invoke(copy);
        };
    }

    private static Func<T, Animation> GetLoader<T>(ref Func<T, Animation> loader)
    {
        if (loader != null)
            return loader;

        TryInitializeIo();
        if (loader != null)
            return loader;

        throw new InvalidOperationException(
            "Animation loading is not available. Reference the ChromaFx or ChromaFx.IO package."
        );
    }

    private static void TryInitializeIo()
    {
        if (_loadFromPath != null)
            return;

        var ioAssembly = AppDomain
            .CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "ChromaFx.IO");

        try
        {
            ioAssembly ??= Assembly.Load(new AssemblyName("ChromaFx.IO"));
        }
        catch (FileNotFoundException)
        {
            return;
        }
        catch (FileLoadException)
        {
            return;
        }

        var type = ioAssembly.GetType("ChromaFx.IO.ImageFormatsBootstrap");
        if (type != null)
            RuntimeHelpers.RunModuleConstructor(type.Module.ModuleHandle);
    }
}
