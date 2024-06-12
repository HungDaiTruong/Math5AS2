using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ExtrusionAxe : MonoBehaviour
{
    // Lista de puntos de la curva
    //public List<Vector3> curvePoints;
    public int segmentCount = 36;

    public void ExtrudeSurAxe(List<Vector3> curve, Transform parent)
    {
        if (curve == null || curve.Count < 2)
        {
            Debug.LogError("La lista de puntos de la curva está vacía o tiene menos de dos puntos.");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        transform.SetParent(parent);
        meshFilter.mesh = CreateRevolvedMesh(curve, segmentCount);
    }

    Mesh CreateRevolvedMesh(List<Vector3> curve, int segments)
    {
        int curvePointCount = curve.Count;
        int verticesCount = curvePointCount * (segments + 1);

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * (curvePointCount - 1) * 6];
        Vector2[] uvs = new Vector2[verticesCount];

        // Crear vértices
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);

            for (int j = 0; j < curvePointCount; j++)
            {
                Vector3 point = curve[j];
                int vertexIndex = i * curvePointCount + j;
                vertices[vertexIndex] = rotation * point;
                uvs[vertexIndex] = new Vector2((float)i / segments, (float)j / (curvePointCount - 1));
            }
        }

        // Crear triángulos
        int triangleIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < curvePointCount - 1; j++)
            {
                int current = i * curvePointCount + j;
                int next = (i + 1) * curvePointCount + j;

                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = current + 1;

                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = next + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}
