// DODATKOWO W RAZIE POTRZEBY:
// !!! Zainstaluj UnityGLTF !!!
// Install this package from git, compatible with UPM (Unity Package Manager).
// Open Window > Package Manager
// Click +
// Select Add Package from git URL
// Paste
// https://github.com/KhronosGroup/UnityGLTF.git
// Click Add.

using GLTF;
using UnityEngine;

public class GltfExporter : MonoBehaviour
{
    public void ExportMeshToGltf(MeshFilter meshFilter, string filePath)
    {
        GLTFSceneExporter exporter = new GLTFSceneExporter(new[] { meshFilter });
        exporter.SaveGLTFandBin(filePath);
    }
}
