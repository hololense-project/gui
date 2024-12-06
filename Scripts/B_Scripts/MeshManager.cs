using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections.Generic;

public class MeshManager : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;

    public void Start()
    {
        meshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (meshObserver == null)
        {
            Debug.LogError("Spatial Awareness Mesh Observer not found. Ensure MRTK is configured.");
            return;
        }
    }

    //public MeshFilter[] GetAllMeshFilters()
    //{
    //    var meshObjects = meshObserver.Meshes;
    //    var meshFilters = new MeshFilter[meshObjects.Count];

    //    int index = 0;
    //    foreach (var mesh in meshObjects.Values)
    //    {
    //        meshFilters[index++] = mesh.Filter;
    //    }

    //    return meshFilters;
    //}

    // public List<MeshFilter> GetAllMeshFilters()
    // {
    //     if (meshObserver == null)
    //     {
    //         Debug.LogError("Mesh Observer is not initialized.");
    //         return new List<MeshFilter>();
    //     }

    //     var meshFilters = new List<MeshFilter>();

    //     // Iteracja przez obiekty siatki w observerze
    //     foreach (var meshObject in meshObserver.Meshes)
    //     {
    //         if (meshObject.Value?.Filter != null)
    //         {
    //             meshFilters.Add(meshObject.Value.Filter);
    //         }
    //     }

    //     Debug.Log($"Found {meshFilters.Count} mesh filters.");
    //     return meshFilters;
    // }
}
