using System.Numerics;
using _1;

namespace _3;

public sealed class DiffieHellmanAlgorithm : IDiffieHellmanAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IPrimalityTest _primalityTest;

    public DiffieHellmanAlgorithm(
        IModularExponentiation modularExponentiation,
        IPrimalityTest primalityTest)
    {
        _modularExponentiation = modularExponentiation;
        _primalityTest = primalityTest;
    }

    public DiffieHellmanResult Calculate(
        BigInteger p,
        BigInteger g,
        BigInteger alicePrivateKey,
        BigInteger bobPrivateKey)
    {
        Validate(p, g, alicePrivateKey, bobPrivateKey);

        var alicePublicKey = _modularExponentiation.Calculate(g, alicePrivateKey, p);
        var bobPublicKey = _modularExponentiation.Calculate(g, bobPrivateKey, p);
        var aliceSharedKey = _modularExponentiation.Calculate(bobPublicKey, alicePrivateKey, p);
        var bobSharedKey = _modularExponentiation.Calculate(alicePublicKey, bobPrivateKey, p);

        return new DiffieHellmanResult(
            p,
            g,
            alicePrivateKey,
            bobPrivateKey,
            alicePublicKey,
            bobPublicKey,
            aliceSharedKey,
            bobSharedKey);
    }

    private void Validate(
        BigInteger p,
        BigInteger g,
        BigInteger alicePrivateKey,
        BigInteger bobPrivateKey)
    {
        if (p <= 2 || !_primalityTest.IsProbablyPrime(p, 20))
        {
            throw new ArgumentException("p должно быть простым числом больше 2.", nameof(p));
        }

        if (g <= 1 || g >= p)
        {
            throw new ArgumentOutOfRangeException(nameof(g), "g должно удовлетворять 1 < g < p.");
        }

        if (alicePrivateKey <= 0 || alicePrivateKey >= p - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(alicePrivateKey), "Секретный ключ Алисы должен удовлетворять 1 <= Xa < p - 1.");
        }

        if (bobPrivateKey <= 0 || bobPrivateKey >= p - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(bobPrivateKey), "Секретный ключ Боба должен удовлетворять 1 <= Xb < p - 1.");
        }
    }
}
