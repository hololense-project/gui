using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class EyeTrackingCollector : MonoBehaviour
{
    // Queue to store eye tracking data for sending to the server
    private Queue<string> eyeDataQueue = new Queue<string>();

    private void Start()
    {
        // Initialization if needed
    }

    private void Update()
    {
        // Assuming you have a method to get eye tracking data
        Vector3 eyeGazeOrigin = Vector3.zero; // Replace with actual data
        Vector3 eyeGazeDirection = Vector3.forward; // Replace with actual data

        // Append eye tracking data to the queue
        AppendEyeDataToQueue(eyeGazeOrigin, eyeGazeDirection);
    }

    private void AppendEyeDataToQueue(Vector3 eyeGazeOrigin, Vector3 eyeGazeDirection)
    {
        string eyeData = $"{eyeGazeOrigin.x},{eyeGazeOrigin.y},{eyeGazeOrigin.z},{eyeGazeDirection.x},{eyeGazeDirection.y},{eyeGazeDirection.z}";
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

    public string GetCSVHeader()
    {
        return "EyeGazeOriginX,EyeGazeOriginY,EyeGazeOriginZ,EyeGazeDirectionX,EyeGazeDirectionY,EyeGazeDirectionZ";
    }
}
