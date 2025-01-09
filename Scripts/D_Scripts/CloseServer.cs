using System;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour // Zmieniono nazw� klasy na "Example"
{
    WebRTCClient _client;

    void Start()
    {
        // Unikalny identyfikator peerId
        string peerId = Guid.NewGuid().ToString();

        // Tworzenie klienta WebRTC
        _client = new WebRTCClientBuilder()
            .SetServer("localhost")
            .SetPort(8765)
            .SetSessionId("ecb8b8d7-9587-41a7-aaf9-ecd0d2d51108")
            .SetPeerId(peerId)
            .SetChannel("chat") // Kana� u�ywany w komunikacji
            .Build();

        _client.InitClient();

        // Znajd� przycisk i przypisz akcj� do zamykania po��czenia
        Button closeButton = GameObject.Find("CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    // Metoda przypisana do przycisku
    public void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked!");
        OnDestroy(); // R�cznie wywo�aj metod� zamykania
    }

    private void OnDestroy()
    {
        if (_client != null)
        {
            Debug.Log("Closing WebRTC client...");
            _client.Close(); // Zamyka po��czenie z serwerem
            _client = null;  // Usuwa odwo�anie do klienta
        }
    }
}
