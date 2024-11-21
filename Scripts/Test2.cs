using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR;
using MixedReality.Toolkit;
using System.Collections.Generic;
using TMPro;

public class MeshControllerMRTK3 : MonoBehaviour
{
    // Referencje do przycisków UI
    public Button startStopMeshButton;
    public Button toggleTransmissionButton;
    public Button toggleRefinementButton;

    private XRMeshSubsystem meshSubsystem;
    private bool isMeshRunning = false;
    private bool isRefinementEnabled = false;
    private bool isWaitingForTransmissionSignal = true;

    // Animatorzy przycisków dla animacji
    // private Animator meshButtonAnimator;
    // private Animator refinementButtonAnimator;
    // private Animator transmissionButtonAnimator;

    private void Start()
    {
        // Znajdź Mesh Subsystem
        meshSubsystem = XRSubsystemHelpers.MeshSubsystem;
        var meshSubsystems = new List<XRMeshSubsystem>();
        SubsystemManager.GetInstances(meshSubsystems);

        if (meshSubsystems.Count > 0)
        {
            meshSubsystem = meshSubsystems[0];
            Debug.Log("Mesh Subsystem initialized successfully.");
        }
        else
        {
            Debug.LogError("Mesh Subsystem not found");
        }

        // Pobieranie komponentów Animatora z przycisków
        // meshButtonAnimator = startStopMeshButton?.GetComponent<Animator>();
        // refinementButtonAnimator = toggleRefinementButton?.GetComponent<Animator>();
        // transmissionButtonAnimator = toggleTransmissionButton?.GetComponent<Animator>();

        // Przypisz metody do przycisków
        if (startStopMeshButton != null)
            startStopMeshButton.onClick.AddListener(ToggleMesh);

        if (toggleTransmissionButton != null)
            toggleTransmissionButton.onClick.AddListener(ToggleTransmissionMode);

        if (toggleRefinementButton != null)
            toggleRefinementButton.onClick.AddListener(ToggleMeshRefinement);

        UpdateButtonLabels();
        UpdateButtonColors();
    }

    // Włączanie i wyłączanie tworzenia siatki otoczenia
    private void ToggleMesh()
    {
        if (meshSubsystem == null) return;

        isMeshRunning = !isMeshRunning;

        if (isMeshRunning)
        {
            meshSubsystem.Start();
        }
        else
        {
            meshSubsystem.Stop();
        }

        UpdateButtonLabels();
        UpdateButtonColors();
        StartCoroutine(HighlightButton(startStopMeshButton));
        // PlayAnimation(meshButtonAnimator, "ButtonPressed");
    }

    // Przełącza tryb przesyłania
    private void ToggleTransmissionMode()
    {
        isWaitingForTransmissionSignal = !isWaitingForTransmissionSignal;

        if (!isWaitingForTransmissionSignal)
        {
            SendMeshData();
        }

        UpdateButtonLabels();
        UpdateButtonColors();
        StartCoroutine(HighlightButton(toggleTransmissionButton));
        // PlayAnimation(transmissionButtonAnimator, "ButtonPressed");
    }

    // Przełącza uszczegółowienie siatki
    private void ToggleMeshRefinement()
    {
        if (meshSubsystem == null) return;

        isRefinementEnabled = !isRefinementEnabled;

        // W MRTK 3 szczegółowość siatki jest kontrolowana w inny sposób, np. poprzez parametry subsystemu
        // Dodaj odpowiednie ustawienia dla swojej implementacji:
        Debug.Log($"Mesh refinement set to {(isRefinementEnabled ? "Fine" : "Coarse")}");

        UpdateButtonLabels();
        UpdateButtonColors();
        StartCoroutine(HighlightButton(toggleRefinementButton));
        // PlayAnimation(refinementButtonAnimator, "ButtonPressed");
    }

    // Wysyła dane siatki - implementacja zależna od sposobu przesyłania
    private void SendMeshData()
    {
        Debug.Log("Sending mesh data...");
        // Wstaw kod do wysyłania danych siatki
    }

    // Aktualizuje etykiety przycisków w UI
    private void UpdateButtonLabels()
    {
        if (startStopMeshButton != null)
        {
            var textComponent = startStopMeshButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = isMeshRunning ? "Stop Mesh" : "Start Mesh";
            }
            else
            {
                Debug.LogError("Text component not found in startStopMeshButton");
            }
        }
        else
        {
            Debug.LogError("startStopMeshButton is null");
        }

        if (toggleTransmissionButton != null)
        {
            var textComponent = toggleTransmissionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = isWaitingForTransmissionSignal ? "Waiting for Signal" : "Send Mesh";
            }
            else
            {
                Debug.LogError("Text component not found in toggleTransmissionButton");
            }
        }
        else
        {
            Debug.LogError("toggleTransmissionButton is null");
        }

        if (toggleRefinementButton != null)
        {
            var textComponent = toggleRefinementButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = isRefinementEnabled ? "Stop Refinement" : "Start Refinement";
            }
            else
            {
                Debug.LogError("Text component not found in toggleRefinementButton");
            }
        }
        else
        {
            Debug.LogError("toggleRefinementButton is null");
        }
    }


    // Aktualizuje kolory przycisków w zależności od ich stanu
    private void UpdateButtonColors()
    {
        Color meshButtonColor = isMeshRunning ? Color.green : Color.red;
        Color refinementButtonColor = isRefinementEnabled ? Color.yellow : Color.gray;
        Color transmissionButtonColor = isWaitingForTransmissionSignal ? Color.blue : Color.cyan;

        if (startStopMeshButton != null)
            startStopMeshButton.GetComponent<Image>().color = meshButtonColor;

        if (toggleRefinementButton != null)
            toggleRefinementButton.GetComponent<Image>().color = refinementButtonColor;

        if (toggleTransmissionButton != null)
            toggleTransmissionButton.GetComponent<Image>().color = transmissionButtonColor;
    }

    // Efekt krótkiego podświetlenia przycisku po kliknięciu
    private IEnumerator HighlightButton(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        Color originalColor = buttonImage.color;
        buttonImage.color = Color.yellow; // Kolor podświetlenia
        yield return new WaitForSeconds(0.2f);
        buttonImage.color = originalColor; // Przywróć oryginalny kolor
    }

    // Uruchamianie animacji dla podanego Animatora
    private void PlayAnimation(Animator animator, string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}
