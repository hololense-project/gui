//using UnityEngine;
//using UnityEngine.Networking;
//using System.Collections;
//using System.IO;
//using System.IO.Compression;
//using System.Threading.Tasks;
//using UnityEngine.UI;

//public class ObjectLoader : MonoBehaviour
//{
//    public string serverUrl = "http://127.0.0.1/hololens/objects/";
//    public Transform objectContainer;
//    public GameObject loadingIndicator;
//    public Text errorMessage;

//    public async void LoadObjectFromServer(string objectName)
//    {
//        string url = $"{serverUrl}{objectName}.obj.gz";
//        string filePath = Path.Combine(Application.persistentDataPath, $"{objectName}.obj.gz");
//        string extractedFilePath = Path.Combine(Application.persistentDataPath, $"{objectName}.obj");

//        loadingIndicator.SetActive(true);
//        errorMessage.text = "";

//        bool downloadSuccess = await DownloadFileAsync(url, filePath);
//        if (!downloadSuccess)
//        {
//            errorMessage.text = "Failed to download file.";
//            loadingIndicator.SetActive(false);
//            return;
//        }

//        bool extractSuccess = ExtractGzFile(filePath, extractedFilePath);
//        if (!extractSuccess)
//        {
//            errorMessage.text = "Failed to extract file.";
//            loadingIndicator.SetActive(false);
//            return;
//        }

//        bool loadSuccess = await LoadObjectAsync(extractedFilePath);
//        if (!loadSuccess)
//        {
//            errorMessage.text = "Failed to load object.";
//        }

//        loadingIndicator.SetActive(false);
//    }

//    private async Task<bool> DownloadFileAsync(string url, string filePath)
//    {
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            www.downloadHandler = new DownloadHandlerFile(filePath);
//            var operation = www.SendWebRequest();

//            while (!operation.isDone)
//            {
//                await Task.Yield();
//            }

//            if (www.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"File download failed: {www.error}");
//                return false;
//            }
//        }

//        return true;
//    }

//    private bool ExtractGzFile(string gzFilePath, string outputFilePath)
//    {
//        try
//        {
//            using (FileStream originalFileStream = new FileStream(gzFilePath, FileMode.Open, FileAccess.Read))
//            using (FileStream decompressedFileStream = new FileStream(outputFilePath, FileMode.Create))
//            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
//            {
//                decompressionStream.CopyTo(decompressedFileStream);
//            }

//            return true;
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError($"File extraction failed: {ex.Message}");
//            return false;
//        }
//    }

//    private async Task<bool> LoadObjectAsync(string filePath)
//    {
//        try
//        {
//            string objText = await File.ReadAllTextAsync(filePath);
//            GameObject obj = new OBJLoader().Load(objText);
//            obj.transform.SetParent(objectContainer, false);
//            return true;
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError($"Failed to load object: {ex.Message}");
//            return false;
//        }
//    }
//}