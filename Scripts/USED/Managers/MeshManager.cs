using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;

    private void Start()
    {
        meshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (meshObserver == null)
        {
            Debug.LogError("Spatial Awareness Mesh Observer not found. Ensure MRTK is configured.");
            return;
        }
        
        // Można tutaj skonfigurować poziom detali itp.
    }
}
