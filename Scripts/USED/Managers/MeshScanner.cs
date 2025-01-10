using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Threading.Tasks;

public class MeshScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float dwellTime = 2.0f;
    [SerializeField] private GameObject gazeCursor;
    [SerializeField] private MeshExporter meshExporter;
    [SerializeField] private MeshManager meshManager;
    [SerializeField] private TextMesh logText;
    [SerializeField] private float autoExportInterval = 0.5f; // co 0.5 sekundy


    [Header("Server Settings")]
    [SerializeField] private RunServer runServer;

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

    private float scanCheckInterval = 0.1f; // Skanowanie co 0.1 sekundy a nie co klatkę
    private float lastScanCheckTime = 0.0f;

    private void Update()
    {
        if (!isScanning || Time.time - lastScanCheckTime < scanCheckInterval) return;
        lastScanCheckTime = Time.time;

        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(gazeRay, Mathf.Infinity, detectionLayer);

        if (hits.Length > 0)
        {
            // Znajdź najbliższy obiekt (unikamy problemów z trafieniem w niechciane obiekty)
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
            // Jeśli Raycast nie trafił w żaden obiekt
            currentTarget = null;
            gazeTimer = 0.0f;
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }
    }

    private void ProcessHit(RaycastHit hit)
    {
        currentTarget = hit.collider.gameObject;
        MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();

        if (meshFilter == null) return; // Brak siatki - nic nie robimy

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

        // Aktualizacja pozycji kursora
        UpdateGazeCursor(hit.point);
    }

    /// <summary>
    /// Ustawia tryb skanowania – jeśli aktywny, uruchamiamy/skracamy auto-eksport.
    /// </summary>
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

    /// <summary>
    /// Główna metoda skanowania siatki obiektu.
    /// </summary>
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

        Debug.Log($"Scanning mesh with {vertices.Length} vertices.");

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexIndexMap.ContainsKey(i))
                continue;

            Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);
            //////////////////// WIZUALIZACJA WIERZCHOŁKÓW ?????????????????????
            VisualizeVertex(worldVertex); 
            float distance = Vector3.Distance(worldVertex, hitPoint);

            if (distance < 5.0f)
            {
                vertexIndexMap[i] = scannedVertices.Count;
                scannedVertices.Add(vertices[i]);
                Debug.Log($"Vertex added at distance {distance}: {worldVertex}");
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

                // Lepszy algorytm sortowania indeksów :)
                int minIndex = Mathf.Min(newIndex0, newIndex1, newIndex2);
                int maxIndex = Mathf.Max(newIndex0, newIndex1, newIndex2);
                int midIndex = newIndex0 + newIndex1 + newIndex2 - minIndex - maxIndex;
                string triangleKey = $"{minIndex}_{midIndex}_{maxIndex}";

                // int[] sortedIndices = new int[] { newIndex0, newIndex1, newIndex2 };
                // Array.Sort(sortedIndices);
                // string triangleKey = $"{sortedIndices[0]}_{sortedIndices[1]}_{sortedIndices[2]}";

                if (!scannedTriangleSet.Contains(triangleKey))
                {
                    scannedTriangleSet.Add(triangleKey);
                    scannedTriangles.Add(newIndex0);
                    scannedTriangles.Add(newIndex1);
                    scannedTriangles.Add(newIndex2);
                }
            }
        }

        Debug.Log($"Scanned {vertexIndexMap.Count} vertices and updated triangles.");

        // Oznaczenie wizualne zeskanowanego obiektu
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

        lock (scanDataLock) // Blokujemy dostęp do danych na czas kopiowania
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

    private async void EnqueueMeshDataForUpload(List<Vector3> vertices, List<int> triangles)
    {
        var scannedData = new ScannedMeshData(vertices, triangles);
        string jsonData = JsonUtility.ToJson(scannedData);

        // Zapisz JSON do kolejki zamiast wysyłać natychmiast
        uploadQueue.Enqueue(jsonData);
        
        // Jeśli kolejka nie jest zajęta, zacznij wysyłanie
        if (!isUploading)
        {
            await ProcessUploadQueue();
        }
    }

    private async Task ProcessUploadQueue()
    {
        if (isUploading) return;
        isUploading = true;

        while (uploadQueue.Count > 0)
        {
            string jsonData = uploadQueue.Dequeue();

            if (runServer != null)
            {
                bool uploadSuccess = await Task.Run(async () => 
                {
                    try
                    {
                        await runServer.PostDataToServer(jsonData);
                        Debug.Log("Data successfully sent to server.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to send data: {ex.Message}. Retrying...");
                        return false;
                    }
                });

                if (!uploadSuccess)
                {
                    uploadQueue.Enqueue(jsonData);
                    if (uploadQueue.Count > 10)
                    {
                        Debug.LogError("Too many failed uploads, aborting.");
                        break;
                    }

                    await Task.Delay(5000);
                }
            }
            else
            {
                Debug.LogError("runServer reference is not set in MeshScanner.");
            }
        }

        isUploading = false;
    }

    /// <summary>
    /// Eksport automatyczny – generuje nazwę z licznikiem.
    /// </summary>
    private void ExportScannedMeshAuto()
    {
        string fileName = $"mesh_{DateTime.Now:yyyyMMdd_HHmmss}_{autoExportCounter++}.obj";
        ExportScannedMesh(fileName);
    }

    /// <summary>
    /// Coroutine wywołująca auto-eksport co autoExportInterval sekund.
    /// </summary>
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
        sphere.GetComponent<Renderer>().enabled = true; // Włącz widoczność
        StartCoroutine(HideSphere(sphere, 5.0f));
    }

    private IEnumerator HideSphere(GameObject sphere, float delay)
    {
        yield return new WaitForSeconds(delay);
        sphere.GetComponent<Renderer>().enabled = false; // Ukryj zamiast wyłączać
        vertexPool.Enqueue(sphere);
    }

}
