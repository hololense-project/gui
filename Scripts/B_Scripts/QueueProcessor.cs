using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QueueProcessor : MonoBehaviour 
{
    private Queue<MeshFilter> meshQueue = new Queue<MeshFilter>();
    private bool isProcessing = false;

    public void AddToQueue(MeshFilter meshFilter)
    {
        meshQueue.Enqueue(meshFilter);
    }

    public async Task ProcessQueueAsync()
    {
        if (isProcessing) return;
        isProcessing = true;

        while (meshQueue.Count > 0)
        {
            MeshFilter meshFilter = meshQueue.Dequeue();
            string filePath = Application.persistentDataPath + $"/mesh_{meshFilter.GetInstanceID()}.obj";
            string compressedPath = filePath + ".gz";

            await MeshExporter.ExportMeshToObjAsync(meshFilter, filePath);
            await Compressor.CompressFileAsync(filePath, compressedPath);

            // Przesyłanie pliku
            Uploader uploader = gameObject.AddComponent<Uploader>();
            StartCoroutine(uploader.UploadFileAsync(compressedPath));
        }

        isProcessing = false;
    }
}