using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MRTK2 - kluczowe przestrzenie nazw
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class WristMenuController : MonoBehaviour
{
    [Header("MRTK Solver Settings")]
    [Tooltip("Solver Handler przypinany do obiektu, aby śledzić nadgarstek prawej dłoni.")]
    public SolverHandler solverHandler;

    [Header("UI References")]
    [Tooltip("Przycisk start/stop skanowania.")]
    public Button scanToggleButton;

    [Tooltip("Przycisk ręcznego eksportu.")]
    public Button exportButton;

    [Tooltip("Tekst statusu wyświetlany na menu (TextMeshPro).")]
    public TextMeshProUGUI statusText;

    [Tooltip("Dodatkowy tekst wyświetlający komunikaty / feedback użytkownikowi (TextMeshPro).")]
    public TextMeshProUGUI feedbackText;

    [Header("Auto Export Settings")]
    [Tooltip("Interwał automatycznego zapisu (w sekundach).")]
    public float autoExportInterval = 10f;

    private bool isScanning = true;       // Domyślnie skanowanie jest aktywne
    private bool isMenuVisible = true;    // Menu jest widoczne od startu

    private MeshScanner meshScanner;


    private void Start()
    {
        // (1) Ustaw skalę menu — tak, aby było odpowiednio duże/małe na ręce.
        transform.localScale = Vector3.one * 0.02f;

        // (2) Jeśli brak SolverHandler, wyświetl ostrzeżenie w konsoli.
        if (solverHandler == null)
        {
            Debug.LogWarning("WristMenuController: Brak SolverHandler – sprawdź w Inspectorze.");
            SetFeedbackText("Brak SolverHandler – sprawdź w Inspectorze.");
        }

        // (3) Upewnij się, że menu jest aktywne od razu.
        gameObject.SetActive(isMenuVisible);

        // (4) Log w konsoli o rozpoczęciu skanowania i informacje w feedbackText.
        Debug.Log("WristMenuController: Aplikacja wystartowała. Rozpoczęto skanowanie.");
        SetFeedbackText("Aplikacja wystartowała. Rozpoczęto skanowanie.");

        // (5) Przypisz metody do przycisków.
        if (scanToggleButton != null)
        {
            scanToggleButton.onClick.AddListener(ToggleScanning);
        }
        if (exportButton != null)
        {
            exportButton.onClick.AddListener(ExportMeshManually);
        }

        // (6) Zaktualizuj wyświetlany status i feedback.
        UpdateStatusText();

        // (7) Rozpocznij co-routine odpowiedzialną za automatyczny zapis.
        StartCoroutine(AutoExportRoutine());
    }

    /// <summary>
    /// Przełącza stan skanowania (Start/Stop).
    /// </summary>
    private void ToggleScanning()
    {
        isScanning = !isScanning;

        if (isScanning)
        {
            Debug.Log("WristMenuController: Skanowanie włączone.");
            SetFeedbackText("Skanowanie zostało włączone.");
        }
        else
        {
            Debug.Log("WristMenuController: Skanowanie wyłączone.");
            SetFeedbackText("Skanowanie zostało wyłączone.");
        }

        UpdateStatusText();
    }

    /// <summary>
    /// Ręczny eksport – wywoływany np. po kliknięciu przycisku „Export Mesh”.
    /// </summary>
    private void ExportMeshManually()
    {
        Debug.Log("WristMenuController: Ręczny eksport danych/siatki.");
        SetFeedbackText("Ręczny eksport zakończony!");

        if (statusText != null)
        {
            statusText.text = "Ręczny eksport zakończony!";
        }
    }

    /// <summary>
    /// Aktualizuje tekst statusu w menu UI, np. „Skanowanie trwa...” / „Skanowanie zatrzymane”.
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isScanning
                ? "Status: Skanowanie trwa..."
                : "Status: Skanowanie zatrzymane";
        }

        if (scanToggleButton != null)
        {
            var buttonText = scanToggleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isScanning ? "Stop Scan" : "Start Scan";
            }
        }
    }

    /// <summary>
    /// Co-routine odpowiedzialna za cykliczne (co 10s) zapisywanie,
    /// wywoływana tylko, gdy isScanning = true.
    /// </summary>
    private System.Collections.IEnumerator AutoExportRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoExportInterval);

            if (isScanning)
            {
                Debug.Log("WristMenuController: Automatyczny zapis co " + autoExportInterval + "s.");
                SetFeedbackText("Automatyczny zapis co " + autoExportInterval + " sekund.");
                // Tutaj można wywołać np. SaveDataToFile();
                meshScanner.ExportScannedMesh();
                
            }
        }
    }

    /// <summary>
    /// Ustawia dodatkowy tekst „feedbackText” (jeśli istnieje), by poinformować użytkownika.
    /// </summary>
    private void SetFeedbackText(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
