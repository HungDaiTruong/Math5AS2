using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtensions
{
    public static void ManuallyRecalculateNormals(this Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = new Vector3[vertices.Length];

        // Initialize normals to zero
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.zero;
        }

        // Calculate face normals and accumulate them to the vertex normals
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index0 = triangles[i];
            int index1 = triangles[i + 1];
            int index2 = triangles[i + 2];

            Vector3 v0 = vertices[index0];
            Vector3 v1 = vertices[index1];
            Vector3 v2 = vertices[index2];

            Vector3 faceNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            normals[index0] += faceNormal;
            normals[index1] += faceNormal;
            normals[index2] += faceNormal;
        }

        // Normalize the accumulated normals
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = normals[i].normalized;
        }

        // Assign the new normals to the mesh
        mesh.normals = normals;
    }
}

