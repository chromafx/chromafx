/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.ExtensionMethods;
using ChromaFx.Colors;

namespace ChromaFx.Procedural;

/// <summary>
/// Perlin noise helper class
/// </summary>
public static class PerlinNoise
{
    /// <summary>
    /// Generates perlin noise
    /// </summary>
    /// <param name="width">Width of the resulting image</param>
    /// <param name="height">Height of the resulting image</param>
    /// <param name="maxRgbValue">MaxRGBValue</param>
    /// <param name="minRgbValue">MinRGBValue</param>
    /// <param name="frequency">Frequency</param>
    /// <param name="amplitude">Amplitude</param>
    /// <param name="persistence">Persistence</param>
    /// <param name="octaves">Octaves</param>
    /// <param name="seed">Random seed</param>
    /// <returns>An image containing perlin noise</returns>
    public static Image Generate(
        int width,
        int height,
        float maxRgbValue,
        float minRgbValue,
        float frequency,
        float amplitude,
        float persistence,
        int octaves,
        int seed
    )
    {
        var returnValue = new Image(width, height, new Color[width * height]);
        var noise = GenerateNoise(seed, width, height);
        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                var value = GetValue(
                    x,
                    y,
                    width,
                    height,
                    frequency,
                    amplitude,
                    persistence,
                    octaves,
                    noise
                );
                value = value * 0.5f + 0.5f;
                value *= 255;
                var rgbValue = (byte)value.Clamp(minRgbValue, maxRgbValue);
                returnValue.Pixels[y * width + x].Red = rgbValue;
                returnValue.Pixels[y * width + x].Green = rgbValue;
                returnValue.Pixels[y * width + x].Blue = rgbValue;
                returnValue.Pixels[y * width + x].Alpha = 255;
            }
        }

        return returnValue;
    }

    private static float[,] GenerateNoise(int seed, int width, int height)
    {
        var noise = new float[width, height];
        var randomGenerator = new System.Random(seed);
        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                noise[x, y] = ((float)randomGenerator.NextDouble() - 0.5f) * 2.0f;
            }
        }

        return noise;
    }

    private static float GetSmoothNoise(float x, float y, int width, int height, float[,] noise)
    {
        if (noise == null)
            return 0.0f;
        var fractionX = x - (int)x;
        var fractionY = y - (int)y;
        var x1 = ((int)x + width) % width;
        var y1 = ((int)y + height) % height;
        var x2 = ((int)x + width - 1) % width;
        var y2 = ((int)y + height - 1) % height;

        var finalValue = 0.0f;
        finalValue += fractionX * fractionY * noise[x1, y1];
        finalValue += fractionX * (1 - fractionY) * noise[x1, y2];
        finalValue += (1 - fractionX) * fractionY * noise[x2, y1];
        finalValue += (1 - fractionX) * (1 - fractionY) * noise[x2, y2];

        return finalValue;
    }

    private static float GetValue(
        int x,
        int y,
        int width,
        int height,
        float frequency,
        float amplitude,
        float persistence,
        int octaves,
        float[,] noise
    )
    {
        if (noise == null)
            return 0.0f;
        var finalValue = 0.0f;
        for (var i = 0; i < octaves; ++i)
        {
            finalValue +=
                GetSmoothNoise(x * frequency, y * frequency, width, height, noise) * amplitude;
            frequency *= 2.0f;
            amplitude *= persistence;
        }

        finalValue = finalValue.Clamp(-1.0f, 1.0f);
        return finalValue;
    }
}
