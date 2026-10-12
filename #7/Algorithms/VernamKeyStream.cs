using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

namespace _7;

public sealed class VernamKeyStream
{
    private readonly byte[] _seed;
    private byte[] _buffer = Array.Empty<byte>();
    private ulong _counter;
    private int _position;

    public VernamKeyStream(BigInteger key)
    {
        if (key <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(key),
                "Ключ K должен быть положительным.");
        }

        var keyBytes = key.ToByteArray(
            isUnsigned: true,
            isBigEndian: true);
        _seed = SHA256.HashData(keyBytes);
        _position = 0;
    }

    public byte NextByte()
    {
        if (_position >= _buffer.Length)
        {
            Refill();
        }

        return _buffer[_position++];
    }

    private void Refill()
    {
        Span<byte> counterBytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(counterBytes, _counter);
        _counter++;
        _buffer = HMACSHA256.HashData(_seed, counterBytes);
        _position = 0;
    }
}
