using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
        Debug.Log("Application started. Scanning initialized.");

        // Sprawdz pozycję i skalowanie menu
        if (handedness == Handedness.Right && rightHand != null)
        {
            transform.SetParent(rightHand.transform);
            transform.localPosition = Vector3.zero; // Ustaw lokalną pozycję
            transform.localRotation = Quaternion.identity; // Ustaw lokalną rotację
            transform.localScale = Vector3.one * 0.01f; // Skaluj menu, aby było widoczne
        }
        else
        {
            Debug.LogError("Right hand not found.");
        }

        // Upewnij się, że menu jest aktywne
        gameObject.SetActive(true);

        // Przypisz akcje do przycisków
        if (scanToggleButton != null)
        {
            scanToggleButton.onClick.AddListener(ToggleScanning);
        }
        if (exportButton != null)
        {
            exportButton.onClick.AddListener(ExportMesh);
        }

        // Rozpocznij cykliczne zapisywanie
        StartCoroutine(AutoExportMesh());
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
        Debug.Log("Mesh exported manually.");
    }

    private void UpdateStatusText()
    {
        statusText.text = isScanning ? "Scanning..." : "Scanning stopped.";
        scanToggleButton.GetComponentInChildren<TextMeshProUGUI>().text = isScanning ? "Stop Scanning" : "Start Scanning";
    }

    private IEnumerator AutoExportMesh()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // Co 10 sekund
            if (isScanning)
            {
                uiController.ExportMesh();
                Debug.Log("Mesh exported automatically.");
            }
        }
    }
}
