using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.IO;
using System;
using TMPro;
using UnityEngine;
using GLTFast;

public class UIController : MonoBehaviour
{
    [Header("Mesh Scanner Reference")]
    [SerializeField] private MeshScanner meshScanner;
    [Header("WebRTC Server Reference")]
    [SerializeField] public ServerWebRTC serverWebRTC;
    [Header("Adv logger")]
    [SerializeField] public AdvancedLogger _logger;

    [Header("UI Elements")]
    [SerializeField] private GameObject ScanButton;
    [SerializeField] private TextMeshPro ScanButtonText;
    [SerializeField] private TextMesh logText;

    [Header("Load Mesh Button Elements")]
    [SerializeField] private GameObject loadMeshButton;
    [SerializeField] private TextMeshPro loadMeshButtonText;

    [Header("Mesh Collection Panel")]
    [SerializeField] private GameObject meshCollectionPanel;
    [SerializeField] private GameObject meshButtonPrefab;

    private string serverIPAddress = "";
    private AdvancedLogger meshLogger;

    private void Start()
    {
        // Set up log directories
        string logDirectoryPath = Path.Combine(Application.persistentDataPath, "Keyboard_logs");
        _logger = new AdvancedLogger(logDirectoryPath);

        string meshLogDirectoryPath = Path.Combine(Application.persistentDataPath, "Mesh_logs");
        meshLogger = new AdvancedLogger(meshLogDirectoryPath);

        if (meshScanner == null)
        {
            Debug.LogError("MeshScanner reference is not set in UIController.");
        }

        if (serverWebRTC == null)
        {
            Debug.LogError("ServerWebRTC reference is not set in UIController.");
        }

        // Explicitly set the scanning mode to false at launch
        meshScanner.SetScanningMode(false);
        UpdateButtonText();

        // Initialize Load Mesh Button
        if (loadMeshButtonText != null)
        {
            loadMeshButtonText.text = "Load Mesh";
        }

        // Hide the mesh collection panel at the start
        if (meshCollectionPanel != null)
        {
            meshCollectionPanel.SetActive(false);
        }
    }

    // SCAN BUTTON
    public void ToggleScanning()
    {
        bool isScanning = meshScanner.IsScanning();
        meshScanner.SetScanningMode(!isScanning);
        _ = serverWebRTC.Send(isScanning ? "#STOP" : "#START");
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (ScanButtonText != null)
        {
            bool isScanning = meshScanner.IsScanning();
            ScanButtonText.text = isScanning ? "Scan is ON" : "Scan is OFF";
            logText.text = isScanning ? "Scanning..." : "Scan stopped.";
        }
    }

    // LOAD MESH BUTTON
    public async void OpenMeshCollection()
    {
        if (meshCollectionPanel != null)
        {
            meshCollectionPanel.SetActive(true);
            string folderName = "grtest"; // Replace with your actual folder name
            try
            {
                await serverWebRTC.DownloadObjFiles(folderName);
                PopulateMeshCollection();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error downloading .obj files: " + ex.Message);
                await _logger.LogAsync("Error downloading .obj files: " + ex.Message);
            }
        }
    }

    private void PopulateMeshCollection()
    {
        // Clear existing buttons
        Transform buttonCollection = meshCollectionPanel.transform.Find("ButtonCollection");
        if (buttonCollection != null)
        {
            foreach (Transform child in buttonCollection)
            {
                Destroy(child.gameObject);
            }
        }

        // Get all mesh files from the default path
        string[] objFiles = Directory.GetFiles(Application.persistentDataPath, "*.obj");
        string[] glbFiles = Directory.GetFiles(Application.persistentDataPath, "*.glb");
        string[] gltfFiles = Directory.GetFiles(Application.persistentDataPath, "*.gltf");

        // Combine all mesh files into one array with GLB files first
        string[] meshFiles = new string[glbFiles.Length + gltfFiles.Length + objFiles.Length];
        glbFiles.CopyTo(meshFiles, 0);
        gltfFiles.CopyTo(meshFiles, glbFiles.Length);
        objFiles.CopyTo(meshFiles, glbFiles.Length + gltfFiles.Length);

        // Sort files by modification date
        Array.Sort(meshFiles, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));

        // Trim the array to only include the first 9 files
        if (meshFiles.Length > 9)
        {
            Array.Resize(ref meshFiles, 9);
        }

        // Log the number of mesh files found
        Debug.Log($"Found {meshFiles.Length} mesh files in the directory.");
        meshLogger.Log($"Found {meshFiles.Length} mesh files in the directory.").FlushLogs();

        // Define grid layout
        Vector3 startPosition = buttonCollection.localPosition - new Vector3(0.016f, -0.016f, 0f); // Start position
        Vector3 offset = new Vector3(0.02f, -0.02f, 0f); // Offset for each button in the grid
        float padding = 0.012f; // Padding to prevent overlapping

