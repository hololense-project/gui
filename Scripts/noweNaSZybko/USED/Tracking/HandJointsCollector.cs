using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Text;

public class HandJointsCollector : MonoBehaviour
{
    // Dictionary to store joint positions for each hand
    private Dictionary<TrackedHandJoint, Transform> leftHandJoints = new Dictionary<TrackedHandJoint, Transform>();
    private Dictionary<TrackedHandJoint, Transform> rightHandJoints = new Dictionary<TrackedHandJoint, Transform>();

    // Queue to store joint data for sending to the server
    private Queue<string> jointDataQueue = new Queue<string>();

    private void Start()
    {
        // Initialize joint dictionaries
        InitializeHandJoints(leftHandJoints);
        InitializeHandJoints(rightHandJoints);
    }

    private void Update()
    {
        // Update joint positions for each hand
        UpdateHandJoints(Handedness.Left, leftHandJoints);
        UpdateHandJoints(Handedness.Right, rightHandJoints);

        // Collect data in real-time
        AppendJointDataToQueue();
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
        StringBuilder sb = new StringBuilder();
        AppendJointData(sb, leftHandJoints);
        AppendJointData(sb, rightHandJoints);
        sb.Length--; // Remove the last comma
        return sb.ToString();
    }

    private void AppendJointData(StringBuilder sb, Dictionary<TrackedHandJoint, Transform> handJoints)
    {
        foreach (var joint in handJoints)
        {
            Vector3 position = joint.Value.position;
            Quaternion rotation = joint.Value.rotation;
            sb.Append($"{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w},");
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

    public string GetCSVHeader()
    {
        StringBuilder header = new StringBuilder();
        foreach (TrackedHandJoint joint in System.Enum.GetValues(typeof(TrackedHandJoint)))
        {
            header.Append($"{joint}PositionX,{joint}PositionY,{joint}PositionZ,{joint}RotationX,{joint}RotationY,{joint}RotationZ,{joint}RotationW,");
        }
        header.Length--; // Remove the last comma
        return header.ToString();
    }
}
