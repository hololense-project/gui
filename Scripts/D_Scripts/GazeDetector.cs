using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GazeDetector : MonoBehaviour
{
    public LayerMask detectionLayer;
    public float dwellTime = 2.0f;
    public float minDistance = 0.5f;
    public float maxDistance = 5.0f;
    public GameObject gazeCursor;
    public QueueProcessor queueProcessor;

    private float gazeTimer = 0.0f;
    private GameObject currentTarget = null;
    private GameObject previousTarget = null;
    private bool isScanning = false;
    private List<GameObject> scannedObjects = new List<GameObject>();

    void Update()
    {
        if (!isScanning) return;

        RaycastHit hit;
        Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(gazeRay, out hit, Mathf.Infinity, detectionLayer))
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
            target.GetComponent<Renderer>().material.color = Color.green; // Highlight the object
        }
    }

    private async void ExportScannedObjects()
    {
        if (scannedObjects.Count == 0)
        {
            Debug.LogError("No objects scanned for export.");
            return;
        }

        foreach (var obj in scannedObjects)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                queueProcessor.AddToQueue(meshFilter);
            }
        }

        await queueProcessor.ProcessQueueAsync();
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