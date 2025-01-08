using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class Uploader : MonoBehaviour
{
    private const string ServerIP = "http://127.0.0.1/hololens/upload.php";

    public IEnumerator UploadFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filePath), "application/octet-stream");

        using (UnityWebRequest www = UnityWebRequest.Post(ServerIP, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"File upload failed: {www.error}");
            }
            else
            {
                Debug.Log("File uploaded successfully");
            }
        }
    }
}