using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MeshExporter : MonoBehaviour
{
    public async Task ExportMeshToObjAsync(List<Vector3> vertices, List<int> triangles, string fileName)
    {
        if (!ValidateMeshData(vertices, triangles))
        {
            Debug.LogError("Invalid mesh data. Export failed.");
            return;
        }

        try
        {
            string objData = GenerateObjData(vertices, triangles);
            string fullPath = GetFilePath(fileName);

            using (StreamWriter writer = new StreamWriter(fullPath, false, Encoding.UTF8, 8192))
            {
                await writer.WriteAsync(objData);
            }

            Debug.Log($"Mesh successfully exported to {fullPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to export mesh: {ex.Message}");
        }
    }

    private bool ValidateMeshData(List<Vector3> vertices, List<int> triangles)
    {
        if (vertices == null || vertices.Count == 0)
        {
            Debug.LogError("Vertices list is null or empty.");
            return false;
        }

        if (triangles == null || triangles.Count % 3 != 0)
        {
            Debug.LogError("Triangles list is null or not a multiple of 3.");
            return false;
        }

        return true;
    }

    public string GenerateObjData(List<Vector3> vertices, List<int> triangles)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Exported Mesh");
        sb.AppendLine();

        // Add vertices
        foreach (Vector3 vertex in vertices)
        {
            sb.AppendLine($"v {vertex.x:F6} {vertex.y:F6} {vertex.z:F6}");
        }

        sb.AppendLine();

        // Add faces
        for (int i = 0; i < triangles.Count; i += 3)
        {
            sb.AppendLine($"f {triangles[i] + 1} {triangles[i + 1] + 1} {triangles[i + 2] + 1}");
        }

        return sb.ToString();
    }

    private string GetFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.");
        }

        return Path.Combine(Application.persistentDataPath, fileName);
    }
}