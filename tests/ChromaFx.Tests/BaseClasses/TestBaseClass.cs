namespace ChromaFx.Tests.BaseClasses;

public abstract class TestBaseClass
{
    private const int BmpHeaderSize = 54;
    private const int MaxRotatePixelDifferences = 500;

    protected TestBaseClass()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(ExpectedDirectory);
    }

    public abstract string ExpectedDirectory { get; }
    public abstract string OutputDirectory { get; }

    protected bool CheckFileCorrect(string expectedFilePath, string outputFilePath)
    {
        var expected = ReadBinary(File.OpenRead(expectedFilePath));
        var actual = ReadBinary(File.OpenRead(outputFilePath));
        if (expected.SequenceEqual(actual))
            return true;

        var fileName = Path.GetFileName(expectedFilePath);
        if (fileName.Contains("Rotate", StringComparison.OrdinalIgnoreCase))
            return CompareBitmapPixels(expected, actual, MaxRotatePixelDifferences);

        return false;
    }

    private static bool CompareBitmapPixels(
        byte[] expected,
        byte[] actual,
        int maxDifferentPixels
    )
    {
        if (expected.Length != actual.Length || expected.Length <= BmpHeaderSize)
            return false;

        if (!expected.AsSpan(0, BmpHeaderSize).SequenceEqual(actual.AsSpan(0, BmpHeaderSize)))
            return false;

        var differentPixels = 0;
        for (var offset = BmpHeaderSize; offset + 2 < expected.Length; offset += 3)
        {
            if (
                expected[offset] == actual[offset]
                && expected[offset + 1] == actual[offset + 1]
                && expected[offset + 2] == actual[offset + 2]
            )
            {
                continue;
            }

            differentPixels++;
            if (differentPixels > maxDifferentPixels)
                return false;
        }

        return true;
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
