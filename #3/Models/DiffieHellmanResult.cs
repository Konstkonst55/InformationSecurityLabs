using System.Numerics;

namespace _3;

public sealed record DiffieHellmanResult(
    BigInteger P,
    BigInteger G,
    BigInteger AlicePrivateKey,
    BigInteger BobPrivateKey,
    BigInteger AlicePublicKey,
    BigInteger BobPublicKey,
    BigInteger AliceSharedKey,
    BigInteger BobSharedKey);
