using UnityEngine;
using System.Collections.Generic;
using System;

public class MeshScanner : MonoBehaviour
{
    public LayerMask detectionLayer;
    public float dwellTime = 2.0f;
    public GameObject gazeCursor;
    public MeshExporter meshExporter;

    // Reference to SendServer
    public SendServer sendServer;

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private bool isScanning = true; // Automatically start scanning
    private List<Vector3> scannedVertices = new List<Vector3>();
    private List<int> scannedTriangles = new List<int>();
    private Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();
    private HashSet<string> scannedTriangleSet = new HashSet<string>();

    private float sendInterval = 10f; // Interval in seconds to send data
    private float sendTimer = 0f;     // Timer to track sending interval

    void Start()
    {
        // Automatically start scanning
        SetScanningMode(true);

        // Ensure sendServer is assigned
        if (sendServer == null)
        {
            Debug.LogError("SendServer reference is not set in MeshScanner.");
        }
    }

    void Update()
    {
        if (!isScanning) return;

        RaycastHit hit;
        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(gazeRay, out hit, Mathf.Infinity, detectionLayer))
        {
            currentTarget = hit.collider.gameObject;
            MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
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
        }
        else
        {
            currentTarget = null;
            gazeTimer = 0.0f;
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }

        // Send data every 10 seconds
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            SendScannedVerticesToServer();
            sendTimer = 0f; // Reset timer
        }
    }

    public void SetScanningMode(bool isScanning)
    {
        this.isScanning = isScanning;
    }

    private void ScanMesh(MeshFilter meshFilter, Vector3 hitPoint)
    {
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;
        Transform meshTransform = meshFilter.transform;

        Debug.Log($"Scanning mesh with {vertices.Length} vertices.");

        // Loop through all vertices and identify those within the scan radius
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexIndexMap.ContainsKey(i))
            {
                // Vertex already scanned
                continue;
            }

            Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);
            float distance = Vector3.Distance(worldVertex, hitPoint);

            if (distance < 5.0f)
            {
                vertexIndexMap[i] = scannedVertices.Count;
                scannedVertices.Add(vertices[i]);
                Debug.Log($"Vertex added at distance {distance}: {worldVertex}");
            }
        }

        // Collect triangles composed entirely of scanned vertices
        for (int i = 0; i < indices.Length; i += 3)
        {
            int index0 = indices[i];
            int index1 = indices[i + 1];
            int index2 = indices[i + 2];

            if (vertexIndexMap.ContainsKey(index0) &&
                vertexIndexMap.ContainsKey(index1) &&
                vertexIndexMap.ContainsKey(index2))
            {
                // Map original indices to new indices in scannedVertices
                int newIndex0 = vertexIndexMap[index0];
                int newIndex1 = vertexIndexMap[index1];
                int newIndex2 = vertexIndexMap[index2];

                // Create a unique key for the triangle to prevent duplicates
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

        meshFilter.GetComponent<Renderer>().material.color = Color.green;
    }

    public List<Vector3> GetScannedVertices()
    {
        return scannedVertices; // Return collected vertices
    }

    public void ExportScannedMesh()
    {
        if (scannedVertices.Count == 0)
        {
            Debug.LogError("No vertices scanned for export.");
            return;
        }

        _ = meshExporter.ExportMeshToObjAsync(scannedVertices, scannedTriangles, "scanned_mesh.obj");

        scannedVertices.Clear();
        scannedTriangles.Clear();
        vertexIndexMap.Clear();
        scannedTriangleSet.Clear();
    }

    private void UpdateGazeCursor(Vector3 position)
    {
        if (gazeCursor != null)
        {
            gazeCursor.transform.position = position;
        }
    }

    // Sends scanned vertices and triangles to the server every 10 seconds
    private void SendScannedVerticesToServer()
    {
        if (scannedVertices.Count == 0) return;

        // Convert vertices and triangles to JSON format
        //ScannedMeshData data = new ScannedMeshData(scannedVertices, scannedTriangles);
        //string json = JsonUtility.ToJson(data);
        string obj = meshExporter.GenerateObjData(scannedVertices, scannedTriangles);
        // Log the JSON data for debugging
        Debug.Log($"Sending OBJ data to server: {obj}");

        // Send data to the server
        if (sendServer != null)
        {
            sendServer.Send(obj);
            Debug.Log("Vertices and triangles sent to the server.");
        }
        else
        {
            Debug.LogError("SendServer reference is not set in MeshScanner.");
        }
    }

    // Helper class for sending vertices and triangles in JSON format
    [System.Serializable]
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
}