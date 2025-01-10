using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

public class RunServer : MonoBehaviour
{
    private HubConnection _connection;
    private const string serverUrl = "http://192.168.0.177:8765";
    private const string peerId = "hololense";
    private static readonly HttpClient httpClient = new HttpClient();

    public event Action OnConnected;
    public event Action OnDisconnected;

    [SerializeField] private TextMesh logText;

    async void Start()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += async (error) =>
        {
            Debug.LogWarning("Połączenie z serwerem zostało zamknięte.");
            OnDisconnected?.Invoke();
            await Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            Debug.LogWarning("Ponowne łączenie z serwerem...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Debug.Log("Ponownie połączono z serwerem.");
            OnConnected?.Invoke();
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            Debug.Log("Połączono z Serwerem.");
            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Błąd połączenia Serwerem: {ex.Message}");
        }
    }

    public async Task PostDataToServer(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            Debug.LogError("Nie można wysłać pustych danych na serwer.");
            return;
        }

        try
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(serverUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Dane pomyślnie wysłane na serwer API.");
                logText.text += "Dane pomyślnie wysłane na serwer API.";
            }
            else
            {
                Debug.LogError($"Błąd wysyłania danych. Kod odpowiedzi: {response.StatusCode}");
                logText.text += $"Błąd wysyłania danych. Kod odpowiedzi: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Błąd połączenia z API: {ex.Message}");
            logText.text = $"Błąd połączenia z API: {ex.Message}";
        }
    }

    async void OnDestroy()
    {
        if (_connection != null)
        {
            await _connection.StopAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }

    public bool IsConnected()
    {
        return _connection != null && _connection.State == HubConnectionState.Connected;
    }
}
