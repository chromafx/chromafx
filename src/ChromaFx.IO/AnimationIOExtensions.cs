using ChromaFx.Core;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO;

public static class AnimationIOExtensions
{
    public static bool Save(this Animation animation, string fileName)
        => new Manager().Encode(fileName, animation);

    public static Animation LoadAnimation(string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
        return new Manager().DecodeAnimation(stream);
    }

    public static Animation LoadAnimation(Stream stream)
        => new Manager().DecodeAnimation(stream);
}
