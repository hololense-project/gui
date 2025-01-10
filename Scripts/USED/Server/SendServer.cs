using System;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

public class SendServer : MonoBehaviour
{
    private HubConnection _connection;
    public CloseServer closeServer;

    private const string peerId = "hololense";

    async Task Start()
    {
        string serverUrl = "http://192.168.0.177:5000/scanHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();

        try
        {
            await _connection.StartAsync();
            Debug.Log("SignalR Connected.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"SignalR Connection Error: {ex.Message}");
        }

        if (closeServer != null)
        {
            closeServer.SetClient(_connection);
        }
        else
        {
            Debug.LogError("CloseServer reference is not set in SendServer.");
        }
    }

    public async Task Send(string message)
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
        {
            try
            {
                await _connection.InvokeAsync("SendScanData", peerId, message);
                Debug.Log("Message sent: " + message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SignalR Send Error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("SignalR connection is not established.");
        }
    }

    private async Task OnDestroy()
    {
        await _connection.StopAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
        await _connection.StopAsync();
        await _connection.DisposeAsync();
    }
}
