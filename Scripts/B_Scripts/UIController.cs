using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Button exportButton;
    private GazeDetector gazeDetector;
    private bool isScanning = false;

    private void Start()
    {
        // Sprawdź, czy exportButton jest przypisany
        if (exportButton == null)
        {
            Debug.LogError("Export button not assigned in the inspector.");
            return;
        }

        gazeDetector = FindObjectOfType<GazeDetector>();

        if (gazeDetector == null)
        {
            Debug.LogError("GazeDetector not found in the scene.");
            return;
        }

        exportButton.onClick.AddListener(() =>
        {
            isScanning = !isScanning;
            gazeDetector.SetScanningMode(isScanning);
            exportButton.GetComponentInChildren<Text>().text = isScanning ? "Stop Scanning" : "Start Scanning";
        });
    }
}