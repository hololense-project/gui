using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ServerWebRTC : MonoBehaviour
{
    private AdvancedLogger _logger;
    private string logDirectoryPath;
    private WebRTCClient _client;
    private string serverIPAddress = "192.168.0.101"; // Default IP
    private int serverPort = 8765; // Default port
    private string sessionId = "S1"; // Default session ID
    private string channel = "chat"; // Default channel
    [SerializeField] private KeyboardHandler keyboardHandler;

    private HttpClientHandler _httpClientHandler;
    private HttpClient _httpClient;

    private readonly string username = "TEST";
    private readonly string password = "TEST123";
    private Cookie rememberTokenCookie;

    private async void Start()
    {
        logDirectoryPath = Path.Combine(Application.persistentDataPath, "ServerWebRTC_Logs");
        _logger = new AdvancedLogger(logDirectoryPath);

        await _logger.LogAsync("ServerWebRTC started.");
        await _logger.LogAsync($"Logs will be saved in: {logDirectoryPath}");

        _httpClientHandler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        _httpClient = new HttpClient(_httpClientHandler);

        SetServerIPAddress(serverIPAddress);
    }

    public async void SetServerIPAddress(string ipAddress)
    {
        serverIPAddress = ipAddress;
        await _logger.LogAsync($"Server IP address set to: {serverIPAddress}");

        try
        {
            bool loginSuccess = await Login();
            if (loginSuccess)
            {
                InitClient();
                ConnectToServer();
            }
            else
            {
                await _logger.LogAsync("Login failed. Cannot initialize client and connect to server.");
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Initialization error: {ex.Message}");
        }
    }

    private async Task<bool> Login()
    {
        string loginUrl = $"http://{serverIPAddress}:5000/login";
        var loginData = new
        {
            username = this.username,
            password = this.password
        };
        string json = JsonUtility.ToJson(loginData);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(loginUrl, content);
            response.EnsureSuccessStatusCode();
            await _logger.LogAsync("Login successful. Cookies saved for future requests.");

            // Log the cookies received after login
            CookieCollection cookies = _httpClientHandler.CookieContainer.GetCookies(new Uri(loginUrl));
            foreach (Cookie cookie in cookies)
            {
                await _logger.LogAsync($"Received cookie: {cookie.Name}={cookie.Value}");
                if (cookie.Name == "remember_token")
                {
                    rememberTokenCookie = cookie;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Login failed. Error: {ex.Message}");
            return false;
        }
    }

    private void InitClient()
    {
        string peerId = "hololense"+Guid.NewGuid().ToString();
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

            _logger.Log($"Initialized WebRTC client:")
                .Log($"Server: {serverIPAddress}")
                .Log($"Port: {serverPort}")
                .Log($"SessionId: {sessionId}")
                .Log($"PeerId: {peerId}")
                .Log($"Channel: {channel}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Client initialization error: {ex.Message}");
        }
    }

    private void ConnectToServer()
    {
        if (_client == null)
        {
            _logger.Log("Cannot connect: WebRTC client not properly initialized.");
            return;
        }

        try
        {
            _client.InitClient();
            _logger.Log("Successfully connected to WebRTC server.");

            _client.OnDataReceived += OnMessage;
            _logger.Log("Listening for messages from WebRTC server.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Connection error: {ex.Message}");
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
                await _logger.LogAsync($"Message sent: {message}");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync($"Message sending error: {ex.Message}");
            }
        }
        else
        {
            await _logger.LogAsync("Cannot send message: data channel is not open.");
        }
    }

    private async void OnMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        await _logger.LogAsync($"Message received: {message}");
    }

    private async void OnDestroy()
    {
        if (_client != null)
        {
            try
            {
                _client.Close();
                await _logger.LogAsync("Disconnected from WebRTC server during object destruction.");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync($"Disconnection error: {ex.Message}");
            }
        }

        await _logger.LogAsync("ServerWebRTC stopped.");
        _logger.FlushLogs();
    }

    private async Task EnsureLoggedInAsync()
    {
        bool loginSuccess = await Login();
        if (!loginSuccess)
        {
            throw new Exception("Login failed.");
        }
    }

    private async Task LogRequestHeaders(HttpRequestMessage request)
    {
        await _logger.LogAsync("Request Headers:");
        foreach (var header in request.Headers)
        {
            await _logger.LogAsync($"{header.Key}: {string.Join(", ", header.Value)}");
        }
    }

    private void AddCookiesToRequest(HttpRequestMessage request, Uri uri)
    {
        CookieCollection cookies = _httpClientHandler.CookieContainer.GetCookies(uri);
        foreach (Cookie cookie in cookies)
        {
            request.Headers.Add("Cookie", $"{cookie.Name}={cookie.Value}");
        }
    }

    public async Task<string> GetAsync(string url)
    {
        try
        {
            await EnsureLoggedInAsync();

            // Log the full URL being used for the GET request
            await _logger.LogAsync($"GET request URL: {url}");

            // Create the GET request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add cookies to the request
            AddCookiesToRequest(request, new Uri(url));

            // Log the request headers
            await LogRequestHeaders(request);

            // Send the GET request
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            await _logger.LogAsync($"HTTP GET request to {url} successful. Response: {responseBody}");
            return responseBody;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"HTTP GET request to {url} failed. Error: {ex.Message}");
            throw;
        }
    }

    public async Task DownloadObjFiles(string folderName)
    {
        string baseUrl = $"http://{serverIPAddress}:5000/user_panel/api/folders/TEST";
        try
        {
            await EnsureLoggedInAsync();

            // Get the list of mesh files in the folder
            string fileListResponse = await GetAsync(baseUrl);
            List<string> meshFiles = ParseMeshFiles(fileListResponse);

            foreach (string fileUrl in meshFiles)
            {
                string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(new Uri(fileUrl).LocalPath));

                // Log the full URL being used for the GET request
                await _logger.LogAsync($"GET request URL: {fileUrl}");

                // Create the GET request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fileUrl);

                // Add cookies to the request
                AddCookiesToRequest(request, new Uri(fileUrl));

                // Log the request headers
                await LogRequestHeaders(request);

                // Send the GET request with retry mechanism
                bool success = false;
                int retryCount = 3;
                for (int i = 0; i < retryCount; i++)
                {
                    try
                    {
                        HttpResponseMessage response = await _httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                        await File.WriteAllBytesAsync(localPath, fileBytes);
                        await _logger.LogAsync($"Downloaded {fileUrl} to {localPath}");
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogAsync($"Error downloading {fileUrl}: {ex.Message}. Attempt {i + 1} of {retryCount}.");
                        if (i == retryCount - 1)
                        {
                            await _logger.LogAsync($"Failed to download {fileUrl} after {retryCount} attempts.");
                        }
                    }
                }

                if (!success)
                {
                    await _logger.LogAsync($"Skipping {fileUrl} due to repeated errors.");
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Error downloading mesh files from {baseUrl}: {ex.Message}");
        }
    }

    private List<string> ParseMeshFiles(string jsonResponse)
    {
        var meshFiles = new List<string>();
        var jsonArray = JArray.Parse(jsonResponse);

        foreach (var item in jsonArray)
        {
            var downloadLink = item["download_link"]?.ToString();
            if (!string.IsNullOrEmpty(downloadLink) && (downloadLink.EndsWith(".obj") || downloadLink.EndsWith(".glb") || downloadLink.EndsWith(".gltf")))
            {
                meshFiles.Add(downloadLink);
            }
        }

        return meshFiles;
    }

    public void AddCookie(string url, string name, string value)
    {
        Uri uri = new Uri(url);
        _httpClientHandler.CookieContainer.Add(uri, new Cookie(name, value));
        _logger.Log($"Added cookie: {name}={value} for URL: {url}");
    }

    public CookieCollection GetCookies(string url)
    {
        Uri uri = new Uri(url);
        return _httpClientHandler.CookieContainer.GetCookies(uri);
    }
}

