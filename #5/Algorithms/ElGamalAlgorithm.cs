using System.Numerics;
using _1;

namespace _5;

public sealed class ElGamalAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;

    public ElGamalAlgorithm(
        IModularExponentiation modularExponentiation,
        IRandomNumberGenerator randomNumberGenerator,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        _modularExponentiation = modularExponentiation;
        _randomNumberGenerator = randomNumberGenerator;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
    }

    public IReadOnlyList<BigInteger> EncryptBlock(
        BigInteger message,
        ElGamalPublicParameters parameters)
    {
        ValidateMessage(message, parameters.P);

        var k = _randomNumberGenerator.Generate(1, parameters.P - 2);
        var r = _modularExponentiation.Calculate(
            parameters.G,
            k,
            parameters.P);
        var shared = _modularExponentiation.Calculate(
            parameters.Cb,
            k,
            parameters.P);
        var s = message * shared % parameters.P;

        return new[] { r, s };
    }

    public BigInteger DecryptBlock(
        IReadOnlyList<BigInteger> ciphertext,
        ElGamalPrivateParameters parameters)
    {
        if (ciphertext.Count != 2)
        {
            throw new InvalidDataException(
                "Криптограмма Эль-Гамаля должна содержать два блока.");
        }

        var r = ciphertext[0];
        var s = ciphertext[1];

        if (r <= 0 || r >= parameters.P || s < 0 || s >= parameters.P)
        {
            throw new InvalidDataException(
                "Некорректное значение блока шифротекста.");
        }

        var shared = _modularExponentiation.Calculate(
            r,
            parameters.Db,
            parameters.P);
        var inverse = Inverse(shared, parameters.P);

        return s * inverse % parameters.P;
    }

    private BigInteger Inverse(
        BigInteger value,
        BigInteger modulus)
    {
        var result = _extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException(
                "Обратный элемент не существует.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }

    private static void ValidateMessage(
        BigInteger message,
        BigInteger p)
    {
        if (message < 0 || message >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(message),
                "Сообщение должно удовлетворять условию 0 <= m < p.");
        }
    }
}
