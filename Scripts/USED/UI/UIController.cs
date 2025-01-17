using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.IO;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;

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

    [Header("IP Input Field")]
    [SerializeField] private TMP_InputField ipInputField;

    [Header("Load Mesh Button Elements")]
    [SerializeField] private GameObject loadMeshButton;
    [SerializeField] private TextMeshPro loadMeshButtonText;

    [Header("Mesh Collection Panel")]
    [SerializeField] private GameObject meshCollectionPanel;
    [SerializeField] private GameObject meshButtonPrefab;

    private string serverIPAddress = "";

    private void Start()
    {

        // Ustaw katalog logów
        string logDirectoryPath = Path.Combine(Application.persistentDataPath, "Keboard_logs");
        _logger = new AdvancedLogger(logDirectoryPath);

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

        // Initialize IP input field
        if (ipInputField != null)
        {
            ipInputField.onEndEdit.AddListener(OnIPInputEndEdit);
        }

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

    // IP INPUT FIELD
    private async void OnIPInputEndEdit(string input)
    {
        serverIPAddress = input;

        if (!string.IsNullOrEmpty(serverIPAddress))
        {
            // Set the IP address in ServerWebRTC and start the connection
            //serverWebRTC.SetServerIPAddress(serverIPAddress);
            //serverWebRTC.InitClient();

            await _logger.LogAsync("Connecting to " + serverIPAddress + "...");
        }
        else
        {
            await _logger.LogAsync("No IP address entered.");
        }
    }

    // LOAD MESH BUTTON
    public void OpenMeshCollection()
    {
        if (meshCollectionPanel != null)
        {
            meshCollectionPanel.SetActive(true);
            PopulateMeshCollection();
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
        string[] meshFiles = Directory.GetFiles(Application.persistentDataPath, "*.obj");

        // Sort files by modification date
        Array.Sort(meshFiles, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));

        // Trim the array to only include the first 9 files
        if (meshFiles.Length > 9)
        {
            Array.Resize(ref meshFiles, 9);
        }

        // Log the number of mesh files found
        Debug.Log($"Found {meshFiles.Length} .obj files in the directory.");

        // Define grid layout
        Vector3 startPosition = buttonCollection.localPosition - new Vector3(0.016f, -0.016f, 0f); // St
        Vector3 offset = new Vector3(0.02f, -0.02f, 0f); // Offset for each button in the grid
        float padding = 0.012f; // Padding to prevent overlapping

        for (int i = 0; i < meshFiles.Length; i++)
        {
            string filePath = meshFiles[i];
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            // Log each mesh file name
            Debug.Log($"Found mesh file: {fileName}");

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

    private void LoadMesh(string meshPath)
    {
        // Modify the OBJ file if necessary
        MeshLoader.ParseAndModifyObjFile(meshPath);

        // Load the mesh from file
        Mesh mesh = new Mesh();
        MeshLoader.LoadOBJ(meshPath, ref mesh);

        // Create a new GameObject to hold the mesh
        GameObject meshObject = new GameObject("LoadedMesh");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        // Assign a default material to the meshRenderer
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Add a BoxCollider to the mesh
        BoxCollider boxCollider = meshObject.AddComponent<BoxCollider>();
        boxCollider.center = mesh.bounds.center;
        boxCollider.size = mesh.bounds.size;

        // Make the object grabbable and scalable
        meshObject.AddComponent<NearInteractionGrabbable>();
        meshObject.AddComponent<ObjectManipulator>();

        // Scale the mesh to 0.1
        meshObject.transform.localScale = Vector3.one * 0.08f;

        // Get the right hand position
        var rightHand = HandJointUtils.FindHand(Handedness.Right);
        if (rightHand != null && rightHand.TryGetJoint(TrackedHandJoint.Palm, out MixedRealityPose pose))
        {
            meshObject.transform.position = pose.Position;
        }
        else
        {
            Debug.LogWarning("Right hand not found. Mesh will be spawned at the origin.");
            meshObject.transform.position = Vector3.zero;
        }

        // Hide the mesh collection panel
        if (meshCollectionPanel != null)
        {
            meshCollectionPanel.SetActive(false);
        }
    }
}

