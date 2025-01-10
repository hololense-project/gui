using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using System.Threading.Tasks;

public class CloseServer : MonoBehaviour
{
    [SerializeField] private GameObject button3D;
    private HubConnection _client;
    private Button3D closeButton;

    public void SetClient(HubConnection client)
    {
        _client = client;
    }

    void Start()
    {
        if (button3D != null)
        {
            closeButton = button3D.AddComponent<Button3D>(); 
            closeButton.OnClick.AddListener(OnCloseButtonClicked);
        }
        else
        {
            Debug.LogError("3D Button is not assigned in UIController.");
        }
    }

    public async void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked!");
        await CloseConnection();
    }

    public async Task CloseConnection()
    {
        if (_client != null)
        {
            await _client.StopAsync();
            await _client.DisposeAsync();
        }
        else
        {
            Debug.LogError("HubConnection client is not initialized in CloseServer.");
        }
    }
}