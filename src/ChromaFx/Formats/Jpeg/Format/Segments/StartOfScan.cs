/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Formats.Jpeg.Format.HelperClasses;
using ChromaFx.Formats.Jpeg.Format.HelperClasses.Enums;
using ChromaFx.Formats.Jpeg.Format.Segments.BaseClasses;
using ChromaFx.Formats.Jpeg.Format.Enums;
using ChromaFx.Colors.ColorSpaces;

namespace ChromaFx.Formats.Jpeg.Format.Segments;

/// <summary>
/// Start of scan segment
/// </summary>
/// <seealso cref="SegmentBase"/>
public class StartOfScan : SegmentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartOfScan"/> class.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    public StartOfScan(ByteBuffer bytes)
        : base(SegmentTypes.StartOfScan, bytes)
    {
        _progressiveCoefficients = new Block[MaximumNumberComponents][];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StartOfScan"/> class.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="defineHuffmanTableSegment">The define huffman table segment.</param>
    /// <param name="defineQuantizationTableSegment">The define quantization table segment.</param>
    public StartOfScan(Image image, DefineHuffmanTable defineHuffmanTableSegment, DefineQuantizationTable defineQuantizationTableSegment)
        : base(SegmentTypes.StartOfScan, null)
    {
        Image = image;
        DefineHuffmanTableSegment = defineHuffmanTableSegment;
        DefineQuantizationTableSegment = defineQuantizationTableSegment;
    }

    /// <summary>
    /// Gets or sets the image.
    /// </summary>
    /// <value>The image.</value>
    public Image Image { get; set; }

    /// <summary>
    /// Gets or sets the define huffman table segment.
    /// </summary>
    /// <value>The define huffman table segment.</value>
    private DefineHuffmanTable DefineHuffmanTableSegment { get; }

    /// <summary>
    /// Gets or sets the define quantization table segment.
    /// </summary>
    /// <value>The define quantization table segment.</value>
    private DefineQuantizationTable DefineQuantizationTableSegment { get; }

    /// <summary>
    /// The grey image
    /// </summary>
    public GreyImage Img1;

    private const int AcTable = 1;
    private const int DcTable = 0;
    private const int MaximumNumberComponents = 4;

    private const int MaximumTh = 3;

    private static readonly int[] Unzig =
    [
        0, 1, 8, 16, 9, 2, 3, 10,
        17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46,
        53, 60, 61, 54, 47, 55, 62, 63
    ];

    private readonly byte[] _bitCount =
    [
        0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
    ];

    /// <summary>
    /// The sos header for y cb cr
    /// </summary>
    private readonly byte[] _sosHeaderYCbCr =
    [
        0xff, 0xda, 0x00, 0x0c, 0x03, 0x01, 0x00, 0x02,
        0x11, 0x03, 0x11, 0x00, 0x3f, 0x00
    ];

    private uint _bits;
    private ushort _endOfBlockRun;
    private YcbcrImg _img3;
    private uint _nBits;
    private readonly Block[][] _progressiveCoefficients;

    /// <summary>
    /// Converts the scan information into an image
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="segments">The segments.</param>
    /// <returns>The resulting image</returns>
    public Image Convert(Image image, IEnumerable<SegmentBase> segments)
    {
        var frame = segments.OfType<StartOfFrame>().FirstOrDefault();
        if (Img1 != null)
        {
            return Img1.Convert(frame.Width, frame.Height, image, segments);
        }

        if (_img3 != null)
        {
            return _img3.Convert(frame.Width, frame.Height, image, segments);
        }
        return image;
    }

    /// <summary>
    /// Setups the specified segments.
    /// </summary>
    /// <param name="segments">The segments.</param>
    /// <exception cref="System.Exception">
    /// missing SOF marker or SOS has wrong length or SOS length inconsistent with number of
    /// components or Unknown component selector or Repeated component selector or bad Td value
    /// or bad Ta value or Total sampling factors too large. or bad spectral selection bounds or
    /// progressive AC coefficients for more than one component or bad successive approximation
    /// values or Excessive DC component or Too many components or Too many components or Bad RST marker
    /// </exception>
    public override void Setup(IEnumerable<SegmentBase> segments)
    {
        Length = GetLength(Bytes);
        var n = Length;
        var startOfFrameSegment = segments.OfType<StartOfFrame>().FirstOrDefault();
        var defineHuffmanTableSegment = segments.OfType<DefineHuffmanTable>().FirstOrDefault();
        var defineRestartIntervalSegment = segments.OfType<DefineRestartInterval>().FirstOrDefault() ?? new DefineRestartInterval(null);
        var defineQuantizationTableSegment = segments.OfType<DefineQuantizationTable>().FirstOrDefault();
        if (startOfFrameSegment == null)
        {
            throw new Exception("missing SOF marker");
        }

        if (n < 6 || 4 + 2 * (int)startOfFrameSegment.TypeOfImage < n || n % 2 != 0)
        {
            throw new Exception("SOS has wrong length");
        }

        Bytes.ReadFull(TempData, 0, n);
        var lnComp = TempData[0];

        if (n != 4 + 2 * lnComp)
        {
            throw new Exception("SOS length inconsistent with number of components");
        }

        var scan = new Scan[MaximumNumberComponents];
        var totalHv = 0;

        for (var i = 0; i < lnComp; i++)
        {
            int cs = TempData[1 + 2 * i];
            var compIndex = -1;
            for (var j = 0; j < (int)startOfFrameSegment.TypeOfImage; j++)
            {
                var compv = startOfFrameSegment.Components[j];
                if (cs == compv.ComponentIdentifier)
                {
                    compIndex = j;
                }
            }

            if (compIndex < 0)
            {
                throw new Exception("Unknown component selector");
            }

            scan[i].ComponentIndex = (byte)compIndex;
            for (var j = 0; j < i; j++)
            {
                if (scan[i].ComponentIndex == scan[j].ComponentIndex)
                {
                    throw new Exception("Repeated component selector");
                }
            }

            totalHv += startOfFrameSegment.Components[compIndex].HorizontalSamplingFactor * startOfFrameSegment.Components[compIndex].VerticalSamplingFactor;

            scan[i].Td = (byte)(TempData[2 + 2 * i] >> 4);
            if (scan[i].Td > MaximumTh)
            {
                throw new Exception("bad Td value");
            }

            scan[i].Ta = (byte)(TempData[2 + 2 * i] & 0x0f);
            if (scan[i].Ta > MaximumTh)
            {
                throw new Exception("bad Ta value");
            }
        }

        if (startOfFrameSegment.TypeOfImage != ImageType.GreyScale && totalHv > 10)
        {
            throw new Exception("Total sampling factors too large.");
        }

        var zigStart = 0;
        var zigEnd = Block.BlockSize - 1;
        var ah = 0;
        var al = 0;

        if (startOfFrameSegment.Progressive)
        {
            zigStart = TempData[1 + 2 * lnComp];
            zigEnd = TempData[2 + 2 * lnComp];
            ah = TempData[3 + 2 * lnComp] >> 4;
            al = TempData[3 + 2 * lnComp] & 0x0f;

            if ((zigStart == 0 && zigEnd != 0) || zigStart > zigEnd || zigEnd >= Block.BlockSize)
            {
                throw new Exception("bad spectral selection bounds");
            }

            if (zigStart != 0 && lnComp != 1)
            {
                throw new Exception("progressive AC coefficients for more than one component");
            }

            if (ah != 0 && ah != al + 1)
            {
                throw new Exception("bad successive approximation values");
            }
        }

        var h0 = startOfFrameSegment.Components[0].HorizontalSamplingFactor;
        var v0 = startOfFrameSegment.Components[0].VerticalSamplingFactor;
        var mxx = (startOfFrameSegment.Width + 8 * h0 - 1) / (8 * h0);
        var myy = (startOfFrameSegment.Height + 8 * v0 - 1) / (8 * v0);

        if (Img1 == null && _img3 == null)
        {
            MakeImg(mxx, myy, startOfFrameSegment);
        }

        if (startOfFrameSegment.Progressive)
        {
            for (var i = 0; i < lnComp; i++)
            {
                int compIndex = scan[i].ComponentIndex;
                if (_progressiveCoefficients[compIndex] == null)
                {
                    _progressiveCoefficients[compIndex] = new Block[mxx * myy * startOfFrameSegment.Components[compIndex].HorizontalSamplingFactor * startOfFrameSegment.Components[compIndex].VerticalSamplingFactor];

                    for (var j = 0; j < _progressiveCoefficients[compIndex].Length; j++)
                    {
                        _progressiveCoefficients[compIndex][j] = new Block();
                    }
                }
            }
        }

        Bytes.Bits = new BitsBuffer();

        var mcu = 0;
        byte expectedRst = SegmentTypes.Restart0;

        var b = new Block();
        var dc = new int[MaximumNumberComponents];

        int bx, by, blockCount = 0;

        for (var my = 0; my < myy; my++)
        {
            for (var mx = 0; mx < mxx; mx++)
            {
                for (var i = 0; i < lnComp; i++)
                {
                    int compIndex = scan[i].ComponentIndex;
                    var hi = startOfFrameSegment.Components[compIndex].HorizontalSamplingFactor;
                    var vi = startOfFrameSegment.Components[compIndex].VerticalSamplingFactor;
                    var qt = defineQuantizationTableSegment.Quant[startOfFrameSegment.Components[compIndex].QuatizationTableDestSelector];

                    for (var j = 0; j < hi * vi; j++)
                    {
                        if (lnComp != 1)
                        {
                            bx = hi * mx + j % hi;
                            by = vi * my + j / hi;
                        }
                        else
                        {
                            var q = mxx * hi;
                            bx = blockCount % q;
                            by = blockCount / q;
                            blockCount++;
                            if (bx * 8 >= startOfFrameSegment.Width || by * 8 >= startOfFrameSegment.Height)
                                continue;
                        }
                        if (startOfFrameSegment.Progressive)
                            b = _progressiveCoefficients[compIndex][by * mxx * hi + bx];
                        else
                            b = new Block();

                        if (ah != 0)
                        {
                            Refine(b, defineHuffmanTableSegment.HuffmanCodes[AcTable, scan[i].Ta], zigStart, zigEnd, 1 << al);
                        }
                        else
                        {
                            var zig = zigStart;
                            if (zig == 0)
                            {
                                zig++;
                                var value = defineHuffmanTableSegment.HuffmanCodes[DcTable, scan[i].Td].DecodeHuffman(Bytes);
                                if (value > 16)
                                {
                                    throw new Exception("Excessive DC component");
                                }

                                var dcDelta = Bytes.ReceiveExtend(value);
                                dc[compIndex] += dcDelta;
                                b[0] = dc[compIndex] << al;
                            }

                            if (zig <= zigEnd && _endOfBlockRun > 0)
                            {
                                _endOfBlockRun--;
                            }
                            else
                            {
                                var huffv = defineHuffmanTableSegment.HuffmanCodes[AcTable, scan[i].Ta];
                                for (; zig <= zigEnd; zig++)
                                {
                                    var value = huffv.DecodeHuffman(Bytes);
                                    var val0 = (byte)(value >> 4);
                                    var val1 = (byte)(value & 0x0f);
                                    if (val1 != 0)
                                    {
                                        zig += val0;
                                        if (zig > zigEnd)
                                            break;

                                        var ac = Bytes.ReceiveExtend(val1);
                                        b[Unzig[zig]] = ac << al;
                                    }
                                    else
                                    {
                                        if (val0 != 0x0f)
                                        {
                                            _endOfBlockRun = (ushort)(1 << val0);
                                            if (val0 != 0)
                                            {
                                                _endOfBlockRun |= (ushort)Bytes.DecodeBits(val0);
                                            }
                                            _endOfBlockRun--;
                                            break;
                                        }
                                        zig += 0x0f;
                                    }
                                }
                            }
                        }

                        if (startOfFrameSegment.Progressive)
                        {
                            if (zigEnd != Block.BlockSize - 1 || al != 0)
                            {
                                _progressiveCoefficients[compIndex][by * mxx * hi + bx] = b;
                                continue;
                            }
                        }
                        for (var zig = 0; zig < Block.BlockSize; zig++)
                            b[Unzig[zig]] *= qt[zig];

                        Idct.Transform(b);
                        byte[] dst;
                        int offset;
                        int stride;
                        if (startOfFrameSegment.TypeOfImage == ImageType.GreyScale)
                        {
                            dst = Img1.Pixels;
                            stride = Img1.Stride;
                            offset = Img1.Offset + 8 * (by * Img1.Stride + bx);
                        }
                        else
                        {
                            switch (compIndex)
                            {
                                case 0:
                                    dst = _img3.YPixels;
                                    stride = _img3.YStride;
                                    offset = _img3.YOffset + 8 * (by * _img3.YStride + bx);
                                    break;

                                case 1:
                                    dst = _img3.CbPixels;
                                    stride = _img3.CStride;
                                    offset = _img3.COffset + 8 * (by * _img3.CStride + bx);
                                    break;

                                case 2:
                                    dst = _img3.CrPixels;
                                    stride = _img3.CStride;
                                    offset = _img3.COffset + 8 * (by * _img3.CStride + bx);
                                    break;

                                case 3:
                                    throw new Exception("Too many components");

                                default:
                                    throw new Exception("Too many components");
                            }
                        }

                        for (var y = 0; y < 8; y++)
                        {
                            var y8 = y * 8;
                            var yStride = y * stride;

                            for (var x = 0; x < 8; x++)
                            {
                                var c = b[y8 + x];
                                if (c < -128)
                                {
                                    c = 0;
                                }
                                else if (c > 127)
                                {
                                    c = 255;
                                }
                                else
                                {
                                    c += 128;
                                }
                                dst[yStride + x + offset] = (byte)c;
                            }
                        }
                    }
                }

                mcu++;

                if (defineRestartIntervalSegment.RestartInterval > 0 && mcu % defineRestartIntervalSegment.RestartInterval == 0 && mcu < mxx * myy)
                {
                    Bytes.ReadFull(TempData, 0, 2);
                    if (TempData[0] != 0xff || TempData[1] != expectedRst)
                    {
                        throw new Exception("Bad RST marker");
                    }

                    expectedRst++;
                    if (expectedRst == SegmentTypes.Restart7 + 1)
                    {
                        expectedRst = SegmentTypes.Restart0;
                    }
                    Bytes.Bits = new BitsBuffer();
                    dc = new int[MaximumNumberComponents];
                    _endOfBlockRun = 0;
                }
            }
        }
    }

    /// <summary>
    /// Writes the information to the specified writer.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    public override void Write(BinaryWriter writer)
    {
        writer.Write(_sosHeaderYCbCr, 0, _sosHeaderYCbCr.Length);
        Encode444(writer);
        Emit(0x7f, 7, writer);
    }

    private static int Div(int a, int b)
    {
        if (a >= 0)
            return (a + (b >> 1)) / b;
        else
            return -((-a + (b >> 1)) / b);
    }

    private void Emit(uint bits, uint nBits, BinaryWriter writer)
    {
        nBits += this._nBits;
        bits <<= (int)(32 - nBits);
        bits |= this._bits;
        while (nBits >= 8)
        {
            var b = (byte)(bits >> 24);
            writer.Write(b);
            if (b == 0xff)
                writer.Write((byte)0x00);
            bits <<= 8;
            nBits -= 8;
        }
        this._bits = bits;
        this._nBits = nBits;
    }

    private void EmitHuff(HuffmanIndex h, int v, BinaryWriter writer)
    {
        var x = DefineHuffmanTableSegment.HuffmanLookUpTables[(int)h].Values[v];
        Emit(x & ((1 << 24) - 1), x >> 24, writer);
    }

    private void EmitHuffRle(HuffmanIndex h, int runLength, int v, BinaryWriter writer)
    {
        var a = v;
        var b = v;
        if (a < 0)
        {
            a = -v;
            b = v - 1;
        }
        uint nBits;
        if (a < 0x100)
            nBits = _bitCount[a];
        else
            nBits = 8 + (uint)_bitCount[a >> 8];

        EmitHuff(h, (int)((uint)(runLength << 4) | nBits), writer);
        if (nBits > 0) Emit((uint)b & (uint)((1 << (int)nBits) - 1), nBits, writer);
    }

    private void Encode444(BinaryWriter writer)
    {
        var b = new Block();
        var cb = new Block();
        var cr = new Block();
        int prevDcy = 0, prevDcCb = 0, prevDcCr = 0;

        for (var y = 0; y < Image.Height; y += 8)
        {
            for (var x = 0; x < Image.Width; x += 8)
            {
                ToYCbCr(x, y, b, cb, cr);
                prevDcy = WriteBlock(b, 0, prevDcy, writer);
                prevDcCb = WriteBlock(cb, (QuantIndex)1, prevDcCb, writer);
                prevDcCr = WriteBlock(cr, (QuantIndex)1, prevDcCr, writer);
            }
        }
    }

    private void MakeImg(int mxx, int myy, StartOfFrame frame)
    {
        if (frame.TypeOfImage == ImageType.GreyScale)
        {
            var m = new GreyImage(8 * mxx, 8 * myy);
            Img1 = m.SubImage(0, 0, frame.Width, frame.Height);
        }
        else
        {
            var h0 = frame.Components[0].HorizontalSamplingFactor;
            var v0 = frame.Components[0].VerticalSamplingFactor;
            var hRatio = h0 / frame.Components[1].HorizontalSamplingFactor;
            var vRatio = v0 / frame.Components[1].VerticalSamplingFactor;

            var ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio444;
            switch ((hRatio << 4) | vRatio)
            {
                case 0x11:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio444;
                    break;

                case 0x12:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio440;
                    break;

                case 0x21:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio422;
                    break;

                case 0x22:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio420;
                    break;

                case 0x41:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio411;
                    break;

                case 0x42:
                    ratio = YCbCrSubsampleRatio.YCbCrSubsampleRatio410;
                    break;
            }

            var m = new YcbcrImg(8 * h0 * mxx, 8 * v0 * myy, ratio);
            _img3 = m.SubImage(0, 0, frame.Width, frame.Height);
        }
    }

    private void Refine(Block b, Huffman h, int zigStart, int zigEnd, int delta)
    {
        if (zigStart == 0)
        {
            if (zigEnd != 0)
                throw new Exception("Invalid state for zig DC component");

            var bit = Bytes.DecodeBit();
            if (bit)
                b[0] |= delta;

            return;
        }

        var zig = zigStart;
        if (_endOfBlockRun == 0)
        {
            for (; zig <= zigEnd; zig++)
            {
                var done = false;
                var z = 0;
                var val = h.DecodeHuffman(Bytes);
                var val0 = val >> 4;
                var val1 = val & 0x0f;

                switch (val1)
                {
                    case 0:
                        if (val0 != 0x0f)
                        {
                            _endOfBlockRun = (ushort)(1 << val0);
                            if (val0 != 0)
                            {
                                var bits = Bytes.DecodeBits(val0);
                                _endOfBlockRun |= (ushort)bits;
                            }

                            done = true;
                        }
                        break;

                    case 1:
                        z = delta;
                        var bit = Bytes.DecodeBit();
                        if (!bit)
                            z = -z;
                        break;

                    default:
                        throw new Exception("unexpected Huffman code");
                }

                if (done)
                    break;

                zig = RefineNonZeroes(b, zig, zigEnd, val0, delta);
                if (zig > zigEnd)
                    throw new Exception(string.Format("too many coefficients {0} > {1}", zig, zigEnd));

                if (z != 0)
                    b[Unzig[zig]] = z;
            }
        }

        if (_endOfBlockRun > 0)
        {
            _endOfBlockRun--;
            RefineNonZeroes(b, zig, zigEnd, -1, delta);
        }
    }

    private int RefineNonZeroes(Block b, int zig, int zigEnd, int nz, int delta)
    {
        for (; zig <= zigEnd; zig++)
        {
            var u = Unzig[zig];
            if (b[u] == 0)
            {
                if (nz == 0)
                    break;
                nz--;
                continue;
            }

            var bit = Bytes.DecodeBit();
            if (!bit)
                continue;

            if (b[u] >= 0)
                b[u] += delta;
            else
                b[u] -= delta;
        }

        return zig;
    }

    private void ToYCbCr(int x, int y, Block yBlock, Block cbBlock, Block crBlock)
    {
        var xmax = Image.Width - 1;
        var ymax = Image.Height - 1;
        for (var j = 0; j < 8; j++)
        {
            for (var i = 0; i < 8; i++)
            {
                var offset = Math.Min(x + i, xmax) + Math.Min(y + j, ymax) * Image.Width;
                YCbCr color = Image.Pixels[offset];
                var index = 8 * j + i;
                yBlock[index] = (int)color.YLuminance;
                cbBlock[index] = (int)color.CbChroma;
                crBlock[index] = (int)color.CrChroma;
            }
        }
    }

    private int WriteBlock(Block b, QuantIndex q, int prevDc, BinaryWriter writer)
    {
        ForwardDiscreteCosineTransform.Transform(b);

        // Emit the DC delta.
        var dc = Div(b[0], 8 * DefineQuantizationTableSegment.Quant[(int)q][0]);
        EmitHuffRle((HuffmanIndex)(2 * (int)q + 0), 0, dc - prevDc, writer);

        // Emit the AC components.
        var h = (HuffmanIndex)(2 * (int)q + 1);
        var runLength = 0;

        for (var zig = 1; zig < Block.BlockSize; zig++)
        {
            var ac = Div(b[Unzig[zig]], 8 * DefineQuantizationTableSegment.Quant[(int)q][zig]);

            if (ac == 0)
            {
                runLength++;
            }
            else
            {
                while (runLength > 15)
                {
                    EmitHuff(h, 0xf0, writer);
                    runLength -= 16;
                }

                EmitHuffRle(h, runLength, ac, writer);
                runLength = 0;
            }
        }
        if (runLength > 0)
            EmitHuff(h, 0x00, writer);
        return dc;
    }
}