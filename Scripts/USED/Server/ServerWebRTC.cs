using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerWebRTC : MonoBehaviour
{
    private WebRTCClient _client;
<<<<<<< Updated upstream
    private string serverIPAddress = "localhost"; // Domyślne IP
    private int serverPort = 8765; // Domyślny port
    private string sessionId = "S1"; // Domyślny ID sesji
    private string channel = "chat"; // Domyślny kanał
=======
    private string serverIPAddress = "localhost";
>>>>>>> Stashed changes

    //public void SetServerIPAddress(string ipAddress)
    //{
    //    serverIPAddress = ipAddress;
    //    return this;
    //}

    // in case keyboard on hololense does not work
    private void Start()
    {
<<<<<<< Updated upstream
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
=======
        InitClient();
    }

    public void InitClient()
    {
        //if (string.IsNullOrEmpty(serverIPAddress))
        //{
        //    Debug.LogError("Set to default IP Address 192.168.0.177");
        //    SetServerIPAddress("192.168.0.105");
        //    return this;
        //}

        // Unique random peerId
        string peerId = "hololens" + Guid.NewGuid().ToString();
>>>>>>> Stashed changes

        _client = new WebRTCClientBuilder()
            .SetServer(serverIPAddress)
            .SetPort(8765)
            .SetSessionId("S1")
            .SetPeerId(peerId)
            .SetChannel("chat")
            .Build();

        _client.InitClient();
        _client.OnDataReceived += OnMessage;
    }

    public async Task Send(string message)
    {
        if (_client != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await Task.Run(() =>
            {
                _client.Send(data);
                Debug.Log("Message sent: " + message);
            });
        }
        else
        {
            Debug.LogError("WebRTCClient is not initialized.");
        }
    }

    private void OnMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Debug.Log("Message received: " + message);
    }

    private void OnDestroy()
    {
        if (_client != null)
        {
            _client.Close();
        }
    }
}
