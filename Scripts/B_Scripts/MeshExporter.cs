using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MeshExporter
{
    public static void ExportMeshToObj(MeshFilter meshFilter, string filePath)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter or Mesh is null. Cannot export.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# Exported Mesh");
        sb.AppendLine();

        foreach (Vector3 vertex in mesh.vertices)
        {
            sb.AppendLine($"v {vertex.x} {vertex.y} {vertex.z}");
        }

        foreach (Vector3 normal in mesh.normals)
        {
            sb.AppendLine($"vn {normal.x} {normal.y} {normal.z}");
        }

        foreach (Vector2 uv in mesh.uv)
        {
            sb.AppendLine($"vt {uv.x} {uv.y}");
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            sb.AppendLine($"f {mesh.triangles[i] + 1} {mesh.triangles[i + 1] + 1} {mesh.triangles[i + 2] + 1}");
        }

        // Użyj StreamWriter z FileShare.None, aby upewnić się, że plik jest zamknięty przed zapisem
        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8, 4096))
        {
            writer.Write(sb.ToString());
        }

        Debug.Log($"Mesh exported to {filePath}");
    }

    public static async Task ExportMeshToObjAsync(MeshFilter meshFilter, string filePath)
    {
        await Task.Run(() =>
        {
            // Przenieś operacje na obiektach Unity do głównego wątku
            UnityMainThreadDispatcher.Instance().Enqueue(() => ExportMeshToObj(meshFilter, filePath));
        });
    }
}