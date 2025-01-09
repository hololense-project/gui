using System;
using System.Text;
using UnityEngine;

public class SendServer : MonoBehaviour
{
    WebRTCClient _client;

    void Start()
    {
        string peerId = Guid.NewGuid().ToString();

        // Inicjalizacja WebRTCClient
        _client = new WebRTCClientBuilder()
            .SetServer("localhost")
            .SetPort(8765)
            .SetSessionId("ecb8b8d7-9587-41a7-aaf9-ecd0d2d51108")
            .SetPeerId(peerId)
            .SetChannel("scan-channel")
            .Build();

        _client.InitClient();
    }

    public void Send(string message)
    {
        if (_client != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _client.Send(data);
            Debug.Log("Wys³ano wiadomoœæ: " + message);
        }
    }
}
