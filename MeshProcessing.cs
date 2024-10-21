// Uzycie:
// using UnityEngine;

// public class MeshProcessingExample : MonoBehaviour
// {
//     public MeshProcessing meshProcessing;  // Referencja do skryptu 'MeshProcessing'

//     void Start()
//     {
//         // Ścieżka do zapisu pliku OBJ
//         string exportPath = Application.dataPath + "/optimizedMesh.obj";

//         // Wywołanie optymalizacji i eksportu siatki z GameObject
//         meshProcessing.OptimizeAndExportMesh(exportPath, 0.5f);  // Redukcja o 50%

//         Debug.Log("Optimization and export complete. File saved to: " + exportPath);
//     }
// }


//Jak to działa:
// Podłączenie skryptu:
// - Upewnij się, że dodałeś skrypt MeshProcessing do GameObject, który chcesz zoptymalizować.
// - W inspektorze Unity przypisz ten komponent do pola meshProcessing w skrypcie MeshProcessingExample.

// Uruchomienie:
// - Kiedy scena zostanie uruchomiona, skrypt MeshProcessingExample wywoła optymalizację siatki i zapisze ją jako plik .obj w folderze Assets.

// Ścieżka zapisu:
// - W wyniku optymalizacji plik OBJ zostanie zapisany w lokalizacji Assets/optimizedMesh.obj.

// =========================================================================================

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityMeshSimplifier;

public class MeshProcessing : MonoBehaviour
{
    public MeshFilter meshFilter;

    // Metoda do zoptymalizowania i wyeksportowania siatki
    public void OptimizeAndExportMesh(string exportPath, float reductionFactor = 0.5f)
    {
        Mesh originalMesh = meshFilter.mesh;

        // 1. Optymalizacja siatki (redukcja wierzchołków i wielokątów)
        Mesh optimizedMesh = OptimizeMesh(originalMesh, reductionFactor);

        // 2. Walidacja siatki
        if (ValidateMesh(optimizedMesh))
        {
            // 3. Eksport zoptymalizowanej siatki do formatu OBJ
            ExportOBJ(exportPath, optimizedMesh);

            Debug.Log("Mesh optimized and exported successfully.");
        }
        else
        {
            Debug.LogError("Mesh is invalid. Export aborted.");
        }
    }

    // Metoda do optymalizacji siatki
    private Mesh OptimizeMesh(Mesh originalMesh, float reductionFactor)
    {
        Mesh mesh = RemoveUnusedVertices(originalMesh);
        return DecimateMesh(mesh, reductionFactor);  // Redukcja wielokątów
    }

    // Usuwanie nieużywanych wierzchołków
    private Mesh RemoveUnusedVertices(Mesh originalMesh)
    {
        List<Vector3> usedVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        foreach (int triangle in originalMesh.triangles)
        {
            Vector3 vertex = originalMesh.vertices[triangle];
            if (!usedVertices.Contains(vertex))
            {
                usedVertices.Add(vertex);
            }
            newTriangles.Add(usedVertices.IndexOf(vertex));
        }

        Mesh optimizedMesh = new Mesh();
        optimizedMesh.vertices = usedVertices.ToArray();
        optimizedMesh.triangles = newTriangles.ToArray();
        optimizedMesh.RecalculateNormals();

        return optimizedMesh; // Zwracamy zoptymalizowaną siatkę
    }

    // Redukcja liczby wielokątów
    private Mesh DecimateMesh(Mesh originalMesh, float reductionFactor)
    {
        var meshSimplifier = new MeshSimplifier();
        meshSimplifier.Initialize(originalMesh);
        meshSimplifier.SimplifyMesh(reductionFactor);
        return meshSimplifier.ToMesh(); // Zwracamy uproszczoną siatkę
    }

    // Walidacja siatki
    public bool ValidateMesh(Mesh mesh)
    {
        if (mesh.vertexCount == 0 || mesh.triangles.Length == 0)
        {
            Debug.LogError("Mesh is invalid: No vertices or triangles.");
            return false;
        }

        if (mesh.normals.Length != mesh.vertexCount)
        {
            Debug.LogWarning("Mesh normals are not properly calculated. Recalculating normals...");
            mesh.RecalculateNormals();
        }

        return true;
    }

    // Eksport zoptymalizowanej siatki do formatu OBJ
    public void ExportOBJ(string path, Mesh mesh)
    {
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(MeshToString(mesh));
        }
    }

    // Konwersja siatki do formatu OBJ
    private string MeshToString(Mesh mesh)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append("g ").Append(mesh.name).Append("\n");
        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        foreach (Vector3 vn in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", vn.x, vn.y, vn.z));
        }
        foreach (Vector2 uv in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
        }
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            sb.Append(string.Format("f {0} {1} {2}\n",
                mesh.triangles[i] + 1,
                mesh.triangles[i + 1] + 1,
                mesh.triangles[i + 2] + 1));
        }

        return sb.ToString();
    }
}
