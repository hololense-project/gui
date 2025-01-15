using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;

public class HandJointsCollector : MonoBehaviour
{
    // Dictionary to store joint positions for each hand
    private Dictionary<TrackedHandJoint, Transform> leftHandJoints = new Dictionary<TrackedHandJoint, Transform>();
    private Dictionary<TrackedHandJoint, Transform> rightHandJoints = new Dictionary<TrackedHandJoint, Transform>();

    // Auto-save interval in seconds
    [SerializeField] private float autoSaveInterval = 0.5f;
    private Coroutine autoSaveCoroutine;

    // Queue to store joint data for sending to the server
    private Queue<string> jointDataQueue = new Queue<string>();

    private void Start()
    {
        // Initialize joint dictionaries
        InitializeHandJoints(leftHandJoints);
        InitializeHandJoints(rightHandJoints);

        // Start the auto-save routine
        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    private void Update()
    {
        // Update joint positions for each hand
        UpdateHandJoints(Handedness.Left, leftHandJoints);
        UpdateHandJoints(Handedness.Right, rightHandJoints);
    }

    private void InitializeHandJoints(Dictionary<TrackedHandJoint, Transform> handJoints)
    {
        foreach (TrackedHandJoint joint in System.Enum.GetValues(typeof(TrackedHandJoint)))
        {
            GameObject jointObject = new GameObject(joint.ToString());
            jointObject.transform.parent = this.transform;
            handJoints[joint] = jointObject.transform;
        }
    }

    private void UpdateHandJoints(Handedness handedness, Dictionary<TrackedHandJoint, Transform> handJoints)
    {
        foreach (TrackedHandJoint joint in handJoints.Keys)
        {
            if (HandJointUtils.TryGetJointPose(joint, handedness, out MixedRealityPose pose))
            {
                handJoints[joint].position = pose.Position;
                handJoints[joint].rotation = pose.Rotation;
            }
        }
    }

    public void SaveHandJointsToFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Left Hand Joints:");
            SaveHandJoints(writer, leftHandJoints);

            writer.WriteLine("Right Hand Joints:");
            SaveHandJoints(writer, rightHandJoints);
        }
        Debug.Log($"Hand joints saved to {filePath}");
    }

    private void SaveHandJoints(StreamWriter writer, Dictionary<TrackedHandJoint, Transform> handJoints)
    {
        foreach (var joint in handJoints)
        {
            writer.WriteLine($"{joint.Key}: Position = {joint.Value.position}, Rotation = {joint.Value.rotation}");
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveHandJointsToFile("HandJointsData.txt");
            AppendJointDataToQueue();
        }
    }

    private void AppendJointDataToQueue()
    {
        string jointData = GetJointDataString();
        lock (jointDataQueue)
        {
            jointDataQueue.Enqueue(jointData);
        }
    }

    private string GetJointDataString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Left Hand Joints:");
        AppendJointData(sb, leftHandJoints);

        sb.AppendLine("Right Hand Joints:");
        AppendJointData(sb, rightHandJoints);

        return sb.ToString();
    }

    private void AppendJointData(System.Text.StringBuilder sb, Dictionary<TrackedHandJoint, Transform> handJoints)
    {
        foreach (var joint in handJoints)
        {
            sb.AppendLine($"{joint.Key}: Position = {joint.Value.position}, Rotation = {joint.Value.rotation}");
        }
    }

    public string DequeueJointData()
    {
        lock (jointDataQueue)
        {
            if (jointDataQueue.Count > 0)
            {
                return jointDataQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }

    public Dictionary<TrackedHandJoint, Transform> GetLeftHandJoints()
    {
        return leftHandJoints;
    }

    public Dictionary<TrackedHandJoint, Transform> GetRightHandJoints()
    {
        return rightHandJoints;
    }
}



