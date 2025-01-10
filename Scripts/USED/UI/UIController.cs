using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Mesh Scanner Reference")]
    [SerializeField] private MeshScanner meshScanner;

    [Header("UI Elements")]
    [SerializeField] private GameObject button3D;
    [SerializeField] private TextMeshPro buttonText;
    [SerializeField] private TextMesh logText;

    private Button3D button3DComponent;


    private void Start()
    {
        if (meshScanner == null)
        {
            Debug.LogError("MeshScanner reference is not set in UIController.");
        }

        if (button3D != null)
        {
            button3DComponent = button3D.AddComponent<Button3D>(); // Add the Button3D script and assign it to button3DComponent
            button3DComponent.OnClick.AddListener(ToggleScanning); // Set the OnClick action
            UpdateButtonText();
        }
        else
        {
            Debug.LogError("3D Button is not assigned in UIController.");
        }
    }

    public void ToggleScanning()
    {
        bool isScanning = meshScanner.IsScanning();
        meshScanner.SetScanningMode(!isScanning);
        UpdateButtonText();
    }

    /// <summary>
    /// Aktualizuje tekst przycisku w zależności od stanu skanowania.
    /// </summary>
    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            bool isScanning = meshScanner.IsScanning();
            buttonText.text = isScanning ? "Scan OFF" : "Scan ON";
            logText.text = isScanning ? "Scanning..." : "Scan stopped.";
        }
    }
}