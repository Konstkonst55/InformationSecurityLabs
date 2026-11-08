using System.Numerics;
using _1;
using _10.Models;

namespace _10.Algorithms;

public sealed class DsaSignatureAlgorithm
{
    public const string AlgorithmName = SignatureParameterGenerator.DsaAlgorithmName;

    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;

    public DsaSignatureAlgorithm(
        IModularExponentiation modularExponentiation,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest)
    {
        _modularExponentiation = modularExponentiation;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
    }

    public SignaturePair SignHash(byte[] hash, SignatureKeyPair keyPair)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(keyPair);
        ValidateKeyPair(keyPair);

        var z = GetDsaHashInteger(hash, (int)keyPair.Q.GetBitLength());
        while (true)
        {
            var k = _randomNumberGenerator.Generate(BigInteger.One, keyPair.Q - BigInteger.One);
            var r = _modularExponentiation.Calculate(keyPair.Generator, k, keyPair.P) % keyPair.Q;
            if (r.IsZero)
            {
                continue;
            }

            var kInverse = SignatureAlgorithmSupport.ModInverse(
                k, keyPair.Q, _extendedEuclideanAlgorithm);
            var s = SignatureAlgorithmSupport.Normalize(
                kInverse * (z + keyPair.X * r), keyPair.Q);
            if (!s.IsZero)
            {
                return new SignaturePair(r, s);
            }
        }
    }

    public bool VerifyHash(
        byte[] hash,
        BigInteger r,
        BigInteger s,
        IReadOnlyDictionary<string, string> publicParameters)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(hash);
            var p = SignatureAlgorithmSupport.ReadParameter(publicParameters, "P");
            var q = SignatureAlgorithmSupport.ReadParameter(publicParameters, "Q");
            var g = SignatureAlgorithmSupport.ReadParameter(publicParameters, "G");
            var y = SignatureAlgorithmSupport.ReadParameter(publicParameters, "Y");

            SignatureAlgorithmSupport.ValidatePublicKey(
                p, q, g, y, _primalityTest, _modularExponentiation);

            if (r <= BigInteger.Zero || r >= q || s <= BigInteger.Zero || s >= q)
            {
                return false;
            }

            var z = GetDsaHashInteger(hash, (int)q.GetBitLength());
            var w = SignatureAlgorithmSupport.ModInverse(
                s, q, _extendedEuclideanAlgorithm);
            var u1 = z * w % q;
            var u2 = r * w % q;
            var first = _modularExponentiation.Calculate(g, u1, p);
            var second = _modularExponentiation.Calculate(y, u2, p);
            var v = first * second % p % q;

            return v == r;
        }
        catch
        {
            return false;
        }
    }

    private void ValidateKeyPair(SignatureKeyPair keyPair)
    {
        if (keyPair.Algorithm != AlgorithmName || keyPair.GeneratorParameterName != "G")
        {
            throw new ArgumentException("Передана пара ключей другого алгоритма.", nameof(keyPair));
        }

        SignatureAlgorithmSupport.ValidatePublicKey(
            keyPair.P,
            keyPair.Q,
            keyPair.Generator,
            keyPair.Y,
            _primalityTest,
            _modularExponentiation);

        if (keyPair.X <= BigInteger.Zero || keyPair.X >= keyPair.Q ||
            _modularExponentiation.Calculate(keyPair.Generator, keyPair.X, keyPair.P) != keyPair.Y)
        {
            throw new InvalidDataException("Закрытый и открытый ключи DSA не согласованы.");
        }
    }

    private static BigInteger GetDsaHashInteger(byte[] hash, int qBitLength)
    {
        var value = new BigInteger(hash, isUnsigned: true, isBigEndian: true);
        var hashBitLength = hash.Length * 8;
        if (hashBitLength > qBitLength)
        {
            value >>= hashBitLength - qBitLength;
        }

        return value;
    }
}
