using System.Numerics;

namespace Shared;

public sealed class FileProcessingService : IFileProcessingService
{
    public FileProcessResult Encrypt(
        string inputPath,
        string outputPath,
        int blockWidth,
        int blocksPerByte,
        Func<byte, IReadOnlyList<BigInteger>> transform)
    {
        ValidateArguments(inputPath, outputPath, blockWidth, blocksPerByte, transform);

        long processedBytes = 0;

        using var input = new FileStream(
            inputPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var output = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var buffer = new byte[8192];

        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (var i = 0; i < read; i++)
            {
                var values = transform(buffer[i]);

                if (values.Count != blocksPerByte)
                {
                    throw new InvalidOperationException(
                        "Преобразование вернуло неверное количество блоков.");
                }

                foreach (var value in values)
                {
                    WriteFixed(value, output, blockWidth);
                }

                processedBytes++;
            }
        }

        return new FileProcessResult(
            inputPath,
            outputPath,
            processedBytes);
    }

    public FileProcessResult Decrypt(
        string inputPath,
        string outputPath,
        int blockWidth,
        int blocksPerByte,
        Func<IReadOnlyList<BigInteger>, byte> transform)
    {
        ValidateArguments(inputPath, outputPath, blockWidth, blocksPerByte, transform);

        long processedBytes = 0;

        using var input = new FileStream(
            inputPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var output = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var block = new byte[blockWidth];
        var values = new BigInteger[blocksPerByte];

        while (true)
        {
            var firstRead = ReadExact(input, block);
            if (firstRead == 0)
            {
                break;
            }

            if (firstRead != blockWidth)
            {
                throw new InvalidDataException(
                    "Размер зашифрованного файла не кратен размеру блока.");
            }

            values[0] = ReadBigInteger(block);

            for (var i = 1; i < blocksPerByte; i++)
            {
                var read = ReadExact(input, block);
                if (read != blockWidth)
                {
                    throw new InvalidDataException(
                        "Зашифрованный файл содержит неполный блок.");
                }

                values[i] = ReadBigInteger(block);
            }

            output.WriteByte(transform(values));
            processedBytes++;
        }

        return new FileProcessResult(
            inputPath,
            outputPath,
            processedBytes);
    }

    private static void ValidateArguments(
        string inputPath,
        string outputPath,
        int blockWidth,
        int blocksPerByte,
        Delegate transform)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Путь к исходному файлу не задан.", nameof(inputPath));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Путь к выходному файлу не задан.", nameof(outputPath));
        }

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Исходный файл не найден.", inputPath);
        }

        if (string.Equals(
                Path.GetFullPath(inputPath),
                Path.GetFullPath(outputPath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Исходный и выходной файлы должны различаться.");
        }

        if (blockWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(blockWidth));
        }

        if (blocksPerByte <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(blocksPerByte));
        }

        if (transform is null)
        {
            throw new ArgumentNullException(nameof(transform));
        }

        var parent = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }

    private static int ReadExact(Stream input, byte[] buffer)
    {
        var total = 0;

        while (total < buffer.Length)
        {
            var read = input.Read(
                buffer,
                total,
                buffer.Length - total);

            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

    private static BigInteger ReadBigInteger(byte[] buffer)
    {
        return new BigInteger(
            buffer,
            isUnsigned: true,
            isBigEndian: true);
    }

    private static void WriteFixed(
        BigInteger value,
        Stream output,
        int width)
    {
        if (value < 0)
        {
            throw new InvalidDataException(
                "Нельзя записать отрицательное значение.");
        }

        var bytes = value.ToByteArray(
            isUnsigned: true,
            isBigEndian: true);

        if (bytes.Length > width)
        {
            throw new InvalidDataException(
                "Значение не помещается в фиксированный блок.");
        }

        if (bytes.Length < width)
        {
            var padded = new byte[width];
            Buffer.BlockCopy(
                bytes,
                0,
                padded,
                width - bytes.Length,
                bytes.Length);
            bytes = padded;
        }

        output.Write(bytes, 0, bytes.Length);
    }
}
