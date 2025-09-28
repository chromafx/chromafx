using ChromaFx.Core;
using ChromaFx.Processing.Filters.ColorMatrix;
using System.Text;

namespace ChromaFx.Processing;

public static class ImageProcessingExtensions
{
    public static string ToAsciiArt(this Image image)
    {
        var AsciiCharacters = new[] { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
        var showLine = true;
        var tempImage = new Greyscale601().Apply(image.Copy());
        var builder = new StringBuilder();
        for (var y = 0; y < tempImage.Height; ++y)
        {
            for (var x = 0; x < tempImage.Width; ++x)
            {
                if (!showLine)
                    continue;
                var rValue = tempImage.Pixels[y * tempImage.Width + x].Red / 255f;
                builder.Append(AsciiCharacters[(int)(rValue * AsciiCharacters.Length)]);
            }
            if (showLine)
            {
                builder.AppendLine();
                showLine = false;
            }
            else
            {
                showLine = true;
            }
        }
        return builder.ToString();
    }
}
