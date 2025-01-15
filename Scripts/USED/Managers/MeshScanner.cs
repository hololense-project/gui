using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;

public class MeshScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float dwellTime = 2.0f;
    [SerializeField] private GameObject gazeCursor;
    [SerializeField] private MeshExporter meshExporter;
    [SerializeField] private MeshManager meshManager;
    [SerializeField] private TextMesh logText;
    [SerializeField] private float autoExportInterval = 0.5f; // Every 0.5 seconds

    [Header("Server Settings")]
    [SerializeField] public ServerWebRTC serverWebRTC;

    private bool isScanning = true;

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;

    private List<Vector3> scannedVertices = new List<Vector3>();
    private List<int> scannedTriangles = new List<int>();
    private Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();
    private HashSet<string> scannedTriangleSet = new HashSet<string>();
    private Queue<string> uploadQueue = new Queue<string>();
    private bool isUploading = false;
    private readonly object scanDataLock = new object();
    private Queue<GameObject> vertexPool = new Queue<GameObject>();

    private Coroutine autoExportCoroutine;

    private int autoExportCounter = 0;

    private void Start()
    {
        if (isScanning && autoExportCoroutine == null)
        {
            autoExportCoroutine = StartCoroutine(AutoExportRoutine());
        }

        SetScanningMode(true);
        scannedVertices.Capacity = 5000;
        scannedTriangles.Capacity = 15000;
    }

    private float scanCheckInterval = 0.1f; // Scanning every 0.1 seconds
    private float lastScanCheckTime = 0.0f;

    private void Update()
    {
        if (!isScanning || Time.time - lastScanCheckTime < scanCheckInterval) return;
        lastScanCheckTime = Time.time;

        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(gazeRay, Mathf.Infinity, detectionLayer);

        if (hits.Length > 0)
        {
            // Find the closest hit
            RaycastHit closestHit = hits[0];
            foreach (var hit in hits)
            {
                if (hit.distance < closestHit.distance)
                    closestHit = hit;
            }

            ProcessHit(closestHit);
        }
        else
        {
            currentTarget = null;
            gazeTimer = 0.0f;
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }
    }

    private void ProcessHit(RaycastHit hit)
    {
        currentTarget = hit.collider.gameObject;
        MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();

        if (meshFilter == null) return;

        if (currentTarget != previousTarget)
        {
            gazeTimer = 0.0f;
            previousTarget = currentTarget;
        }

        gazeTimer += Time.deltaTime;

        if (gazeTimer >= dwellTime)
        {
            ScanMesh(meshFilter, hit.point);
            gazeTimer = 0.0f;
        }

        UpdateGazeCursor(hit.point);
    }

    public void SetScanningMode(bool isScanning)
    {
        this.isScanning = isScanning;

        if (isScanning)
        {
            if (autoExportCoroutine == null)
            {
                autoExportCoroutine = StartCoroutine(AutoExportRoutine());
            }

            if (meshManager != null)
            {
                meshManager.HideMesh();
                meshManager.EnableMeshObserver();
            }
        }
        else
        {
            if (autoExportCoroutine != null)
            {
                StopCoroutine(autoExportCoroutine);
                autoExportCoroutine = null;
            }

            scannedVertices.Clear();
            scannedTriangles.Clear();
            vertexIndexMap.Clear();
            scannedTriangleSet.Clear();

            if (meshManager != null)
            {
                meshManager.DisableMeshObserver();
            }
        }
    }

    private void ScanMesh(MeshFilter meshFilter, Vector3 hitPoint)
    {
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;
        Transform meshTransform = meshFilter.transform;

        if (vertices.Length == 0 || indices.Length == 0)
        {
            Debug.LogError("Mesh has no vertices or triangles!");
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexIndexMap.ContainsKey(i))
                continue;

            Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);
            float distance = Vector3.Distance(worldVertex, hitPoint);

            if (distance < 5.0f)
            {
                vertexIndexMap[i] = scannedVertices.Count;
                scannedVertices.Add(vertices[i]);
                VisualizeVertex(worldVertex);
            }
        }

        for (int i = 0; i < indices.Length; i += 3)
        {
            int index0 = indices[i];
            int index1 = indices[i + 1];
            int index2 = indices[i + 2];

            if (vertexIndexMap.ContainsKey(index0) &&
                vertexIndexMap.ContainsKey(index1) &&
                vertexIndexMap.ContainsKey(index2))
            {
                int newIndex0 = vertexIndexMap[index0];
                int newIndex1 = vertexIndexMap[index1];
                int newIndex2 = vertexIndexMap[index2];

                int minIndex = Mathf.Min(newIndex0, newIndex1, newIndex2);
                int maxIndex = Mathf.Max(newIndex0, newIndex1, newIndex2);
                int midIndex = newIndex0 + newIndex1 + newIndex2 - minIndex - maxIndex;
                string triangleKey = $"{minIndex}_{midIndex}_{maxIndex}";

                if (!scannedTriangleSet.Contains(triangleKey))
                {
                    scannedTriangleSet.Add(triangleKey);
                    scannedTriangles.Add(newIndex0);
                    scannedTriangles.Add(newIndex1);
                    scannedTriangles.Add(newIndex2);
                }
            }
        }

        Renderer rend = meshFilter.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green;
        }
    }

    public void ExportScannedMesh(string customFileName = null)
    {
        if (scannedVertices.Count == 0)
        {
            Debug.LogError("No vertices scanned for export.");
            return;
        }

        List<Vector3> verticesCopy;
        List<int> trianglesCopy;

        lock (scanDataLock)
        {
            verticesCopy = new List<Vector3>(scannedVertices);
            trianglesCopy = new List<int>(scannedTriangles);
        }

        string fileName = string.IsNullOrEmpty(customFileName)
            ? "scanned_mesh.obj"
            : customFileName;

        Debug.Log($"Exporting to file: {fileName}");
        logText.text = "Exporting scanned mesh...";

        _ = meshExporter.ExportMeshToObjAsync(verticesCopy, trianglesCopy, fileName);

        EnqueueMeshDataForUpload(verticesCopy, trianglesCopy);
    }

    private void EnqueueMeshDataForUpload(List<Vector3> vertices, List<int> triangles)
    {
        string objData = meshExporter.GenerateObjData(vertices, triangles);
        uploadQueue.Enqueue(objData);

        if (!isUploading)
        {
            StartCoroutine(ProcessUploadQueue());
        }
    }

    private IEnumerator ProcessUploadQueue()
    {
        if (isUploading) yield break;
        isUploading = true;

        while (uploadQueue.Count > 0)
        {
            string objData = uploadQueue.Dequeue();

            if (serverWebRTC != null)
            {
                bool success = false;
                while (!success)
                {
                    var sendTask = serverWebRTC.Send(objData);
                    yield return new WaitUntil(() => sendTask.IsCompleted);

                    if (sendTask.Exception == null)
                    {
                        Debug.Log("Data successfully sent to server.");
                        Debug.Log(objData);
                        success = true;
                    }
                    else
                    {
                        Debug.LogError($"Failed to send data: {sendTask.Exception.InnerException?.Message}. Retrying...");

                        uploadQueue.Enqueue(objData);

                        if (uploadQueue.Count > 10)
                        {
                            Debug.LogError("Too many failed uploads, aborting.");
                            break;
                        }

                        yield return new WaitForSeconds(5);
                    }
                }
            }
            else
            {
                Debug.LogError("ServerWebRTC reference is not set in MeshScanner.");
            }

            yield return null;
        }

        isUploading = false;
    }

    private void ExportScannedMeshAuto()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "exported_meshes");
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        string fileName = $"{directoryPath}/mesh_{DateTime.Now:yyyyMMdd_HHmmss}_{autoExportCounter++}.obj";
        ExportScannedMesh(fileName);
    }

    private IEnumerator AutoExportRoutine()
    {
        while (isScanning)
        {
            yield return new WaitForSeconds(autoExportInterval);
            ExportScannedMeshAuto();
        }

        autoExportCoroutine = null;
    }

    private void UpdateGazeCursor(Vector3 position)
    {
        if (gazeCursor != null)
        {
            gazeCursor.transform.position = position;
        }
    }

    [Serializable]
    public class ScannedMeshData
    {
        public List<Vector3> vertices;
        public List<int> triangles;

        public ScannedMeshData(List<Vector3> vertices, List<int> triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }

    public bool IsScanning()
    {
        return isScanning;
    }

    private void VisualizeVertex(Vector3 position)
    {
        GameObject sphere;
        if (vertexPool.Count > 0)
        {
            sphere = vertexPool.Dequeue();
        }
        else
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * 0.05f;
            sphere.GetComponent<Renderer>().material.color = Color.red;
        }

        sphere.transform.position = position;
        sphere.GetComponent<Renderer>().enabled = true;
        StartCoroutine(HideSphere(sphere, 5.0f));
    }

    private IEnumerator HideSphere(GameObject sphere, float delay)
    {
        yield return new WaitForSeconds(delay);
        sphere.GetComponent<Renderer>().enabled = false;
        vertexPool.Enqueue(sphere);
    }
}