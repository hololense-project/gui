using System;
using System.Text;
using UnityEngine;

public class SendServer : MonoBehaviour
{
    public WebRTCClient _client;
    public CloseServer closeServer;

    void Start()
    {
        string peerId = "hololense";

        _client = new WebRTCClientBuilder()
            .SetServer("192.168.0.177")
            .SetPort(8765)
            .SetSessionId("S1")
            .SetPeerId(peerId)
            .SetChannel("scan-channel")
            .Build();

        _client.InitClient();

        if (closeServer != null)
        {
            closeServer.SetClient(_client);
        }
        else
        {
            Debug.LogError("CloseServer reference is not set in SendServer.");
        }
    }

    public void Send(string message)
    {
        if (_client != null)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            _client.Send(data);
            Debug.Log("Message sent: " + message);
        }
        else
        {
            Debug.LogError("WebRTCClient is not initialized.");
        }
    }
}
