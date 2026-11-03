namespace ChromaFx.Tests.BaseClasses;

/// <summary>
/// BMP byte compares that tolerate historical <c>bfSize</c>/<c>biSizeImage</c> mismatches
/// (older encodes used <c>PixelCount * 4</c>; current encodes use padded 24-bit row bytes).
/// </summary>
internal static class BmpTestCompare
{
    private const int BmpHeaderSize = 54;

    public static bool Equal(byte[] expected, byte[] actual)
    {
        if (expected.Length != actual.Length)
            return false;
        if (expected.SequenceEqual(actual))
            return true;
        if (expected.Length < BmpHeaderSize || expected[0] != (byte)'B' || expected[1] != (byte)'M')
            return false;
        if (actual[0] != (byte)'B' || actual[1] != (byte)'M')
            return false;

        // [0,2) BM; skip [2,6) bfSize; [6,34); skip [34,38) biSizeImage; [38,).
        return expected.AsSpan(0, 2).SequenceEqual(actual.AsSpan(0, 2))
            && expected.AsSpan(6, 28).SequenceEqual(actual.AsSpan(6, 28))
            && expected.AsSpan(38).SequenceEqual(actual.AsSpan(38));
    }

    public static bool HeadersCompatible(byte[] expected, byte[] actual)
    {
        if (expected.Length < BmpHeaderSize || actual.Length < BmpHeaderSize)
            return false;
        if (expected[0] != (byte)'B' || expected[1] != (byte)'M')
            return false;
        if (actual[0] != (byte)'B' || actual[1] != (byte)'M')
            return false;
        return expected.AsSpan(6, 28).SequenceEqual(actual.AsSpan(6, 28));
    }
}
