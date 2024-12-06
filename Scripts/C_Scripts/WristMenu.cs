using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WristMenu : MonoBehaviour
{
    public Button scanToggleButton;
    public Button exportButton;
    public TextMeshProUGUI statusText;
    public UIController uiController;
    public GameObject rightHand;
    public Handedness handedness;

    private bool isScanning;

    public enum Handedness
    {
        Left,
        Right
    }

    private void Start()
    {
        if (handedness == Handedness.Right && rightHand != null)
        {
            // Przypisz WristMenu jako dziecko GameObjectu reprezentującego nadgarstek
            transform.SetParent(rightHand.transform);
            transform.localPosition = Vector3.zero; // Ustaw lokalną pozycję
            transform.localRotation = Quaternion.identity; // Ustaw lokalną rotację
        }
        else
        {
            Debug.LogError("Right hand not found.");
        }

        // Przypisz akcje do przycisków
        if (scanToggleButton != null)
        {
            scanToggleButton.onClick.AddListener(ToggleScanning);
        }
        if (exportButton != null)
        {
            exportButton.onClick.AddListener(ExportMesh);
        }
    }

    private void ToggleScanning()
    {
        isScanning = !isScanning;
        uiController.ToggleScanning();
        UpdateStatusText();
    }

    private void ExportMesh()
    {
        uiController.ExportMesh();
        statusText.text = "Mesh exported.";
    }

    private void UpdateStatusText()
    {
        statusText.text = isScanning ? "Scanning..." : "Scanning stopped.";
        scanToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = isScanning ? "Stop Scanning" : "Start Scanning";
    }
}