using System;
using UnityEngine;
using UnityEngine.UI;

public class CloseServer : MonoBehaviour
{
    private WebRTCClient _client;
    public Button closeButton; // Assign this in the Inspector

    public void SetClient(WebRTCClient client)
    {
        _client = client;
    }

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        else
        {
            Debug.LogError("CloseButton reference is not set in CloseServer.");
        }
    }

    public void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked!");
        CloseConnection();
    }

    public void CloseConnection()
    {
        if (_client != null)
        {
            Debug.Log("Closing WebRTC client...");
            _client.Close();
            _client = null;
        }
        else
        {
            Debug.LogError("WebRTCClient is not initialized in CloseServer.");
        }
    }
}