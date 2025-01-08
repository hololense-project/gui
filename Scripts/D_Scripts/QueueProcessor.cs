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

            List<Vector3> vertices = new List<Vector3>(meshFilter.sharedMesh.vertices);
            List<int> triangles = new List<int>(meshFilter.sharedMesh.triangles);
            MeshExporter meshExporter = new MeshExporter();
            await meshExporter.ExportMeshToObjAsync(vertices, triangles, filePath);
            await Compressor.CompressFileAsync(filePath, compressedPath);

            Uploader uploader = gameObject.AddComponent<Uploader>();
            StartCoroutine(uploader.UploadFileAsync(compressedPath));
        }

        isProcessing = false;
    }
}