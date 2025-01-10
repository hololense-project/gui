using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class MeshScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float dwellTime = 2.0f;
    [SerializeField] private GameObject gazeCursor;
    [SerializeField] private MeshExporter meshExporter;
    [SerializeField] private TextMesh logText;


    [Header("Server Settings")]
    [SerializeField] private SendServer sendServer;


    private bool isScanning = true; 

    // Dane skanowanego obiektu
    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    
    // Bufory na wierzchołki i trójkąty
    private List<Vector3> scannedVertices = new List<Vector3>();
    private List<int> scannedTriangles = new List<int>();
    private Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();
    private HashSet<string> scannedTriangleSet = new HashSet<string>();

    // Parametry wysyłania danych (przykładowe, obecnie nieużywane)
    private float sendInterval = 10.0f; 
    private float sendTimer = 0.0f;

    // Auto-eksport co zadany interwał
    [SerializeField] private float autoExportInterval = 0.5f; // co 0.5 sekundy
    private Coroutine autoExportCoroutine;

    // Licznik plików OBJ
    private int autoExportCounter = 0;

    private void Start()
    {
        // Domyślnie uruchamiamy skanowanie (i tym samym auto-eksport)
        SetScanningMode(true);
    }

    private void Update()
    {
        if (!isScanning) return;

        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Wykonujemy Raycast na warstwę wykrywania
        if (Physics.Raycast(gazeRay, out RaycastHit hit, Mathf.Infinity, detectionLayer))
        {
            currentTarget = hit.collider.gameObject;
            MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                // Gdy patrzymy na nowy obiekt, reset timera
                if (currentTarget != previousTarget)
                {
                    gazeTimer = 0.0f;
                    previousTarget = currentTarget;
                }

                gazeTimer += Time.deltaTime;

                // Po upływie dwellTime skanujemy
                if (gazeTimer >= dwellTime)
                {
                    ScanMesh(meshFilter, hit.point);
                    gazeTimer = 0.0f;
                }

                // Aktualizacja pozycji kursora
                UpdateGazeCursor(hit.point);
            }
        }
        else
        {
            // Jeśli Raycast nie trafił w obiekt
            currentTarget = null;
            gazeTimer = 0.0f;
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }

        // Przykładowe wysyłanie co 10 sek. (obecnie zakomentowane)
        // sendTimer += Time.deltaTime;
        // if (sendTimer >= sendInterval)
        // {
        //     sendTimer = 0f;
        //     // SendScannedVerticesToServer();
        // }
    }

    /// <summary>
    /// Ustawia tryb skanowania – jeśli aktywny, uruchamiamy/skracamy auto-eksport.
    /// </summary>
    public void SetScanningMode(bool isScanning)
    {
        this.isScanning = isScanning;

        if (isScanning && autoExportCoroutine == null)
        {
            autoExportCoroutine = StartCoroutine(AutoExportRoutine());
        }
        else if (!isScanning && autoExportCoroutine != null)
        {
            StopCoroutine(autoExportCoroutine);
            autoExportCoroutine = null;
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

        Debug.Log($"Scanning mesh with {vertices.Length} vertices.");

        // Zbieramy wierzchołki w pobliżu hitPoint
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexIndexMap.ContainsKey(i))
                continue;

            Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);
            float distance = Vector3.Distance(worldVertex, hitPoint);

            // Przykładowo skanujemy, gdy dystans < 5f
            if (distance < 5.0f)
            {
                vertexIndexMap[i] = scannedVertices.Count;
                scannedVertices.Add(vertices[i]);
                Debug.Log($"Vertex added at distance {distance}: {worldVertex}");
            }
        }

        // Zbieramy trójkąty z zeskanowanych wierzchołków
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

                int[] sortedIndices = new int[] { newIndex0, newIndex1, newIndex2 };
                Array.Sort(sortedIndices);

                string triangleKey = $"{sortedIndices[0]}_{sortedIndices[1]}_{sortedIndices[2]}";

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

    public async void ExportScannedMesh(string customFileName = null)
    {
        if (scannedVertices.Count == 0)
        {
            Debug.LogError("No vertices scanned for export.");
            return;
        }

        // Domyślna nazwa, jeśli nic nie podano
        string fileName = string.IsNullOrEmpty(customFileName)
            ? "scanned_mesh.obj"
            : customFileName;

        Debug.Log($"Exporting to file: {fileName}");
        logText.text = "Exporting scanned mesh...";
        _ = meshExporter.ExportMeshToObjAsync(scannedVertices, scannedTriangles, fileName);

        var scannedData = new ScannedMeshData(scannedVertices, scannedTriangles);
        string jsonData = JsonUtility.ToJson(scannedData);

        // Wysyłanie danych na serwer
        if (sendServer != null)
        {
            await sendServer.Send(jsonData);
        }
        else
        {
            Debug.LogError("SendServer reference is not set in MeshScanner.");
        }


        // Czyścimy listy, jeśli chcesz zbierać od nowa
        scannedVertices.Clear();
        scannedTriangles.Clear();
        vertexIndexMap.Clear();
        scannedTriangleSet.Clear();
    }

    /// <summary>
    /// Eksport automatyczny – generuje nazwę z licznikiem.
    /// </summary>
    private void ExportScannedMeshAuto()
    {
        string fileName = $"mesh_{autoExportCounter++}.obj";
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
}
