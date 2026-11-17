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

using ChromaFx.Core;
using Xunit;

namespace ChromaFx.Tests;

public class ImageSimilarityTests
{
    [Fact]
    public void Mse_IdenticalImages_IsZero()
    {
        var a = Solid(2, 2, new Color(10, 20, 30, 255));
        var b = Solid(2, 2, new Color(10, 20, 30, 200)); // alpha ignored
        Assert.Equal(0.0, ImageSimilarity.Mse(a, b));
        Assert.Equal(0.0, ImageSimilarity.Rmse(a, b));
        Assert.Equal(0.0, ImageSimilarity.Mae(a, b));
        Assert.Equal(double.PositiveInfinity, ImageSimilarity.Psnr(a, b));
        Assert.Equal(0, ImageSimilarity.CountDifferingPixels(a, b));
        Assert.Equal(1.0, ImageSimilarity.Ssim(a, b), 5);
    }

    [Fact]
    public void Mse_KnownDifference()
    {
        var a = new Image(1, 1, [new Color(0, 0, 0, 255)]);
        var b = new Image(1, 1, [new Color(10, 0, 0, 255)]);
        // One channel delta 10 → MSE = 100/3
        Assert.Equal(100.0 / 3.0, ImageSimilarity.Mse(a, b), 10);
        Assert.Equal(10.0 / 3.0, ImageSimilarity.Mae(a, b), 10);
        Assert.Equal(1, ImageSimilarity.CountDifferingPixels(a, b));
    }

    [Fact]
    public void CreateDiffImage_AbsRgb()
    {
        var a = new Image(1, 1, [new Color(50, 10, 0, 128)]);
        var b = new Image(1, 1, [new Color(20, 40, 5, 255)]);
        var diff = ImageSimilarity.CreateDiffImage(a, b);
        Assert.Equal(new Color(30, 30, 5, 255), diff[0, 0]);
    }

    [Fact]
    public void MismatchSize_Throws()
    {
        var a = Solid(2, 1, new Color(0, 0, 0, 255));
        var b = Solid(1, 2, new Color(0, 0, 0, 255));
        Assert.Throws<ArgumentException>(() => ImageSimilarity.Mse(a, b));
        Assert.Throws<ArgumentException>(() => ImageSimilarity.Ssim(a, b));
        Assert.Throws<ArgumentException>(() => ImageSimilarity.CreateDiffImage(a, b));
    }

    [Fact]
    public void Null_Throws()
    {
        var a = Solid(1, 1, new Color(0, 0, 0, 255));
        Assert.Throws<ArgumentNullException>(() => ImageSimilarity.Psnr(null!, a));
        Assert.Throws<ArgumentNullException>(() => ImageSimilarity.Psnr(a, null!));
    }

    [Fact]
    public void Ssim_DifferentImages_BelowOne()
    {
        var a = Solid(16, 16, new Color(0, 0, 0, 255));
        var b = Solid(16, 16, new Color(255, 255, 255, 255));
        Assert.True(ImageSimilarity.Ssim(a, b) < 0.1);
    }

    private static Image Solid(int w, int h, Color color)
    {
        var data = new Color[w * h];
        Array.Fill(data, color);
        return new Image(w, h, data);
    }
}
