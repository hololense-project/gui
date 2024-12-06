using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    public float defaultDistance = 2.0f;
    public Transform cameraTransform;

    void Update()
    {
        transform.position = cameraTransform.position + cameraTransform.forward * defaultDistance;
        transform.rotation = cameraTransform.rotation;
    }
}