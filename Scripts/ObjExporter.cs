using System.Text;
using UnityEngine;
using System.IO;

public class OBJExporter : MonoBehaviour
{
    public void ExportToObj(GameObject model, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        MeshFilter meshFilter = model.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No MeshFilter found on the model.");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv = mesh.uv;

        // Sprawdzamy, czy mamy normalne, jeœli nie, to generujemy
        if (normals.Length == 0)
        {
            normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = Vector3.up; // W tym przypadku ustawiamy normalne w górê
            }
        }

        // Sprawdzamy, czy mamy UV, jeœli nie, to ustawiamy domyœlne wartoœci
        if (uv.Length == 0)
        {
            uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(0, 0); // Domyœlne UV
            }
        }

        // Zapisujemy wierzcho³ki
        foreach (Vector3 vertex in vertices)
        {
            sb.AppendLine("v " + vertex.x + " " + vertex.y + " " + vertex.z);
        }

        // Zapisujemy normalne
        foreach (Vector3 normal in normals)
        {
            sb.AppendLine("vn " + normal.x + " " + normal.y + " " + normal.z);
        }

        // Zapisujemy wspó³rzêdne UV
        foreach (Vector2 uvCoord in uv)
        {
            sb.AppendLine("vt " + uvCoord.x + " " + uvCoord.y);
        }

        // Zapisujemy trójk¹ty
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            sb.AppendLine("f " +
                (triangles[i] + 1) + "/" + (triangles[i] + 1) + "/" + (triangles[i] + 1) + " " +
                (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) + "/" + (triangles[i + 1] + 1) + " " +
                (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1) + "/" + (triangles[i + 2] + 1));
        }

        // Zapisujemy wynikowy plik .obj
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log("Exported OBJ file to: " + filePath);
    }
}
