using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class ServerCommunicator : MonoBehaviour
{
    [SerializeField] private HandJointsCollector handJointsCollector;
    [SerializeField] private HeadPositionCollector headPositionCollector;
    [SerializeField] private EyeTrackingCollector eyeTrackingCollector;
    [SerializeField] private ServerWebRTC serverWebRTC;

    [SerializeField] private float sendInterval = 1.0f;

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

        // Start the coroutine to send data to the server
        StartCoroutine(SendDataToServerRoutine());
    }

    private IEnumerator SendDataToServerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);

            if (serverWebRTC != null)
            {
                if (eyeTrackingCollector != null)
                {
                    string eyeData = eyeTrackingCollector.DequeueEyeData();

                    if (!string.IsNullOrEmpty(eyeData))
                    {
                        _ = SendDataAsync(eyeData);
                    }
                }
                if (handJointsCollector != null)
                {
                    string jointData = handJointsCollector.DequeueJointData();
                    if (!string.IsNullOrEmpty(jointData))
                    {
                        _ = SendDataAsync(jointData);
                    }
                }
                if (headPositionCollector != null)
                {
                    string headData = headPositionCollector.DequeueHeadData();
                    if (!string.IsNullOrEmpty(headData))
                    {
                        _ = SendDataAsync(headData);
                    }
                }
            }
        }
    }

    private async Task SendDataAsync(string data)
    {
        await serverWebRTC.Send(data);
    }
}

