using UnityEngine;
using System.Collections.Generic;

public class GazeDetector : MonoBehaviour
{
    [Header("Gaze Detection Settings")]
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float dwellTime = 0.0f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float maxDistance = 5.0f;

    [Header("Cursor Reference")]
    [SerializeField] private GameObject gazeCursor;

    [Header("Queue Processing")]
    [SerializeField] private QueueProcessor queueProcessor;

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private bool isScanning = false;
    private List<GameObject> scannedObjects = new List<GameObject>();

    private void Update()
    {
        if (!isScanning) return;

        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(gazeRay, out RaycastHit hit, Mathf.Infinity, detectionLayer))
        {
            currentTarget = hit.collider.gameObject;
            float distance = Vector3.Distance(Camera.main.transform.position, currentTarget.transform.position);

            if (distance >= minDistance && distance <= maxDistance)
            {
                if (currentTarget != previousTarget)
                {
                    gazeTimer = 0.0f;
                    previousTarget = currentTarget;
                }

                gazeTimer += Time.deltaTime;

                if (gazeTimer >= dwellTime)
                {
                    ScanObject(currentTarget);
                    gazeTimer = 0.0f;
                }

                UpdateGazeCursor(hit.point);
            }
        }
        else
        {
            currentTarget = null;
            gazeTimer = 0.0f;

            // Przesuń kursor "w nieskończoność"
            UpdateGazeCursor(gazeRay.origin + gazeRay.direction * 10);
        }
    }

    public void SetScanningMode(bool isScanning)
    {
        this.isScanning = isScanning;
        if (!isScanning)
        {
            ExportScannedObjects();
        }
    }

    private void ScanObject(GameObject target)
    {
        if (!scannedObjects.Contains(target))
        {
            Debug.Log($"Scanned object: {target.name}");
            scannedObjects.Add(target);

            // Zmiana koloru obiektu, aby zaznaczyć, że został zeskanowany
            Renderer rend = target.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.green;
            }
        }
    }

    private async void ExportScannedObjects()
    {
        if (scannedObjects.Count == 0)
        {
            Debug.LogError("No objects scanned for export.");
            return;
        }

        // Dodawanie do kolejki w QueueProcessor
        foreach (var obj in scannedObjects)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                queueProcessor.AddToQueue(meshFilter);
            }
        }

        // Asynchroniczne przetwarzanie kolejki
        await queueProcessor.ProcessQueueAsync();

        // Wyczyść listę po eksporcie
        scannedObjects.Clear();
    }

    private void UpdateGazeCursor(Vector3 position)
    {
        if (gazeCursor != null)
        {
            gazeCursor.transform.position = position;
        }
    }
}
