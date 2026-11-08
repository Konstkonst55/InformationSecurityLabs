using System.Numerics;
using SignatureShared;
using _1;

namespace _9;

public sealed class ElGamalSignatureAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;

    public ElGamalSignatureAlgorithm(
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

    public SignatureDocument SignHash(
        byte[] hash,
        string originalFileName,
        ElGamalSignatureKeyPair keyPair)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(keyPair);

        var publicKey = keyPair.PublicKey;
        ValidatePublicKey(publicKey);
        if (keyPair.X <= 1 || keyPair.X >= publicKey.P - 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(keyPair),
                "Секретный ключ x должен удовлетворять условию 1 < x < P - 1.");
        }

        var signatureValues = new List<List<string>>(hash.Length);
        foreach (var hashByte in hash)
        {
            var h = GetHashRepresentative(hashByte, publicKey.P);
            var (r, s) = SignHashRepresentative(h, keyPair);
            signatureValues.Add(new List<string>
            {
                r.ToString(),
                s.ToString()
            });
        }

        return new SignatureDocument
        {
            Algorithm = "ElGamal",
            HashAlgorithm = "SHA-256",
            OriginalFileName = Path.GetFileName(originalFileName),
            HashHex = Convert.ToHexString(hash),
            PublicParameters = new Dictionary<string, string>
            {
                ["P"] = publicKey.P.ToString(),
                ["Q"] = publicKey.Q.ToString(),
                ["g"] = publicKey.G.ToString(),
                ["y"] = publicKey.Y.ToString()
            },
            SignatureValues = signatureValues
        };
    }

    public bool VerifyHash(byte[] hash, SignatureDocument document)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(document);

        if (!string.Equals(document.Algorithm, "ElGamal", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(document.HashAlgorithm, "SHA-256", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Алгоритм подписи или хеширования не поддерживается.");
        }

        var publicKey = ReadPublicKey(document.PublicParameters);
        ValidatePublicKey(publicKey);

        if (document.SignatureValues.Count != hash.Length)
        {
            return false;
        }

        for (var i = 0; i < hash.Length; i++)
        {
            var values = document.SignatureValues[i];
            if (values is null || values.Count != 2 ||
                !BigInteger.TryParse(values[0], out var r) ||
                !BigInteger.TryParse(values[1], out var s))
            {
                return false;
            }

            if (r <= 0 || r >= publicKey.P || s < 0 || s >= publicKey.P - 1)
            {
                return false;
            }

            var h = GetHashRepresentative(hash[i], publicKey.P);
            var leftFirst = _modularExponentiation.Calculate(publicKey.Y, r, publicKey.P);
            var leftSecond = _modularExponentiation.Calculate(r, s, publicKey.P);
            var left = leftFirst * leftSecond % publicKey.P;
            var right = _modularExponentiation.Calculate(publicKey.G, h, publicKey.P);

            if (left != right)
            {
                return false;
            }
        }

        return true;
    }

    private (BigInteger R, BigInteger S) SignHashRepresentative(
        BigInteger h,
        ElGamalSignatureKeyPair keyPair)
    {
        var p = keyPair.PublicKey.P;
        var pMinusOne = p - 1;

        while (true)
        {
            var k = _randomNumberGenerator.Generate(2, p - 2);
            if (_extendedEuclideanAlgorithm.Calculate(k, pMinusOne).Gcd != 1)
            {
                continue;
            }

            var r = _modularExponentiation.Calculate(
                keyPair.PublicKey.G,
                k,
                p);
            var kInverse = ModularInverse(k, pMinusOne);
            var u = Normalize(h - keyPair.X * r, pMinusOne);
            var s = kInverse * u % pMinusOne;

            return (r, s);
        }
    }

    private void ValidatePublicKey(ElGamalSignaturePublicKey publicKey)
    {
        if (publicKey.P <= 257 || !_primalityTest.IsProbablyPrime(publicKey.P, 40))
        {
            throw new InvalidDataException("P должно быть простым числом больше 257.");
        }

        var q = (publicKey.P - 1) / 2;
        if (publicKey.Q != q || !_primalityTest.IsProbablyPrime(q, 40))
        {
            throw new InvalidDataException("Параметры P и Q не удовлетворяют P = 2Q + 1.");
        }

        if (publicKey.G <= 1 || publicKey.G >= publicKey.P)
        {
            throw new InvalidDataException("Некорректный открытый параметр g.");
        }

        var gSquared = _modularExponentiation.Calculate(publicKey.G, 2, publicKey.P);
        var gToQ = _modularExponentiation.Calculate(publicKey.G, q, publicKey.P);
        if (gSquared == 1 || gToQ == 1)
        {
            throw new InvalidDataException("g не является первообразным корнем по модулю P.");
        }

        if (publicKey.Y <= 1 || publicKey.Y >= publicKey.P)
        {
            throw new InvalidDataException("Некорректный открытый ключ y.");
        }
    }

    private static ElGamalSignaturePublicKey ReadPublicKey(
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!TryRead(parameters, "P", out var p) ||
            !TryRead(parameters, "Q", out var q) ||
            !TryRead(parameters, "g", out var g) ||
            !TryRead(parameters, "y", out var y))
        {
            throw new InvalidDataException(
                "Файл подписи не содержит все открытые параметры P, Q, g, y.");
        }

        return new ElGamalSignaturePublicKey(p, q, g, y);
    }

    private static bool TryRead(
        IReadOnlyDictionary<string, string> values,
        string key,
        out BigInteger result)
    {
        result = BigInteger.Zero;
        return values.TryGetValue(key, out var value) &&
               BigInteger.TryParse(value, out result);
    }

    private static BigInteger GetHashRepresentative(byte hashByte, BigInteger p)
    {
        var h = new BigInteger(hashByte) + 2;
        if (h <= 1 || h >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(p),
                "Для байтового представления SHA-256 требуется P > 257.");
        }

        return h;
    }

    private BigInteger ModularInverse(BigInteger value, BigInteger modulus)
    {
        var result = _extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException(
                "Обратный элемент k^(-1) по модулю P - 1 не существует.");
        }

        return Normalize(result.X, modulus);
    }

    private static BigInteger Normalize(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
