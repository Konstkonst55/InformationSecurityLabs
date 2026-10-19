using System.Numerics;
using SignatureShared;
using _1;

namespace _8;

public sealed class RsaSignatureAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IPrimalityTest _primalityTest;

    public RsaSignatureAlgorithm(
        IModularExponentiation modularExponentiation,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IPrimalityTest primalityTest)
    {
        _modularExponentiation = modularExponentiation;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
        _primalityTest = primalityTest;
    }

    public RsaSignatureKeys CreateKeys(
        BigInteger p,
        BigInteger q,
        BigInteger d)
    {
        ValidatePrime(p, nameof(p));
        ValidatePrime(q, nameof(q));

        if (p == q)
        {
            throw new ArgumentException("P и Q должны быть разными простыми числами.");
        }

        var n = p * q;
        if (n <= byte.MaxValue)
        {
            throw new ArgumentException("N = P * Q должно быть больше 255.");
        }

        var phi = (p - 1) * (q - 1);
        if (d <= 1 || d >= phi)
        {
            throw new ArgumentOutOfRangeException(
                nameof(d),
                "d должно удовлетворять условию 1 < d < φ.");
        }

        if (_extendedEuclideanAlgorithm.Calculate(d, phi).Gcd != 1)
        {
            throw new ArgumentException("d должно быть взаимно простым с φ.", nameof(d));
        }

        var c = ModularInverse(d, phi);
        return new RsaSignatureKeys(p, q, n, phi, d, c);
    }

    public RsaSignatureKeys GenerateKeys(
        int primeBitLength,
        IRandomNumberGenerator randomNumberGenerator)
    {
        if (primeBitLength < 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(primeBitLength),
                "Разрядность простых чисел должна быть не меньше 8.");
        }

        while (true)
        {
            var p = randomNumberGenerator.GeneratePrime(primeBitLength, _primalityTest, 40);
            var q = randomNumberGenerator.GeneratePrime(primeBitLength, _primalityTest, 40);
            if (p == q || p * q <= byte.MaxValue)
            {
                continue;
            }

            var phi = (p - 1) * (q - 1);
            var d = GenerateD(phi, randomNumberGenerator);
            var c = ModularInverse(d, phi);
            return new RsaSignatureKeys(p, q, p * q, phi, d, c);
        }
    }

    public SignatureDocument SignHash(
        byte[] hash,
        string originalFileName,
        RsaSignatureKeys keys)
    {
        ArgumentNullException.ThrowIfNull(hash);
        EnsureHashBytesFit(hash, keys.N);

        var signatureValues = new List<List<string>>(hash.Length);
        foreach (var hashByte in hash)
        {
            var s = _modularExponentiation.Calculate(
                new BigInteger(hashByte),
                keys.C,
                keys.N);
            signatureValues.Add(new List<string> { s.ToString() });
        }

        return new SignatureDocument
        {
            Algorithm = "RSA",
            HashAlgorithm = "SHA-256",
            OriginalFileName = Path.GetFileName(originalFileName),
            HashHex = Convert.ToHexString(hash),
            PublicParameters = new Dictionary<string, string>
            {
                ["N"] = keys.N.ToString(),
                ["d"] = keys.D.ToString()
            },
            SignatureValues = signatureValues
        };
    }

    public bool VerifyHash(byte[] hash, SignatureDocument document)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(document);

        if (!string.Equals(document.Algorithm, "RSA", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(document.HashAlgorithm, "SHA-256", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Алгоритм подписи или хеширования не поддерживается.");
        }

        if (!document.PublicParameters.TryGetValue("N", out var nValue) ||
            !BigInteger.TryParse(nValue, out var n) || n <= byte.MaxValue)
        {
            throw new InvalidDataException("Параметр N отсутствует или некорректен.");
        }

        if (!document.PublicParameters.TryGetValue("d", out var dValue) ||
            !BigInteger.TryParse(dValue, out var d) || d <= 0)
        {
            throw new InvalidDataException("Открытый показатель d отсутствует или некорректен.");
        }

        if (document.SignatureValues.Count != hash.Length)
        {
            return false;
        }

        for (var i = 0; i < hash.Length; i++)
        {
            var signatureTuple = document.SignatureValues[i];
            if (signatureTuple is null || signatureTuple.Count != 1 ||
                !BigInteger.TryParse(signatureTuple[0], out var signature) ||
                signature < 0 || signature >= n)
            {
                return false;
            }

            var verifiedHashByte = _modularExponentiation.Calculate(signature, d, n);
            if (verifiedHashByte != hash[i])
            {
                return false;
            }
        }

        return true;
    }

    private BigInteger GenerateD(
        BigInteger phi,
        IRandomNumberGenerator randomNumberGenerator)
    {
        while (true)
        {
            var d = randomNumberGenerator.Generate(2, phi - 1);
            if (_extendedEuclideanAlgorithm.Calculate(d, phi).Gcd == 1)
            {
                return d;
            }
        }
    }

    private BigInteger ModularInverse(BigInteger value, BigInteger modulus)
    {
        var result = _extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException("Обратный элемент по модулю φ не существует.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }

    private void ValidatePrime(BigInteger value, string name)
    {
        if (value <= 3 || !_primalityTest.IsProbablyPrime(value, 40))
        {
            throw new ArgumentException("P и Q должны быть простыми числами больше 3.", name);
        }
    }

    private static void EnsureHashBytesFit(IEnumerable<byte> hash, BigInteger n)
    {
        if (n <= byte.MaxValue || hash.Any(value => new BigInteger(value) >= n))
        {
            throw new ArgumentException("N должно быть больше максимального значения байта хеша.");
        }
    }
}
