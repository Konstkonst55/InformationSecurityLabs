using System.Numerics;
using _1;
using _10.Models;

namespace _10.Algorithms;

public sealed class Gost94SignatureAlgorithm
{
    public const string AlgorithmName = SignatureParameterGenerator.GostAlgorithmName;

    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;

    public Gost94SignatureAlgorithm(
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

        var h = GetHashRepresentative(hash, keyPair.Q);
        while (true)
        {
            var k = _randomNumberGenerator.Generate(BigInteger.One, keyPair.Q - BigInteger.One);
            var r = _modularExponentiation.Calculate(keyPair.Generator, k, keyPair.P) % keyPair.Q;
            if (r.IsZero)
            {
                continue;
            }

            var s = (k * h + keyPair.X * r) % keyPair.Q;
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
            var a = SignatureAlgorithmSupport.ReadParameter(publicParameters, "A");
            var y = SignatureAlgorithmSupport.ReadParameter(publicParameters, "Y");

            SignatureAlgorithmSupport.ValidatePublicKey(
                p, q, a, y, _primalityTest, _modularExponentiation);

            if (r <= BigInteger.Zero || r >= q || s <= BigInteger.Zero || s >= q)
            {
                return false;
            }

            var h = GetHashRepresentative(hash, q);
            var inverseH = SignatureAlgorithmSupport.ModInverse(
                h, q, _extendedEuclideanAlgorithm);
            var u1 = SignatureAlgorithmSupport.Normalize(s * inverseH, q);
            var u2 = SignatureAlgorithmSupport.Normalize(-r * inverseH, q);
            var first = _modularExponentiation.Calculate(a, u1, p);
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
        if (keyPair.Algorithm != AlgorithmName || keyPair.GeneratorParameterName != "A")
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
            throw new InvalidDataException("Закрытый и открытый ключи ГОСТ не согласованы.");
        }
    }

    private static BigInteger GetHashRepresentative(byte[] hash, BigInteger q)
    {
        var value = new BigInteger(hash, isUnsigned: true, isBigEndian: true) % q;
        return value.IsZero ? BigInteger.One : value;
    }
}
