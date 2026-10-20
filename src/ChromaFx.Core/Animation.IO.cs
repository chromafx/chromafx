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

using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChromaFx.Core;

public partial class Animation
{
    private static Func<string, Animation> _loadFromPath;
    private static Func<Stream, Animation> _loadFromStream;

    /// <summary>
    /// Registers animation loaders. Called automatically when ChromaFx.IO is referenced.
    /// </summary>
    internal static void RegisterLoaders(
        Func<string, Animation> fromPath,
        Func<Stream, Animation> fromStream
    )
    {
        _loadFromPath = fromPath;
        _loadFromStream = fromStream;
    }

    /// <summary>
    /// Loads an animation from a file path.
    /// </summary>
    public static Animation Load(string fileName) => GetLoader(ref _loadFromPath).Invoke(fileName);

    /// <summary>
    /// Loads an animation from a stream.
    /// </summary>
    public static Animation Load(Stream stream) => GetLoader(ref _loadFromStream).Invoke(stream);

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
