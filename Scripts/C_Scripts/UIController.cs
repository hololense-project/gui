using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public MeshScanner meshScanner;

    private bool isScanning = false;

    public void ToggleScanning()
    {
        isScanning = !isScanning;
        meshScanner.SetScanningMode(isScanning);
    }

    public void ExportMesh()
    {
        meshScanner.ExportScannedMesh();
    }
}