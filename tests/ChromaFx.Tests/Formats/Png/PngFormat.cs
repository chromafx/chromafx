using System.Buffers.Binary;
using System.Text;
using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Png;

public class PngFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Png/";

    public override string InputDirectory => "./TestImages/Formats/Png/";

    public override string OutputDirectory => "./TestOutput/Formats/Png/";

    public static readonly TheoryData<string> InputFileNames =
    [
        "splash.png",
        "48bit.png",
        "blur.png",
        "indexed.png",
        "splashbw.png",
    ];

    [Fact]
    public void Capabilities()
    {
        var format = new IO.Formats.Png.PngFormat();
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(format.CanDecode(header));
        Assert.False(format.CanDecode(BitConverter.GetBytes(19777)));
        Assert.True(format.CanDecode("test.png"));
        Assert.False(format.CanDecode("test.jpg"));
        Assert.True(format.CanDecode(new MemoryStream(header)));
        Assert.True(format.CanEncode("ASDF.png"));
        Assert.False(format.CanEncode("ASDF.bmp"));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Png/splash.png");
        var tempImage = new IO.Formats.Png.PngFormat().Decode(tempFile);
        Assert.Equal(241500, tempImage.PixelCount);
        Assert.Equal(500, tempImage.Width);
        Assert.Equal(483, tempImage.Height);
        Assert.Equal(500d / 483d, tempImage.PixelRatio);
    }

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var imageFormat = new IO.Formats.Png.PngFormat();
            var tempImage = imageFormat.Decode(tempFile);
            using var tempFile2 = File.Create(OutputDirectory + fileName);
            imageFormat.Encode(new BinaryWriter(tempFile2), tempImage);
        }

        Assert.True(CheckDecodedPngCorrect(fileName));
    }

    [Fact]
    public void Decode_WithTextChunks_DoesNotThrow()
    {
        var png = new Image(1, 1, [new Color(10, 20, 30)]).ToByteArray(FileFormats.Png);
        png = InsertChunkBeforeIend(png, "tEXt", BuildTextPayload("Comment", "hello"));
        png = InsertChunkBeforeIend(png, "iTXt", BuildInternationalTextPayload("Author", "ChromaFx"));

        var image = Image.Load(png);
        Assert.Equal(1, image.Width);
        Assert.Equal(1, image.Height);
        Assert.Equal(new Color(10, 20, 30), image[0, 0]);
    }

    private static byte[] BuildTextPayload(string key, string value)
    {
        var keyBytes = Encoding.Latin1.GetBytes(key);
        var valueBytes = Encoding.Latin1.GetBytes(value);
        var payload = new byte[keyBytes.Length + 1 + valueBytes.Length];
        keyBytes.CopyTo(payload, 0);
        payload[keyBytes.Length] = 0;
        valueBytes.CopyTo(payload, keyBytes.Length + 1);
        return payload;
    }

    private static byte[] BuildInternationalTextPayload(string key, string value)
    {
        // keyword\0 compressionFlag compressionMethod language\0 translated\0 text
        var keyBytes = Encoding.Latin1.GetBytes(key);
        var textBytes = Encoding.UTF8.GetBytes(value);
        var payload = new byte[keyBytes.Length + 1 + 2 + 1 + 1 + textBytes.Length];
        var o = 0;
        keyBytes.CopyTo(payload, o);
        o += keyBytes.Length;
        payload[o++] = 0;
        payload[o++] = 0; // uncompressed
        payload[o++] = 0; // compression method
        payload[o++] = 0; // empty language
        payload[o++] = 0; // empty translated keyword
        textBytes.CopyTo(payload, o);
        return payload;
    }

    private static byte[] InsertChunkBeforeIend(byte[] png, string type, byte[] payload)
    {
        var iend = FindChunkOffset(png, "IEND");
        Assert.True(iend >= 0);

        var chunk = new byte[12 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(0, 4), (uint)payload.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(chunk, 4);
        payload.CopyTo(chunk, 8);
        var crc = Crc32(chunk.AsSpan(4, 4 + payload.Length));
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8 + payload.Length, 4), crc);

        var result = new byte[png.Length + chunk.Length];
        png.AsSpan(0, iend).CopyTo(result);
        chunk.CopyTo(result, iend);
        png.AsSpan(iend).CopyTo(result.AsSpan(iend + chunk.Length));
        return result;
    }

    private static int FindChunkOffset(byte[] png, string type)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        for (var i = 8; i + 8 <= png.Length; )
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(i, 4));
            if (png.AsSpan(i + 4, 4).SequenceEqual(typeBytes))
                return i;
            i += 12 + (int)length;
        }

        return -1;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }

        return ~crc;
    }
}
