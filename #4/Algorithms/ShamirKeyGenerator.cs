using System.Numerics;
using _1;

namespace _4;

public sealed class ShamirKeyGenerator : IShamirKeyGenerator
{
    private readonly IRandomNumberGenerator _random;
    private readonly IPrimalityTest _primality;
    private readonly IExtendedEuclideanAlgorithm _gcd;

    public ShamirKeyGenerator(
        IRandomNumberGenerator random,
        IPrimalityTest primality,
        IExtendedEuclideanAlgorithm gcd)
    {
        _random = random;
        _primality = primality;
        _gcd = gcd;
    }

    public ShamirKeys Generate(int bitLength = 32, int primalityIterations = 20)
    {
        if (bitLength < 9)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Разрядность p должна быть не меньше 9.");
        }

        var p = _random.GeneratePrime(bitLength, _primality, primalityIterations);
        var phi = p - 1;
        var ca = GenerateC(phi);
        var cb = GenerateC(phi);
        var da = Inverse(ca, phi);
        var db = Inverse(cb, phi);

        return new ShamirKeys(p, ca, da, cb, db);
    }

    public ShamirKeys FromParameters(BigInteger p, BigInteger ca, BigInteger cb)
    {
        if (p <= 257 || !_primality.IsProbablyPrime(p, 20))
        {
            throw new ArgumentException("p должно быть простым числом больше 257.", nameof(p));
        }

        var phi = p - 1;
        ValidateC(ca, phi, nameof(ca));
        ValidateC(cb, phi, nameof(cb));

        return new ShamirKeys(p, ca, Inverse(ca, phi), cb, Inverse(cb, phi));
    }

    private BigInteger GenerateC(BigInteger phi)
    {
        while (true)
        {
            var candidate = _random.Generate(2, phi - 1);
            if (_gcd.Calculate(candidate, phi).Gcd == 1)
            {
                return candidate;
            }
        }
    }

    private void ValidateC(BigInteger c, BigInteger phi, string name)
    {
        if (c <= 1 || c >= phi)
        {
            throw new ArgumentOutOfRangeException(name, "Показатель должен удовлетворять 1 < C < p - 1.");
        }

        if (_gcd.Calculate(c, phi).Gcd != 1)
        {
            throw new ArgumentException("C должно быть взаимно простым с p - 1.", name);
        }
    }

    private BigInteger Inverse(BigInteger value, BigInteger modulus)
    {
        var result = _gcd.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException("Обратный элемент не существует.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }
}
