using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;

public class HeadPositionCollector : MonoBehaviour
{
    // Auto-save interval in seconds
    [SerializeField] private float autoSaveInterval = 0.5f;
    private Coroutine autoSaveCoroutine;

    // Queue to store head position data for sending to the server
    private Queue<string> headDataQueue = new Queue<string>();

    private void Start()
    {
        // Start the auto-save routine
        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
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

    public void SaveHeadPositionToFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            Vector3 headPosition = CameraCache.Main.transform.position;
            Quaternion headRotation = CameraCache.Main.transform.rotation;

            writer.WriteLine($"Head Position: {headPosition}, Head Rotation: {headRotation}");
        }
        Debug.Log($"Head position saved to {filePath}");
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveHeadPositionToFile("HeadPositionData.txt");
        }
    }

    private void AppendHeadDataToQueue(Vector3 headPosition, Quaternion headRotation)
    {
        string headData = $"Head Position: {headPosition}, Head Rotation: {headRotation}";
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
}



