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

namespace ChromaFx.Formats.Gif.Format.Helpers;

/// <summary>
/// LZW Encoder class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LzwEncoder"/> class.
/// </remarks>
/// <param name="indexedPixels">The indexed pixels.</param>
/// <param name="colorDepth">The color depth.</param>
public class LzwEncoder(byte[] indexedPixels, int colorDepth)
{
    private const int Bits = 12;
    private const int Eof = -1;
    private const int HashSize = 5003;

    private readonly byte[] _accumulators = new byte[256];

    private readonly int[] _codeTable = new int[HashSize];
    private readonly int[] _hashTable = new int[HashSize];
    private readonly int _initialCodeSize = Math.Max(2, colorDepth);

    private readonly int[] _masks =
    [
        0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F, 0x00FF,
        0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF
    ];

    private readonly byte[] _pixelArray = indexedPixels;

    private int _accumulatorCount;
    private int _bitCount;

    private int _clearCode;

    private bool _clearFlag;

    private int _curPixel;
    private int _currentAccumulator;

    private int _currentBits;

    private int _eofCode;

    private int _freeEntry;

    private int _globalInitialBits;

    private readonly int _hsize = HashSize;

    private readonly int _maxbits = Bits;

    private int _maxcode;

    private readonly int _maxmaxcode = 1 << Bits;

    /// <summary>
    /// Encodes the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Encode(Stream stream)
    {
        stream.WriteByte((byte)_initialCodeSize);
        _curPixel = 0;
        Compress(_initialCodeSize + 1, stream);
        stream.WriteByte(SectionTypes.Terminator);
    }

    /// <summary>
    /// Gets the maxcode.
    /// </summary>
    /// <param name="bitCount">The bit count.</param>
    /// <returns></returns>
    private static int GetMaxcode(int bitCount)
    {
        return (1 << bitCount) - 1;
    }

    /// <summary>
    /// Adds the character.
    /// </summary>
    /// <param name="c">The c.</param>
    /// <param name="stream">The stream.</param>
    private void AddCharacter(byte c, Stream stream)
    {
        _accumulators[_accumulatorCount++] = c;
        if (_accumulatorCount >= 254)
        {
            FlushPacket(stream);
        }
    }

    /// <summary>
    /// Clears the block.
    /// </summary>
    /// <param name="stream">The stream.</param>
    private void ClearBlock(Stream stream)
    {
        ResetCodeTable(_hsize);
        _freeEntry = _clearCode + 2;
        _clearFlag = true;

        Output(_clearCode, stream);
    }

    /// <summary>
    /// Compresses the specified intial bits.
    /// </summary>
    /// <param name="intialBits">The intial bits.</param>
    /// <param name="stream">The stream.</param>
    private void Compress(int intialBits, Stream stream)
    {
        int fcode;
        int c;
        int ent;
        int hsizeReg;
        int hshift;

        // Set up the globals:  globalInitialBits - initial number of bits
        _globalInitialBits = intialBits;

        // Set up the necessary values
        _clearFlag = false;
        _bitCount = _globalInitialBits;
        _maxcode = GetMaxcode(_bitCount);

        _clearCode = 1 << (intialBits - 1);
        _eofCode = _clearCode + 1;
        _freeEntry = _clearCode + 2;

        _accumulatorCount = 0; // clear packet

        ent = NextPixel();

        hshift = 0;
        for (fcode = _hsize; fcode < 65536; fcode *= 2)
        {
            ++hshift;
        }
        hshift = 8 - hshift; // set hash code range bound

        hsizeReg = _hsize;

        ResetCodeTable(hsizeReg); // clear hash table

        Output(_clearCode, stream);

        while ((c = NextPixel()) != Eof)
        {
            fcode = (c << _maxbits) + ent;
            var i = (c << hshift) ^ ent /* = 0 */;

            if (_hashTable[i] == fcode)
            {
                ent = _codeTable[i];
                continue;
            }

            // Non-empty slot
            if (_hashTable[i] >= 0)
            {
                var disp = hsizeReg - i;
                if (i == 0)
                    disp = 1;
                do
                {
                    if ((i -= disp) < 0)
                    {
                        i += hsizeReg;
                    }

                    if (_hashTable[i] == fcode)
                    {
                        ent = _codeTable[i];
                        break;
                    }
                }
                while (_hashTable[i] >= 0);

                if (_hashTable[i] == fcode)
                {
                    continue;
                }
            }

            Output(ent, stream);
            ent = c;
            if (_freeEntry < _maxmaxcode)
            {
                _codeTable[i] = _freeEntry++; // code -> hashtable
                _hashTable[i] = fcode;
            }
            else ClearBlock(stream);
        }

        // Put out the final code.
        Output(ent, stream);

        Output(_eofCode, stream);
    }

    /// <summary>
    /// Flushes the packet.
    /// </summary>
    /// <param name="outs">The outs.</param>
    private void FlushPacket(Stream outs)
    {
        if (_accumulatorCount > 0)
        {
            outs.WriteByte((byte)_accumulatorCount);
            outs.Write(_accumulators, 0, _accumulatorCount);
            _accumulatorCount = 0;
        }
    }

    /// <summary>
    /// Nexts the pixel.
    /// </summary>
    /// <returns></returns>
    private int NextPixel()
    {
        if (_curPixel == _pixelArray.Length)
        {
            return Eof;
        }

        if (_curPixel == _pixelArray.Length)
            return Eof;

        _curPixel++;
        return _pixelArray[_curPixel - 1] & 0xff;
    }

    /// <summary>
    /// Outputs the specified code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="outs">The outs.</param>
    private void Output(int code, Stream outs)
    {
        _currentAccumulator &= _masks[_currentBits];

        if (_currentBits > 0) _currentAccumulator |= code << _currentBits;
        else _currentAccumulator = code;

        _currentBits += _bitCount;

        while (_currentBits >= 8)
        {
            AddCharacter((byte)(_currentAccumulator & 0xff), outs);
            _currentAccumulator >>= 8;
            _currentBits -= 8;
        }

        // If the next entry is going to be too big for the code size,
        // then increase it, if possible.
        if (_freeEntry > _maxcode || _clearFlag)
        {
            if (_clearFlag)
            {
                _maxcode = GetMaxcode(_bitCount = _globalInitialBits);
                _clearFlag = false;
            }
            else
            {
                ++_bitCount;
                _maxcode = _bitCount == _maxbits
                    ? _maxmaxcode
                    : GetMaxcode(_bitCount);
            }
        }

        if (code == _eofCode)
        {
            // At EOF, write the rest of the buffer.
            while (_currentBits > 0)
            {
                AddCharacter((byte)(_currentAccumulator & 0xff), outs);
                _currentAccumulator >>= 8;
                _currentBits -= 8;
            }

            FlushPacket(outs);
        }
    }

    /// <summary>
    /// Resets the code table.
    /// </summary>
    /// <param name="size">The size.</param>
    private void ResetCodeTable(int size)
    {
        for (var i = 0; i < size; ++i)
        {
            _hashTable[i] = -1;
        }
    }
}