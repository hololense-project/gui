using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;

    private void Start()
    {
        if (CoreServices.SpatialAwarenessSystem == null)
        {
            Debug.LogError("Spatial Awareness System not initialized. Ensure MRTK is correctly set up.");
            return;
        }

        meshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (meshObserver == null)
        {
            Debug.LogError("Spatial Awareness Mesh Observer not found. Ensure MRTK is configured.");
            return;
        }

        ConfigureMeshObserver();
    }

    private void ConfigureMeshObserver()
    {
        // W³¹czanie i ustawianie parametrów skanowania
        meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
        meshObserver.LevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Coarse; // Mo¿na zmieniæ na Fine dla lepszej jakoœci
        meshObserver.UpdateInterval = 1.0f; // Skanowanie co 1 sekundê
    }

    public void HideMesh()
    {
        if (meshObserver != null)
        {
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
            Debug.Log("Mesh is now hidden.");
        }
    }

    public void ShowMesh()
    {
        if (meshObserver != null)
        {
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
            Debug.Log("Mesh is now visible.");
        }
    }

    public void EnableMeshObserver()
    {
        if (meshObserver != null)
        {
            meshObserver.Resume();
            Debug.Log("Mesh Observer Enabled");
        }
    }

    public void DisableMeshObserver()
    {
        if (meshObserver != null)
        {
            meshObserver.Suspend();
            Debug.Log("Mesh Observer Disabled");
        }
    }
}
