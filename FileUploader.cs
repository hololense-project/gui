// Uzycie:
// Załącz oba skrypty do obiektu:
// - Utwórz nowy obiekt w scenie (np. Empty GameObject).
// - Załącz do niego skrypt FileUploader.

// Ustaw adres serwera: 
// - W oknie "Inspector" dla obiektu, do którego załączyłeś skrypt FileUploader, ustaw odpowiedni adres URL serwera, na który chcesz wysłać plik.

// Uruchom scenę: 
// - StartCoroutine(UploadFileAsync("path/to/your/file.png.gz"));


// Działanie:
// - Skrypt wysyła plik na serwer za pomocą metody POST.
// - Wysyłany plik musi być skompresowany w formacie gzip.

// ==============================================================================

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class FileUploader : MonoBehaviour
{
    private string serverUrl = "https://your-server-url.com/upload";  // Ustaw odpowiedni adres serwera

    public IEnumerator UploadFileAsync(string compressedFilePath)
    {
        byte[] fileData = File.ReadAllBytes(compressedFilePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(compressedFilePath), "application/gzip");

        UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("File upload failed: " + www.error);
        }
        else
        {
            Debug.Log("File uploaded successfully.");
        }

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Connection or Protocol Error: " + www.error);
        }
        else if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("File uploaded successfully.");
        }
    }
}