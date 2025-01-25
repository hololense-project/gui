using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private float defaultDistance = 2.0f;
    [SerializeField] private Transform cameraTransform;

    private void Update()
    {
        if (!cameraTransform)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Ustawia kursor przed kamerą
        transform.position = cameraTransform.position + cameraTransform.forward * defaultDistance;
        transform.rotation = cameraTransform.rotation;
    }
}
