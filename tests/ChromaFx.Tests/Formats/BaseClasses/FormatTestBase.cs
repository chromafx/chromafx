namespace ChromaFx.Tests.Formats.BaseClasses;

public abstract class FormatTestBase
{
    protected FormatTestBase()
    {
        new DirectoryInfo(OutputDirectory).Create();
    }

    public abstract string ExpectedDirectory { get; }
    public abstract string InputDirectory { get; }
    public abstract string OutputDirectory { get; }

    protected bool CheckFileCorrect(string fileName)
    {
        using var outputStream = File.OpenRead(OutputDirectory + fileName);
        using var expectedStream = File.OpenRead(ExpectedDirectory + fileName);
        return ReadBinary(outputStream).SequenceEqual(ReadBinary(expectedStream));
    }

    protected bool CheckDecodedPngCorrect(string fileName)
    {
        using var expectedStream = File.OpenRead(ExpectedDirectory + fileName);
        using var outputStream = File.OpenRead(OutputDirectory + fileName);

        var imageFormat = new ChromaFx.Formats.Png.PngFormat();
        var expectedImage = imageFormat.Decode(expectedStream);
        var outputImage = imageFormat.Decode(outputStream);

        var dimensionsOk =
            outputImage.Width == expectedImage.Width && outputImage.Height == expectedImage.Height;
        if (!dimensionsOk)
            return false;

        var pixelsOk = true;
        for (var index = 0; index < outputImage.Pixels.Length; index++)
        {
            if (!outputImage.Pixels[index].Equals(expectedImage.Pixels[index]))
                pixelsOk = false;
        }

        return pixelsOk;
    }

    protected static byte[] ReadBinary(FileStream stream)
    {
        var buffer = new byte[1024];
        using MemoryStream temp = new();
        while (true)
        {
            var count = stream.Read(buffer, 0, buffer.Length);
            if (count <= 0)
                return temp.ToArray();
            temp.Write(buffer, 0, count);
        }
    }
}
