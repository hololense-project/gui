using UnityEngine;
using System.Collections.Generic;

public class MeshScanner : MonoBehaviour
{
    public LayerMask detectionLayer;
    public float dwellTime = 2.0f;
    public GameObject gazeCursor;
    public MeshExporter meshExporter;

    // Odwołanie do SendServer
    public SendServer sendServer; 

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private bool isScanning = true; // Automatyczne rozpoczęcie skanowania
    private HashSet<Vector3> scannedVertices = new HashSet<Vector3>();

    //DODANE
    private float sendInterval = 10f; // Czas co ile sekund wysyłamy dane
    private float sendTimer = 0f; // Zegar do śledzenia interwału wysyłania



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


        //DODANE
        // Wysyłanie danych co 10 sekund
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            SendScannedVerticesToServer();
            sendTimer = 0f; // Reset zegara
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

        Debug.Log($"Scanning mesh with {vertices.Length} vertices.");

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldVertex = meshTransform.TransformPoint(vertex);
            float distance = Vector3.Distance(worldVertex, hitPoint);

            if (distance < 5.0f)
            {
                scannedVertices.Add(vertex);
                Debug.Log($"Vertex added at distance {distance}: {worldVertex}");
            }
        }

        if (scannedVertices.Count == 0)
        {
            Debug.LogWarning("No vertices found within the scan radius.");
        }
        else
        {
            Debug.Log($"Scanned {scannedVertices.Count} vertices.");
        }

        meshFilter.GetComponent<Renderer>().material.color = Color.green;
    }



    //DODANA FUNKCJA
    //-------------------------------------------------------------------------------------
    public HashSet<Vector3> GetScannedVertices()
    {
        return scannedVertices;  // Zwróć zebrane wierzchołki
    }
    //-------------------------------------------------------------------------------------



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
        // Implement logic to get triangles from the scanned mesh
        return new List<int>();
    }



    //DODANA FUNKCJA
    //-------------------------------------------------------------------------------------
    // Wysyła zeskanowane wierzchołki na serwer co 10 sekund
    private void SendScannedVerticesToServer()
    {
        if (scannedVertices.Count == 0) return;

        List<Vector3> verticesList = new List<Vector3>(scannedVertices);

        // Konwertujemy wierzchołki na format JSON
        string json = JsonUtility.ToJson(new ScannedMeshData(verticesList));

        // Wysyłamy dane na serwer
        sendServer.Send(json);
        Debug.Log("Wysłano wierzchołki na serwer.");
    }

    // Klasa pomocnicza do przesyłania wierzchołków w formacie JSON
    [System.Serializable]
    public class ScannedMeshData
    {
        public List<Vector3> vertices;

        public ScannedMeshData(List<Vector3> vertices)
        {
            this.vertices = vertices;
        }
    }
    //-------------------------------------------------------------------------------------
}
























































