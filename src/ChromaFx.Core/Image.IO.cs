/*
 * Copyright 2025 Ho Tzin Mein
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

using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChromaFx.Core;

public partial class Image
{
    private static Func<string, Image> _loadFromPath;
    private static Func<Stream, Image> _loadFromStream;

    /// <summary>
    /// Registers image loaders. Called automatically when ChromaFx.IO is referenced.
    /// </summary>
    public static void RegisterLoaders(Func<string, Image> fromPath, Func<Stream, Image> fromStream)
    {
        _loadFromPath = fromPath;
        _loadFromStream = fromStream;
    }

    /// <summary>
    /// Loads an image from a file path.
    /// </summary>
    /// <param name="fileName">The file path.</param>
    /// <returns>The decoded image.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no loader is registered. Reference ChromaFx or ChromaFx.IO.
    /// </exception>
    public static Image Load(string fileName) => GetLoader(ref _loadFromPath).Invoke(fileName);

    /// <summary>
    /// Loads an image from a stream.
    /// </summary>
    /// <param name="stream">The stream containing image data.</param>
    /// <returns>The decoded image.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no loader is registered. Reference ChromaFx or ChromaFx.IO.
    /// </exception>
    public static Image Load(Stream stream) => GetLoader(ref _loadFromStream).Invoke(stream);

    private static Func<T, Image> GetLoader<T>(ref Func<T, Image> loader)
    {
        if (loader != null)
            return loader;

        TryInitializeIo();
        if (loader != null)
            return loader;

        throw CreateLoaderNotRegisteredException();
    }

    private static void TryInitializeIo()
    {
        if (_loadFromPath != null)
            return;

        var ioAssembly = AppDomain.CurrentDomain.GetAssemblies()
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

        var extensionsType = ioAssembly.GetType("ChromaFx.IO.ImageIOExtensions");
        if (extensionsType != null)
            RuntimeHelpers.RunClassConstructor(extensionsType.TypeHandle);
    }

    private static InvalidOperationException CreateLoaderNotRegisteredException() =>
        new(
            "Image loading is not available. Reference the ChromaFx or ChromaFx.IO package."
        );
}
