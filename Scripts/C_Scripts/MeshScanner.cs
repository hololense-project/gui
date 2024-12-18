using UnityEngine;
using System.Collections.Generic;

public class MeshScanner : MonoBehaviour
{
    public LayerMask detectionLayer;
    public float dwellTime = 2.0f;
    public GameObject gazeCursor;
    public MeshExporter meshExporter;

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private bool isScanning = true; // Automatyczne rozpoczęcie skanowania
    private HashSet<Vector3> scannedVertices = new HashSet<Vector3>();

    void Start()
    {
        // Automatyczne rozpoczęcie skanowania
        SetScanningMode(true);
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
    }

    public void SetScanningMode(bool isScanning)
    {
        this.isScanning = isScanning;
    }

    private void ScanMesh(MeshFilter meshFilter, Vector3 hitPoint)
    {
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Transform meshTransform = meshFilter.transform;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldVertex = meshTransform.TransformPoint(vertex);
            if (Vector3.Distance(worldVertex, hitPoint) < 0.1f)
            {
                scannedVertices.Add(vertex);
            }
        }

        // Highlight the scanned area
        meshFilter.GetComponent<Renderer>().material.color = Color.green;
    }

    public void ExportScannedMesh()
    {
        if (scannedVertices.Count == 0)
        {
            Debug.LogError("No vertices scanned for export.");
            return;
        }

        // Assuming you have a method to get the triangles from the scanned mesh
        List<int> scannedTriangles = GetScannedTriangles();

        _ = meshExporter.ExportMeshToObjAsync(new List<Vector3>(scannedVertices), scannedTriangles, "scanned_mesh.obj");
        scannedVertices.Clear();
    }

    private void UpdateGazeCursor(Vector3 position)
    {
        if (gazeCursor != null)
        {
            gazeCursor.transform.position = position;
        }
    }

    // Example method to get triangles (you need to implement this based on your mesh data)
    private List<int> GetScannedTriangles()
    {
        if (currentTarget == null) return new List<int>();

        MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();
        if (meshFilter == null) return new List<int>();
    
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Transform meshTransform = meshFilter.transform;
        HashSet<int> scannedIndices = new HashSet<int>();

        // Map scanned vertices to indices in the mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);
            if (scannedVertices.Contains(worldVertex))
            {
                scannedIndices.Add(i);
            }
        }

        // Add all triangles containing scanned indices
        List<int> scannedTriangles = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (scannedIndices.Contains(triangles[i]) ||
                scannedIndices.Contains(triangles[i + 1]) ||
                scannedIndices.Contains(triangles[i + 2]))
            {
                scannedTriangles.Add(triangles[i]);
                scannedTriangles.Add(triangles[i + 1]);
                scannedTriangles.Add(triangles[i + 2]);
            }
        }

        return scannedTriangles;
    }
}
