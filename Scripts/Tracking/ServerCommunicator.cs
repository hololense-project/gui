using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Globalization;
using System;

public class ServerCommunicator : MonoBehaviour
{
    [SerializeField] private HandJointsCollector handJointsCollector;
    [SerializeField] private HeadPositionCollector headPositionCollector;
    [SerializeField] private EyeTrackingCollector eyeTrackingCollector;
    [SerializeField] private ServerWebRTC serverWebRTC;

    private List<string> dataBuffer = new List<string>();
    private int frameCounter = 0;

    private void Start()
    {
        if (handJointsCollector == null)
        {
            Debug.LogError("HandJointsCollector not found in the scene.");
        }

        if (headPositionCollector == null)
        {
            Debug.LogError("HeadPositionCollector not found in the scene.");
        }

        if (eyeTrackingCollector == null)
        {
            Debug.LogError("EyeTrackingCollector not found in the scene.");
        }

        if (serverWebRTC == null)
        {
            Debug.LogError("ServerWebRTC not found in the scene.");
        }

        // Start the auto-save routine
        StartCoroutine(AutoSaveRoutine());
    }

    private void Update()
    {
        frameCounter++;
        if (frameCounter % 2 == 0)
        {
            CollectAndSendData();
        }
    }

    private void CollectAndSendData()
    {
        StringBuilder csvData = new StringBuilder();

        // Append data only
        csvData.Append(DateTime.Now.ToString("o", CultureInfo.InvariantCulture)); // Exact time in ISO 8601 format

        if (handJointsCollector != null)
        {
            string jointData = handJointsCollector.DequeueJointData();
            if (!string.IsNullOrEmpty(jointData))
            {
                csvData.Append($",{jointData}");
            }
        }

        if (headPositionCollector != null)
        {
            string headData = headPositionCollector.DequeueHeadData();
            if (!string.IsNullOrEmpty(headData))
            {
                csvData.Append($",{headData}");
            }
        }

        if (eyeTrackingCollector != null)
        {
            string eyeData = eyeTrackingCollector.DequeueEyeData();
            if (!string.IsNullOrEmpty(eyeData))
            {
                csvData.Append($",{eyeData}");
            }
        }

        string dataToSend = csvData.ToString();
        lock (dataBuffer)
        {
            dataBuffer.Add(dataToSend);
        }
        _ = SendDataAsync(dataToSend);
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10.0f); // Save every 10 seconds
            SaveBufferedDataToFile("AggregatedData.csv");
        }
    }

    private async void SaveBufferedDataToFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        bool fileExists = File.Exists(filePath);

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write headers only if the file does not exist
            if (!fileExists)
            {
                StringBuilder headerBuilder = new StringBuilder();
                headerBuilder.Append("Timestep,");
                if (handJointsCollector != null)
                {
                    headerBuilder.Append(handJointsCollector.GetCSVHeader() + ",");
                }
                if (headPositionCollector != null)
                {
                    headerBuilder.Append(headPositionCollector.GetCSVHeader() + ",");
                }
                if (eyeTrackingCollector != null)
                {
                    headerBuilder.Append(eyeTrackingCollector.GetCSVHeader() + ",");
                }
                headerBuilder.Length--; // Remove the last comma
                writer.WriteLine(headerBuilder.ToString());
            }

            List<string> linesToWrite;
            lock (dataBuffer)
            {
                if (dataBuffer.Count > 0)
                {
                    linesToWrite = new List<string>(dataBuffer);
                    dataBuffer.Clear();
                }
                else
                {
                    linesToWrite = new List<string>();
                }
            }

            // Write data lines to the file
            foreach (var line in linesToWrite)
            {
                writer.WriteLine(line);
            }
        }
        Debug.Log($"Aggregated data saved to {filePath}");

        // Send CSV file to server
        if (serverWebRTC != null)
        {
            await serverWebRTC.SendFileAsync(filePath);
        }
        else
        {
            Debug.LogError("ServerWebRTC reference is not set.");
        }
    }


    private async Task SendDataAsync(string data)
    {
        await serverWebRTC.Send(data);
    }
}
