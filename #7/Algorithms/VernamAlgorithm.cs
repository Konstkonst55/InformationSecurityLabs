namespace _7;

public sealed class VernamAlgorithm
{
    public byte Transform(byte value, byte key)
    {
        return (byte)(value ^ key);
    }
}