        for (int i = 0; i < meshFiles.Length; i++)
        {
            string filePath = meshFiles[i];
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            // Log each mesh file name
            Debug.Log($"Found mesh file: {fileName}");
            meshLogger.Log($"Found mesh file: {fileName}").FlushLogs();

            // Create a button for each mesh
            GameObject buttonObject = Instantiate(meshButtonPrefab, buttonCollection);
            TextMeshPro buttonText = buttonObject.GetComponentInChildren<TextMeshPro>();
            if (buttonText != null)
            {
                buttonText.text = fileName;
            }

            // Add listener to load mesh on click
            Interactable interactable = buttonObject.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.OnClick.AddListener(() => LoadMesh(filePath));
            }
            // Calculate grid position
            int row = i / 3;
            int col = i % 3;
            float xOffset = col > 0 ? col * (offset.x + padding) : 0;
            float yOffset = row > 0 ? row * (offset.y - padding) : 0;
            Vector3 buttonPosition = startPosition + new Vector3(xOffset, yOffset, 0);

            // Check if the button position is already taken and update position if necessary
            while (IsPositionTaken(buttonPosition, buttonCollection))
            {
                buttonPosition.y -= 0.032f;
            }
            buttonObject.transform.localPosition = buttonPosition;
        }
    }

    private bool IsPositionTaken(Vector3 position, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.localPosition == position)
            {
                return true;
            }
        }
        return false;
    }

 private async void LoadMesh(string meshPath)
    {
        string extension = Path.GetExtension(meshPath).ToLower();
        if (extension == ".obj")
        {
            // Parse and potentially modify OBJ
            MeshLoader.ParseAndModifyObjFile(meshPath);

            // Load the mesh
            Mesh mesh = new Mesh();
            MeshLoader.LoadOBJ(meshPath, ref mesh);

            // Create a new GameObject for the mesh
            GameObject meshObject = CreateMeshGameObject(mesh, "LoadedMesh", null);
        }
        else if (extension == ".gltf")
        {
            var (meshes, materials) = await MeshLoader.LoadGLTF(meshPath);
            if (meshes != null && meshes.Length > 0)
            {
                // Iterate through all meshes and create GameObjects for them
                for (int i = 0; i < meshes.Length; i++)
                {
                    GameObject meshObject = CreateMeshGameObject(meshes[i], "LoadedGLTFMesh_" + i, materials[i]);
                }
            }
        }
        else if (extension == ".glb")
        {
            var (meshes, materials) = await MeshLoader.LoadGLB(meshPath);
            if (meshes != null && meshes.Length > 0)
            {
                // Iterate through all meshes and create GameObjects for them
                for (int i = 0; i < meshes.Length; i++)
                {
                    GameObject meshObject = CreateMeshGameObject(meshes[i], "LoadedGLBMesh_" + i, materials[i]);
                }
            }
        }

        // Hide the mesh collection panel
        if (meshCollectionPanel != null)
        {
            meshCollectionPanel.SetActive(false);
        }
    }

private GameObject CreateMeshGameObject(Mesh mesh, string name, Material[] materials)
{
    // Create a parent GameObject to hold everything
    GameObject parentObject = new GameObject(name + "_Parent");

    // Create a new GameObject for the mesh itself
    GameObject meshObject = new GameObject(name);
    meshObject.transform.SetParent(parentObject.transform);  // Make it a child of the parent object

    // Add MeshFilter and MeshRenderer components to the mesh object
    MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
    meshFilter.mesh = mesh;
    MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
    if (materials != null && materials.Length > 0)
    {
        meshRenderer.materials = materials;
    }
    else
    {
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }

    // Add a BoxCollider to the parent object (this will allow for grabbing/manipulation of the whole object)
    BoxCollider boxCollider = parentObject.AddComponent<BoxCollider>();
    boxCollider.center = mesh.bounds.center;
    boxCollider.size = mesh.bounds.size;

    // Add ObjectManipulator and NearInteractionGrabbable components to the parent object
    parentObject.AddComponent<ObjectManipulator>();
    var grabbable = parentObject.AddComponent<NearInteractionGrabbable>();

    // Optionally, you can configure the ObjectManipulator to use the parent object's transform
    var objectManipulator = parentObject.GetComponent<ObjectManipulator>();
    objectManipulator.HostTransform = parentObject.transform;

    // Scale the parent object
    parentObject.transform.localScale = Vector3.one * 0.08f;

    // Position the parent object based on the user's right hand (if available)
    var rightHand = HandJointUtils.FindHand(Handedness.Right);
    if (rightHand != null && rightHand.TryGetJoint(TrackedHandJoint.Palm, out MixedRealityPose pose))
    {
        parentObject.transform.position = pose.Position;
    }
    else
    {
        parentObject.transform.position = Vector3.zero;
    }

    return parentObject;  // Return the parent object, which contains the mesh as a child
}

}

