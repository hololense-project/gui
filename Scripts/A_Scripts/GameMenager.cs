using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cube;
    public string filePath = "Assets/";
    private FileUploader fileUploader;

    private void Start()
    {
        fileUploader = GetComponent<FileUploader>();
    }

    public void ExportAndUpload()
    {
        if (cube != null)
        {
            OBJExporter objExporter = new OBJExporter();
            objExporter.ExportToObj(cube, filePath);

            FullCompress compressor = new FullCompress();
            string compressedFilePath = filePath + ".gz";
            compressor.CompressFile(filePath, compressedFilePath);

            if (fileUploader != null)
            {
                StartCoroutine(fileUploader.UploadFileAsync(compressedFilePath));
            }
            else
            {
                Debug.LogError("FileUploader component not found on the GameObject.");
            }
        }
        else
        {
            Debug.LogError("Cube object not assigned.");
        }
    }
}
