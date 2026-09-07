using System.Numerics;
using System.Security.Cryptography;

namespace _1;

public sealed class SecureRandomNumberGenerator : IRandomNumberGenerator
{
    public BigInteger Generate(BigInteger minInclusive, BigInteger maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentException("Минимальная граница не может быть больше максимальной.");
        }

        if (minInclusive == maxInclusive)
        {
            return minInclusive;
        }

        if (minInclusive < 0 || maxInclusive < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minInclusive), "Границы должны быть неотрицательными.");
        }

        var range = maxInclusive - minInclusive + 1;
        var randomValue = RandomBelow(range);
        return minInclusive + randomValue;
    }

    public BigInteger GeneratePositive(int bitLength)
    {
        if (bitLength < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Разрядность должна быть не меньше 2.");
        }

        var byteLength = (bitLength + 7) / 8;
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        bytes[^1] &= (byte)(0xFF >> (byteLength * 8 - bitLength));
        bytes[^1] |= (byte)(1 << ((bitLength - 1) % 8));
        bytes[0] |= 1;

        return new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
    }

    public BigInteger GeneratePrime(int bitLength, IPrimalityTest primalityTest, int iterations = 100)
    {
        if (primalityTest is null)
        {
            throw new ArgumentNullException(nameof(primalityTest));
        }

        while (true)
        {
            var candidate = GeneratePositive(bitLength);
            if (primalityTest.IsProbablyPrime(candidate, iterations))
            {
                return candidate;
            }
        }
    }

    private static BigInteger RandomBelow(BigInteger exclusiveUpperBound)
    {
        if (exclusiveUpperBound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
        }

        var byteLength = exclusiveUpperBound.ToByteArray(isUnsigned: true, isBigEndian: false).Length;
        var buffer = new byte[byteLength];
        var mask = GetMostSignificantByteMask(exclusiveUpperBound);

        do
        {
            RandomNumberGenerator.Fill(buffer);
            buffer[^1] &= mask;
        }
        while (new BigInteger(buffer, isUnsigned: true, isBigEndian: false) >= exclusiveUpperBound);

        return new BigInteger(buffer, isUnsigned: true, isBigEndian: false);
    }

    private static byte GetMostSignificantByteMask(BigInteger value)
    {
        var bits = GetBitLength(value - 1);
        var remainder = bits % 8;
        return remainder == 0 ? (byte)0xFF : (byte)((1 << remainder) - 1);
    }

    private static int GetBitLength(BigInteger value)
    {
        if (value <= 0)
        {
            return 1;
        }

        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        var first = bytes[0];
        var leading = 0;
        var mask = 0x80;
        while ((first & mask) == 0)
        {
            leading++;
            mask >>= 1;
        }

        return bytes.Length * 8 - leading;
    }
}
