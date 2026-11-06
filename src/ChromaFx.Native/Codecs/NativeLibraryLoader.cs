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
using System.Runtime.InteropServices;

namespace ChromaFx.Native.Codecs;

/// <summary>
/// Loads bundled libwebp / libchromaavif / libchromajxl from the app directory or runtimes/&lt;rid&gt;/native.
/// </summary>
internal static class NativeLibraryLoader
{
    private static readonly object Gate = new();
    private static bool _registered;

    internal static void EnsureRegistered()
    {
        if (_registered)
            return;

        lock (Gate)
        {
            if (_registered)
                return;

            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, Resolve);
            _registered = true;
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var fileName = FileName(libraryName);
        if (fileName is null)
            return IntPtr.Zero;

        foreach (var path in Candidates(assembly, fileName))
        {
            if (File.Exists(path) && NativeLibrary.TryLoad(path, out var handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> Candidates(Assembly assembly, string fileName)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in ProbeDirectories(assembly))
        {
            var flat = Path.Combine(dir, fileName);
            if (seen.Add(flat))
                yield return flat;

            foreach (var rid in RuntimeIds())
            {
                var nested = Path.Combine(dir, "runtimes", rid, "native", fileName);
                if (seen.Add(nested))
                    yield return nested;
            }
        }
    }

    private static IEnumerable<string> ProbeDirectories(Assembly assembly)
    {
        yield return AppContext.BaseDirectory;
        var location = assembly.Location;
        if (!string.IsNullOrEmpty(location))
        {
            var dir = Path.GetDirectoryName(location);
            if (!string.IsNullOrEmpty(dir))
                yield return dir;
        }
    }

    private static IEnumerable<string> RuntimeIds()
    {
        var reported = RuntimeInformation.RuntimeIdentifier;
        if (!string.IsNullOrEmpty(reported))
            yield return reported;

        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "arm64",
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => null,
        };
        if (arch is null)
            yield break;

        if (OperatingSystem.IsMacOS())
            yield return "osx-" + arch;
        else if (OperatingSystem.IsLinux())
            yield return "linux-" + arch;
        else if (OperatingSystem.IsWindows())
            yield return "win-" + arch;
    }

    private static string? FileName(string libraryName)
    {
        var stem = libraryName switch
        {
            "webp" => "webp",
            "chromaavif" => "chromaavif",
            "chromajxl" => "chromajxl",
            _ => null,
        };
        if (stem is null)
            return null;

        if (OperatingSystem.IsWindows())
            return stem + ".dll";
        if (OperatingSystem.IsMacOS())
            return "lib" + stem + ".dylib";
        return "lib" + stem + ".so";
    }
}
