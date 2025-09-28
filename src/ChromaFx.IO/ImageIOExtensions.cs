using ChromaFx.Core;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO;

public static class ImageIOExtensions
{
    public static bool Save(this Image image, string fileName)
        => new Manager().Encode(fileName, image);

    public static bool Save(this Image image, Stream stream, FileFormats format)
        => new Manager().Encode(stream, image, format);

    public static string ToBase64String(this Image image, FileFormats desiredFormat)
    {
        using var stream = new MemoryStream();
        if (!image.Save(stream, desiredFormat))
            return string.Empty;
        var tempArray = stream.ToArray();
        return Convert.ToBase64String(tempArray, 0, tempArray.Length);
    }

    public static Image LoadImage(this string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new Manager().Decode(stream);
    }

    public static Image LoadImage(this Stream stream)
        => new Manager().Decode(stream);
}
