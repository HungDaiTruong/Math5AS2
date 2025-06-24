using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    public class Vertex
    {
        public Vertex(Vector3 point)
        {
            Value = point;
            Edges = new List<Edge>();
        }
        
        public Vector3 Value { get; set; } = Vector3.zero;

        public List<Edge> Edges { get; } = new();

        /**
         * Makes and edge and it to edge list of both v1 and v2
         */
        public Edge AddEdge(Vertex v1, Vertex v2)
        {
            var edge = new Edge(v1, v2);
            Edges.Add(edge);
            v2.Edges.Add(edge);
            return edge;
        }
    }

    public class Edge
    {
        public Edge(Vertex startPoint, Vertex endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
        
        public Vertex StartPoint { get; private set; }
        public Vertex EndPoint { get; private set; }
        
        public FaceSet Faces { get; set; }
        
    }

    public class FaceSet
    {
        public FaceSet(Face ff, Face sf)
        {
            FirstFace = ff;
            SecondFace = sf;
        }
        
        public Face FirstFace { get; set; }
        public Face SecondFace { get; set; }
    }

    public class Face
    {
        public Face()
        {
        }
        
        public List<Edge> Edges { get; set; }
    }

    public class Geometry
    {
        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public List<Face> Faces { get; private set; } = new List<Face>();

        public static Geometry GetCube()
        {
            var geometry = new Geometry();
        
            // Define all 8 vertices of a cube
            geometry.Vertices = new List<Vertex>
            {
                // Front face
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f)), // 0
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f)), // 1
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f)), // 2
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f)), // 3
        
                // Back face
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f)), // 4
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f)), // 5
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f)), // 6
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f))  // 7
            };

            int[] triangles = {
                // Front face
                0, 1, 2,  0, 2, 3,
                // Back face
                4, 6, 5,  4, 7, 6,
                // Top face
                3, 2, 6,  3, 6, 7,
                // Bottom face
                0, 5, 1,  0, 4, 5,
                // Left face
                0, 3, 7,  0, 7, 4,
                // Right face
                1, 5, 6,  1, 6, 2
            };

            // Create faces (optional - if you need to maintain your Face structure)
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var face = new Face();
                var v0 = geometry.Vertices[triangles[i]];
                var v1 = geometry.Vertices[triangles[i+1]];
                var v2 = geometry.Vertices[triangles[i+2]];
            
                // Create edges and add to face
                var e0 = v0.AddEdge(v0, v1);
                var e1 = v1.AddEdge(v1, v2);
                var e2 = v0.AddEdge(v2, v0);
            
                face.Edges = new List<Edge> { 
                    e0, e1, e2
                };
            
                geometry.Faces.Add(face);
            }

            return geometry;
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();
        
            // Convert vertices to Vector3 array
            mesh.vertices = Vertices.Select(v => v.Value).ToArray();
        
            // Generate triangles from faces
            List<int> triangles = new List<int>();
            Dictionary<Vertex, int> vertexIndices = new Dictionary<Vertex, int>();
        
            // Create vertex index mapping
            for (int i = 0; i < Vertices.Count; i++)
            {
                vertexIndices[Vertices[i]] = i;
            }
        
            // Convert each face to triangles
            foreach (var face in Faces)
            {
                if (face.Edges.Count >= 3) // At least a triangle
                {
                    // Get the first 3 vertices to form a triangle
                    var v0 = face.Edges[0].StartPoint;
                    var v1 = face.Edges[0].EndPoint;
                    var v2 = face.Edges[1].EndPoint;
                
                    triangles.Add(vertexIndices[v0]);
                    triangles.Add(vertexIndices[v1]);
                    triangles.Add(vertexIndices[v2]);
                
                    // For quads or n-gons, you'd need triangulation here
                }
            }
        
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        
            return mesh;
        }
    }

}