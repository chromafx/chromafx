﻿using System.Text;
using Xunit;

namespace Structure.Sketching.Tests.Formats.Png.Format;

public class Property
{
    [Fact]
    public void Create()
    {
        var testObject = new Sketching.Formats.Png.Format.Property("TestKey", "TestValue");
        Assert.Equal("TestKey", testObject.Key);
        Assert.Equal("TestValue", testObject.Value);
        testObject = new Sketching.Formats.Png.Format.Property(null, null);
        Assert.Equal("", testObject.Key);
        Assert.Equal("", testObject.Value);
    }

    [Fact]
    public void ReadFromChunk()
    {
        Sketching.Formats.Png.Format.Property testObject = new Sketching.Formats.Png.Format.Helpers.Chunk(10, "Something", Encoding.UTF8.GetBytes("Testing\0THIS"), 1234);
        Assert.Equal("Testing", testObject.Key);
        Assert.Equal("THIS", testObject.Value);
    }
}