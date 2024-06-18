using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ExtrusionAxe : MonoBehaviour
{
    // Lista de puntos de la curva
    //public List<Vector3> curvePoints;
    public int segmentCount = 36;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ExtrudeSurAxe(List<Vector3> curve, Transform parent, Color currentColor)
    {
        if (curve == null || curve.Count < 2)
        {
            Debug.LogError("La lista de puntos de la curva está vacía o tiene menos de dos puntos.");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        transform.SetParent(parent);
        meshFilter.mesh = CreateRevolvedMesh(curve, segmentCount, currentColor);
    }

    public Mesh CreateRevolvedMesh(List<Vector3> curve, int segments, Color currentColor)
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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = new int[0]; // Initially, no triangles
        mesh.RecalculateNormals();

        // Apply the new color to the material
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        meshRenderer.material.color = currentColor;

        return mesh;
    }

    public IEnumerator AnimateTriangles(Mesh mesh, List<Vector3> curve, int segments, int speed)
    {
        int curvePointCount = curve.Count;
        int triangleIndex = 0;
        int[] triangles = new int[segments * (curvePointCount - 1) * 6];

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

                if (triangleIndex % (speed * 6) == 0)
                {
                    mesh.triangles = triangles;
                    mesh.RecalculateNormals();
                    // Wait for the next frame
                    yield return null;
                }
            }
        }
    }

    public void StartAnimation(List<Vector3> curve, int segments, Color currentColor, int speed)
    {
        Mesh mesh = CreateRevolvedMesh(curve, segments, currentColor);
        GetComponent<MeshFilter>().mesh = mesh;
        StartCoroutine(AnimateTriangles(mesh, curve, segments, speed));
    }
}
