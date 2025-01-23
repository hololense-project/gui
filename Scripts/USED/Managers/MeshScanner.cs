using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
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

    [Header("Server Settings")]
    [SerializeField] private ServerWebRTC serverWebRTC;

    private bool isScanning = false;
    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private AdvancedLogger logger;

    private Camera mainCamera;
    private List<Vector3> worldVertices;
    private List<int> collectedTriangles;
    private Ray gazeRay;
    private RaycastHit hit;
    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;
    private float saveTimer = 0.0f;

    private void Start()
    {
        logger = new AdvancedLogger(Path.Combine(Application.persistentDataPath, "Logs"));
        SetScanningMode(true);
        mainCamera = Camera.main;

        worldVertices = new List<Vector3>();
        collectedTriangles = new List<int>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (!isScanning) return;

        gazeRay.origin = mainCamera.transform.position;
        gazeRay.direction = mainCamera.transform.forward;

        if (Physics.Raycast(gazeRay, out hit, Mathf.Infinity, detectionLayer))
        {
            ProcessHit(hit);
        }
        else
        {
            ResetGazeTimer();
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }

        saveTimer += Time.deltaTime;
        if (saveTimer >= 1.0f)
        {
            SaveCollectedData();
            saveTimer = 0.0f;
        }

        SendCollectedData();
    }

    private void ProcessHit(RaycastHit hit)
    {
        currentTarget = hit.collider.gameObject;

        if (currentTarget != previousTarget)
        {
            ResetGazeTimer();
            previousTarget = currentTarget;
        }

        gazeTimer += Time.deltaTime;

        if (gazeTimer >= dwellTime)
        {
            MeshFilter meshFilter = currentTarget.GetComponent<MeshFilter>();

            if (meshFilter == null)
            {
                logger.Log("MeshFilter is null for the current target.");
                ResetGazeTimer();
                return;
            }

            ScanMesh(meshFilter);
            ResetGazeTimer();
        }

        UpdateGazeCursor(hit.point);
    }

    private void ResetGazeTimer()
    {
        gazeTimer = 0.0f;
        previousTarget = null;
    }

    private void ScanMesh(MeshFilter meshFilter)
    {
        try
        {
            Mesh mesh = meshFilter.sharedMesh;

            if (mesh == null || mesh.vertexCount == 0 || mesh.triangles.Length == 0)
            {
                logger.Log("Mesh has no vertices or triangles!");
                return;
            }

            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;
            Transform meshTransform = meshFilter.transform;

            // Transform vertices to world positions
            worldVertices.Clear();
            foreach (var vertex in vertices)
            {
                worldVertices.Add(meshTransform.TransformPoint(vertex));
            }

            // Collect triangles
            collectedTriangles.AddRange(indices);

            // Update mesh color to indicate it has been scanned
            cachedRenderer = meshFilter.GetComponent<Renderer>();
            if (cachedRenderer != null)
            {
                cachedRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", Color.green);
                cachedRenderer.SetPropertyBlock(propertyBlock);
            }

            logger.Log("Scanned mesh and collected data.");
        }
        catch (Exception ex)
        {
            logger.Log($"Error scanning mesh: {ex.Message}");
        }
    }

    private async void SaveCollectedData()
    {
        if (collectedTriangles.Count == 0) return;

        try
        {
            string fileName = $"collected_data_{DateTime.Now:yyyyMMdd_HHmmssfff}.obj";

            // Convert indices array to List<int>
            List<int> indicesList = new List<int>(collectedTriangles);

            // Export and save asynchronously
            await ExportMeshAsync(worldVertices, indicesList, fileName);
            logger.Log($"Saved collected data: {fileName}");

            // Clear collected data after saving
            collectedTriangles.Clear();
        }
        catch (Exception ex)
        {
            logger.Log($"Error saving collected data: {ex.Message}");
        }
    }

    private async void SendCollectedData()
    {
        if (collectedTriangles.Count == 0) return;

        try
        {
            // Convert indices array to List<int>
            List<int> indicesList = new List<int>(collectedTriangles);

            // Generate OBJ data
            string objData = meshExporter.GenerateObjData(worldVertices, indicesList);

            // Send OBJ data to server
            if (serverWebRTC != null)
            {
                await serverWebRTC.Send(objData);
                logger.Log("Mesh data sent to server.");
            }
            else
            {
                logger.Log("ServerWebRTC reference is not set.");
            }
        }
        catch (Exception ex)
        {
            logger.Log($"Error sending collected data: {ex.Message}");
        }
    }

    private async Task ExportMeshAsync(List<Vector3> vertices, List<int> triangles, string fileName)
    {
        try
        {
            // Generate OBJ data
            string objData = meshExporter.GenerateObjData(vertices, triangles);

            // Save OBJ file locally using async IO
            string directoryPath = Path.Combine(Application.persistentDataPath, "exported_meshes");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = Path.Combine(directoryPath, fileName);
            await File.WriteAllTextAsync(filePath, objData);

            logger.Log($"Exported mesh to file: {filePath}");
        }
        catch (Exception ex)
        {
            logger.Log($"Error exporting mesh: {ex.Message}");
        }
    }

    public void SetScanningMode(bool scanning)
    {
        isScanning = scanning;

        if (meshManager != null)
        {
            if (isScanning)
            {
                meshManager.HideMesh();
                meshManager.EnableMeshObserver();
            }
            else
            {
                meshManager.DisableMeshObserver();
            }
        }
    }

    private void UpdateGazeCursor(Vector3 position)
    {
        if (gazeCursor != null)
        {
            gazeCursor.transform.position = position;
        }
    }

    public bool IsScanning()
    {
        return isScanning;
    }
}
