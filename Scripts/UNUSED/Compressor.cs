using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

public class Compressor
{
    public static async Task CompressFileAsync(string inputFilePath, string outputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            Debug.LogError($"Input file does not exist: {inputFilePath}");
            return;
        }

        using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
        using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
        using (GZipStream compressionStream = new GZipStream(outputFileStream, CompressionMode.Compress))
        {
            await inputFileStream.CopyToAsync(compressionStream);
        }

        Debug.Log($"File compressed to {outputFilePath}");
    }
}
