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
    private List<string> GetScannedTriangles()
    {
        if (currentTarget == null) return new List<string>();

        MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();
        if (meshFilter == null) return new List<string>();

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;

        Transform meshTransform = meshFilter.transform;
        HashSet<Vector3> transformedScannedVertices = new HashSet<Vector3>();

        // Check null for scannedVertices
        if (scannedVertices == null) return new List<string>();

        // Transform and store unique scanned vertices
        foreach (var vertex in vertices)
        {
            Vector3 worldVertex = meshTransform.TransformPoint(vertex);
            if (scannedVertices.Contains(worldVertex))
            {
                transformedScannedVertices.Add(worldVertex);
            }
        }

        // Convert unique vertices to a list
        List<Vector3> vertexList = new List<Vector3>(transformedScannedVertices);
        List<string> result = new List<string>();

        // Add vertices to the result list in "v x y z" format
        foreach (Vector3 vertex in vertexList)
        {
            result.Add($"v {vertex.X} {vertex.Y} {vertex.Z}");
        }

        // Define triangles using vertex positions
        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 v0 = meshTransform.TransformPoint(vertices[indices[i]]);
            Vector3 v1 = meshTransform.TransformPoint(vertices[indices[i + 1]]);
            Vector3 v2 = meshTransform.TransformPoint(vertices[indices[i + 2]]);

            if (transformedScannedVertices.Contains(v0) && transformedScannedVertices.Contains(v1) && transformedScannedVertices.Contains(v2))
            {
                result.Add($"f {v0.X} {v0.Y} {v0.Z} {v1.X} {v1.Y} {v1.Z} {v2.X} {v2.Y} {v2.Z}");
            }
        }

        return result;
    }
}
