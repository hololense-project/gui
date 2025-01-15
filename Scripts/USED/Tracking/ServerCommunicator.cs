using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        // Append headers
        csvData.Append("Timestep,");
        if (handJointsCollector != null)
        {
            csvData.Append(handJointsCollector.GetCSVHeader() + ",");
        }
        if (headPositionCollector != null)
        {
            csvData.Append(headPositionCollector.GetCSVHeader() + ",");
        }
        if (eyeTrackingCollector != null)
        {
            csvData.Append(eyeTrackingCollector.GetCSVHeader() + ",");
        }
        csvData.Length--; // Remove the last comma
        csvData.AppendLine();

        // Append data
        csvData.Append(System.DateTime.Now.ToString("o")); // Exact time in ISO 8601 format

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

    private void SaveBufferedDataToFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            lock (dataBuffer)
            {
                if (dataBuffer.Count > 0)
                {
                    foreach (var data in dataBuffer)
                    {
                        writer.WriteLine(data);
                    }
                    dataBuffer.Clear();
                }
            }
        }
        Debug.Log($"Aggregated data saved to {filePath}");
    }

    private async Task SendDataAsync(string data)
    {
        await serverWebRTC.Send(data);
    }
}
