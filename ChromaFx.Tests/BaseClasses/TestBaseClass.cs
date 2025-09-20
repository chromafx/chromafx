namespace ChromaFx.Tests.BaseClasses;

public abstract class TestBaseClass
{
    protected TestBaseClass()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(ExpectedDirectory);
    }

    public abstract string ExpectedDirectory { get; }
    public abstract string OutputDirectory { get; }

    protected bool CheckFileCorrect(string expectedFilePath, string outputFilePath)
    {
        using var outputStream = File.OpenRead(outputFilePath);
        using var expectedStream = File.OpenRead(expectedFilePath);
        return ReadBinary(outputStream).SequenceEqual(ReadBinary(expectedStream));
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