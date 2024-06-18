using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Extrusion : MonoBehaviour
{
    public List<Vector3> curvePoints; // Points de la courbe de Bézier en 3D
    public float height = 1.0f; // Hauteur d'extrusion
    public float scale = 1.0f; // Coefficient d'agrandissement ou de réduction
    private Mesh mesh;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        GenerateMesh();
    }
    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Générer les sommets
        foreach (Vector3 point in curvePoints)
        {
            vertices.Add(new Vector3(point.x, point.y, 0)); // Bas de l'extrusion
            vertices.Add(new Vector3(point.x * scale, point.y * scale, height)); // Haut de l'extrusion
        }

        // Générer les triangles pour les faces latérales
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            int baseIndex = i * 2;
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);

            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }

        // Générer les triangles pour les faces supérieure et inférieure
        //AddCap(triangles, vertices, true); // Top face
        //AddCap(triangles, vertices, false); // Bottom face

        // Appliquer les vertices et triangles au mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    /* void AddCap(List<int> triangles, List<Vector3> vertices, bool isTop)
    {
        int startIndex = isTop ? 1 : 0;
        int step = 2;

        for (int i = 0; i < curvePoints.Count - 2; i++)
        {
            triangles.Add(startIndex);
            triangles.Add(startIndex + step * (i + 1));
            triangles.Add(startIndex + step * (i + 2));
        }
    } */

    // Méthode pour mettre à jour l'extrusion si nécessaire
    public void UpdateExtrusion(List<Vector3> newCurvePoints, Color currentColor, Transform parent)
    {
        curvePoints = new List<Vector3>(newCurvePoints); // Copy the points to ensure isolation

        // Apply the new color to the material
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        meshRenderer.material.color = currentColor;

        // Set the parent
        transform.SetParent(parent);

        GenerateMesh();
    }
}
