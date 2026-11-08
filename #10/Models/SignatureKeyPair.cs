using System.Globalization;
using System.Numerics;

namespace _10.Models;

public sealed record SignatureKeyPair(
    string Algorithm,
    string GeneratorParameterName,
    BigInteger P,
    BigInteger Q,
    BigInteger Generator,
    BigInteger X,
    BigInteger Y)
{
    public Dictionary<string, string> GetPublicParameters()
    {
        return new Dictionary<string, string>
        {
            ["P"] = P.ToString(CultureInfo.InvariantCulture),
            ["Q"] = Q.ToString(CultureInfo.InvariantCulture),
            [GeneratorParameterName] = Generator.ToString(CultureInfo.InvariantCulture),
            ["Y"] = Y.ToString(CultureInfo.InvariantCulture)
        };
    }
}
