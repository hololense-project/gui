using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast;

public static class MeshLoader
{
    public static void LoadOBJ(string path, ref Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (line.StartsWith("v "))
            {
                string[] parts = line.Split(' ');
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);
                vertices.Add(new Vector3(x, y, z));
            }
            else if (line.StartsWith("vn "))
            {
                string[] parts = line.Split(' ');
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);
                normals.Add(new Vector3(x, y, z));
            }
            else if (line.StartsWith("vt "))
            {
                string[] parts = line.Split(' ');
                float u = float.Parse(parts[1]);
                float v = float.Parse(parts[2]);
                uvs.Add(new Vector2(u, v));
            }
            else if (line.StartsWith("f "))
            {
                string[] parts = line.Split(' ');
                for (int i = 1; i < parts.Length; i++)
                {
                    string[] indices = parts[i].Split('/');
                    int vertexIndex = int.Parse(indices[0]) - 1;
                    triangles.Add(vertexIndex);

                    if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]))
                    {
                        int uvIndex = int.Parse(indices[1]) - 1;
                        // Ensure the UVs list is large enough
                        while (uvs.Count <= uvIndex)
                        {
                            uvs.Add(Vector2.zero);
                        }
                    }

                    if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]))
                    {
                        int normalIndex = int.Parse(indices[2]) - 1;
                        // Ensure the normals list is large enough
                        while (normals.Count <= normalIndex)
                        {
                            normals.Add(Vector3.zero);
                        }
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        if (normals.Count > 0)
        {
            mesh.normals = normals.ToArray();
        }
        if (uvs.Count > 0)
        {
            mesh.uv = uvs.ToArray();
        }
    }

public static async Task<(Mesh[] meshes, Material[][] materials)> LoadGLTF(string filePath)
{
    if (!File.Exists(filePath))
    {
        Debug.LogError("File not found: " + filePath);
        return (null, null);
    }

    GltfImport importer = new GltfImport();
    bool success = await importer.Load(new Uri(filePath));
    if (success)
    {
        // Create a parent object for the scene
        GameObject parentObject = new GameObject("GLTFScene");

        // Asynchronously instantiate the scene
        bool sceneLoaded = await importer.InstantiateSceneAsync(parentObject.transform);
        if (sceneLoaded)
        {
            // Prepare lists to collect meshes and materials
            List<Mesh> meshes = new List<Mesh>();
            List<Material[]> materialsList = new List<Material[]>();

            // Loop through all MeshFilter components in the child objects of the parent object
            foreach (var meshFilter in parentObject.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    meshes.Add(meshFilter.sharedMesh);

                    // Collect the materials from the corresponding MeshRenderer
                    MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        // Ensure that materials exist before adding them
                        if (meshRenderer.sharedMaterials != null && meshRenderer.sharedMaterials.Length > 0)
                        {
                            materialsList.Add(meshRenderer.sharedMaterials);
                        }
                        else
                        {
                            // If no materials are found, add a default material
                            materialsList.Add(new Material[] { new Material(Shader.Find("Standard")) });
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer component missing for mesh: " + meshFilter.gameObject.name);
                        materialsList.Add(new Material[] { new Material(Shader.Find("Standard")) });
                    }
                }
                else
                {
                    Debug.LogWarning("MeshFilter or sharedMesh missing for GameObject: " + meshFilter.gameObject.name);
                }
            }

            // Return the meshes and materials as arrays
            if (meshes.Count > 0)
            {
                Debug.Log("GLTF meshes and materials loaded successfully.");
                return (meshes.ToArray(), materialsList.ToArray());
            }
            else
            {
                Debug.LogError("No meshes found in the GLTF scene.");
                return (null, null);
            }
        }
        else
        {
            Debug.LogError("Failed to instantiate the GLTF scene.");
            return (null, null);
        }
    }
    else
    {
        Debug.LogError($"Failed to load GLTF from {filePath}.");
        return (null, null);
    }
}

public static async Task<(Mesh[] meshes, Material[][] materials)> LoadGLB(string filePath)
{
    if (!File.Exists(filePath))
    {
        Debug.LogError("File not found: " + filePath);
        return (null, null);
    }

    GltfImport importer = new GltfImport();
    bool success = await importer.Load(new Uri(filePath));
    if (success)
    {
        // Create a parent object for the scene
        GameObject parentObject = new GameObject("GLBScene");

        // Asynchronously instantiate the scene
        bool sceneLoaded = await importer.InstantiateSceneAsync(parentObject.transform);
        if (sceneLoaded)
        {
            // Prepare lists to collect meshes and materials
            List<Mesh> meshes = new List<Mesh>();
            List<Material[]> materialsList = new List<Material[]>();

            // Loop through all MeshFilter components in the child objects of the parent object
            foreach (var meshFilter in parentObject.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    meshes.Add(meshFilter.sharedMesh);
                    
                    // Collect the materials from the corresponding MeshRenderer
                    MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        // Ensure that materials exist before adding them
                        if (meshRenderer.sharedMaterials != null && meshRenderer.sharedMaterials.Length > 0)
                        {
                            materialsList.Add(meshRenderer.sharedMaterials);
                        }
                        else
                        {
                            // If no materials are found, add a default material
                            materialsList.Add(new Material[] { new Material(Shader.Find("Standard")) });
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MeshRenderer component missing for mesh: " + meshFilter.gameObject.name);
                        materialsList.Add(new Material[] { new Material(Shader.Find("Standard")) });
                    }
                }
                else
                {
                    Debug.LogWarning("MeshFilter or sharedMesh missing for GameObject: " + meshFilter.gameObject.name);
                }
            }

            // Return the meshes and materials as arrays
            if (meshes.Count > 0)
            {
                Debug.Log("GLB meshes and materials loaded successfully.");
                return (meshes.ToArray(), materialsList.ToArray());
            }
            else
            {
                Debug.LogError("No meshes found in the GLB scene.");
                return (null, null);
            }
        }
        else
        {
            Debug.LogError("Failed to instantiate the GLB scene.");
            return (null, null);
        }
    }
    else
    {
        Debug.LogError($"Failed to load GLB from {filePath}.");
        return (null, null);
    }
}

    public static void ParseAndModifyObjFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        bool modified = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("f "))
            {
                if (lines[i].Contains("/"))
                {
                    lines[i] = ReformatFaceLine(lines[i]);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            File.WriteAllLines(filePath, lines);
            Debug.Log("File modified: " + filePath);
        }
        else
        {
            Debug.Log("No modifications needed: " + filePath);
        }
    }

    private static string ReformatFaceLine(string faceLine)
    {
        string[] parts = faceLine.Split(' ');
        for (int i = 1; i < parts.Length; i++)
        {
            parts[i] = parts[i].Split('/')[0];
        }
        return string.Join(" ", parts);
    }
}