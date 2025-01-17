using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerWebRTC : MonoBehaviour
{
    private AdvancedLogger _logger;
    private string logDirectoryPath;
    private WebRTCClient _client;
    private string serverIPAddress = "localhost"; // Domyślne IP
    private int serverPort = 8765; // Domyślny port
    private string sessionId = "S1"; // Domyślny ID sesji
    private string channel = "chat"; // Domyślny kanał

    private async void Start()
    {
        // Ustaw katalog logów
        logDirectoryPath = Path.Combine(Application.persistentDataPath, "ServerWebRTC_Logs");
        _logger = new AdvancedLogger(logDirectoryPath);

        await _logger.LogAsync("ServerWebRTC uruchomiony.");
        await _logger.LogAsync($"Logi będą zapisywane w katalogu: {logDirectoryPath}");

        try
        {
            InitClient();
            await ConnectToServer();
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Błąd w trakcie inicjalizacji: {ex.Message}");
        }
    }

    private void InitClient()
    {
        string peerId = Guid.NewGuid().ToString();
        try
        {
             _client = new WebRTCClientBuilder()
                .SetServer(serverIPAddress)
                .SetPort((uint)serverPort)
                .SetSessionId(sessionId)
                .SetPeerId(peerId)
                .SetChannel(channel)
                .EnableDebug(true)
                .Build();
            
            _client.OnDataReceived += OnMessage;

            _logger.Log($"Zainicjalizowano klienta WebRTC:")
                .Log($"Server: {serverIPAddress}")
                .Log($"Port: {serverPort}")
                .Log($"SessionId: {sessionId}")
                .Log($"PeerId: {peerId}")
                .Log($"Channel: {channel}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Błąd w trakcie inicjalizacji klienta: {ex.Message}");
        }
    }

    private async Task ConnectToServer()
    {
        if (_client == null)
        {
            await _logger.LogAsync("Nie można nawiązać połączenia: klient WebRTC nie został poprawnie zainicjalizowany.");
            return;
        }

        try
        {
            _client.InitClient();
            await _logger.LogAsync("Pomyślnie połączono z serwerem WebRTC.");

            _client.OnDataReceived += OnMessage;
            await _logger.LogAsync("Nasłuchuję na wiadomości od serwera WebRTC.");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Błąd podczas łączenia z serwerem: {ex.Message}");
        }

    }

    public async Task Send(string message)
    {
        if (_client != null && _client.IsChannelOpen())
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            try
            {
                _client.Send(data);
                await _logger.LogAsync($"Wysłano wiadomość: {message}");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync($"Błąd podczas wysyłania wiadomości: {ex.Message}");
            }
        }
        else
        {
            await _logger.LogAsync("Nie można wysłać wiadomości: kanał danych nie jest otwarty.");
        }
    }

    private async void OnMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        await _logger.LogAsync($"Otrzymano wiadomość: {message}");
    }

    private async void OnDestroy()
    {
        if (_client != null)
        {
            try
            {
                _client.Close();
                await _logger.LogAsync("Rozłączono z serwerem WebRTC podczas niszczenia obiektu.");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync($"Błąd podczas rozłączania: {ex.Message}");
            }
        }

        await _logger.LogAsync("ServerWebRTC zakończył działanie.");
        _logger.FlushLogs();
    }
}
