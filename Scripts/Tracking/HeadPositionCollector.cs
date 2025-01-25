using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HeadPositionCollector : MonoBehaviour
{
    // Queue to store head position data for sending to the server
    private Queue<string> headDataQueue = new Queue<string>();

    private void Start()
    {
        // Initialization if needed
    }

    private void Update()
    {
        if (CameraCache.Main != null)
        {
            Transform headTransform = CameraCache.Main.transform;
            Vector3 headPosition = headTransform.position;
            Quaternion headRotation = headTransform.rotation;

            // Append head position data to the queue
            AppendHeadDataToQueue(headPosition, headRotation);
        }
    }

    private void AppendHeadDataToQueue(Vector3 headPosition, Quaternion headRotation)
    {
        string headData = $"{headPosition.x},{headPosition.y},{headPosition.z},{headRotation.x},{headRotation.y},{headRotation.z},{headRotation.w}";
        lock (headDataQueue)
        {
            headDataQueue.Enqueue(headData);
        }
    }

    public string DequeueHeadData()
    {
        lock (headDataQueue)
        {
            if (headDataQueue.Count > 0)
            {
                return headDataQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }

    public string GetCSVHeader()
    {
        return "HeadPositionX,HeadPositionY,HeadPositionZ,HeadRotationX,HeadRotationY,HeadRotationZ,HeadRotationW";
    }
}
