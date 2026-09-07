using System.Numerics;

namespace _1;

public sealed class ExtendedEuclideanAlgorithm : IExtendedEuclideanAlgorithm
{
    public ExtendedGcdResult Calculate(BigInteger a, BigInteger b)
    {
        var originalA = a;
        var originalB = b;
        var signA = a < 0 ? -BigInteger.One : BigInteger.One;
        var signB = b < 0 ? -BigInteger.One : BigInteger.One;
        a = BigInteger.Abs(a);
        b = BigInteger.Abs(b);

        var u1 = a;
        var u2 = BigInteger.One;
        var u3 = BigInteger.Zero;
        var v1 = b;
        var v2 = BigInteger.Zero;
        var v3 = BigInteger.One;

        while (v1 != 0)
        {
            var q = u1 / v1;
            var t1 = u1 % v1;
            var t2 = u2 - q * v2;
            var t3 = u3 - q * v3;

            u1 = v1;
            u2 = v2;
            u3 = v3;
            v1 = t1;
            v2 = t2;
            v3 = t3;
        }

        if (originalA == 0 && originalB == 0)
        {
            return new ExtendedGcdResult(BigInteger.Zero, BigInteger.Zero, BigInteger.Zero);
        }

        var x = u2 * signA;
        var y = u3 * signB;
        return new ExtendedGcdResult(u1, x, y);
    }
}
