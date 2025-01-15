using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
