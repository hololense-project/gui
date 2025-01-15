using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;

public class EyeTrackingCollector : MonoBehaviour
{
    // Auto-save interval in seconds
    [SerializeField] private float autoSaveInterval = 0.5f;
    private Coroutine autoSaveCoroutine;

    // Queue to store eye tracking data for sending to the server
    private Queue<string> eyeDataQueue = new Queue<string>();

    private void Start()
    {
        // Start the auto-save routine
        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    // tu nie dziala
    private void Update()
    {
        if (CoreServices.InputSystem?.EyeGazeProvider?.IsEyeTrackingEnabledAndValid == true)
        {
            Vector3 eyeGazeOrigin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            Vector3 eyeGazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;

            // Append eye tracking data to the queue
           // AppendEyeDataToQueue(eyeGazeOrigin, eyeGazeDirection);
        }
    }

    // a tu dziala xd
    public void SaveEyeTrackingDataToFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            Vector3 eyeGazeOrigin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            Vector3 eyeGazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            string eyeData = $"Eye Gaze Origin: {eyeGazeOrigin}, Eye Gaze Direction: {eyeGazeDirection}";
            writer.WriteLine(eyeData);
            AppendEyeDataToQueue(eyeData);
        }
        Debug.Log($"Eye tracking data saved to {filePath}");
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveEyeTrackingDataToFile("EyeTrackingData.txt");
        }
    }

    private void AppendEyeDataToQueue(string eyeData)
    {
        lock (eyeDataQueue)
        {
            eyeDataQueue.Enqueue(eyeData);
        }
    }

    public string DequeueEyeData()
    {
        lock (eyeDataQueue)
        {
            if (eyeDataQueue.Count > 0)
            {
                return eyeDataQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }
}




