// U�ycie:
// Do��cz skrypt do GameObject w scenie.
// Zainicjuj kompresj� tekstury

// void Start()
// {
//     // Zak�adamy, �e masz tekstur� zdefiniowan� wcze�niej
//     Texture2D myTexture = new Texture2D(512, 512);

//     // Uruchamiamy proces kompresji
//     StartCompression(myTexture);
// }

// Dzia�anie:
// - Kompresuje tekstur� do formatu PNG lub JPEG.
// - Nast�pnie kompresuje plik do GZip.
// - Zapisuje wynikowy plik .gz w katalogu projektu.

// =========================================================================================


using System.IO;
using System.IO.Compression;
using UnityEngine;

public class FullCompress : MonoBehaviour
{
    // Metoda do kompresji tekstury
    public void CompressAndSaveTexture(Texture2D texture, string textureSavePath, bool useJPEG = true)
    {
        // Kompresja tekstury do formatu JPEG lub PNG
        byte[] compressedBytes = useJPEG ? texture.EncodeToJPG() : texture.EncodeToPNG();

        // Zapis skompresowanej tekstury do pliku
        File.WriteAllBytes(textureSavePath, compressedBytes);

        // Nast�pnie wywo�ujemy kompresj� GZip na zapisanym pliku
        string compressedFilePath = textureSavePath + ".gz";
        CompressFile(textureSavePath, compressedFilePath);

        Debug.Log("Texture compressed and saved to: " + compressedFilePath);
    }

    // Metoda do kompresji pliku (GZip)
    public void CompressFile(string filePath, string compressedFilePath)
    {
        // Sprawdzamy, czy plik istnieje
        if (File.Exists(filePath))
        {
            using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Create))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }

            Debug.Log("File compressed successfully: " + compressedFilePath);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    // Metoda do inicjacji kompresji i zapisania tekstury
    public void StartCompression(Texture2D texture)
    {
        string savePath = Application.dataPath + "/CompressedTextures/texture.png";
        CompressAndSaveTexture(texture, savePath);
    }
}