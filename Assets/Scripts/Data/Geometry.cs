using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    /**
     * Class responsible for representing a point in space
     */
    
    public class Vertex
    {
        public Vertex(Vector3 point)
        {
            Value = point;
            Edges = new List<Edge>();
            Faces = new List<Face>();
        }
        
        public Vector3 Value { get; set; }

        public List<Edge> Edges { get; }

        public List<Face> Faces { get; private set; }
    }
    
    /**
     * Class responsible for representing an edge in space
     */

    public class Edge
    {
        private const short MaxFaces = 2;
        public Edge(Vertex startPoint, Vertex endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Faces = new List<Face>(MaxFaces);
        }
        
        public Vertex StartPoint { get; private set; }
        public Vertex EndPoint { get; private set; }
        
        public List<Face> Faces { get; private set; }
    }

    /**
     * Class responsible for representing a face in space
     */
    
    public class Face
    {
        public Face(List<Edge> edges)
        {
            Edges = edges;
            Vertices = new List<Vertex>();

            IsClosed = IsMeshClosed();

            foreach (Edge edge in Edges)
            {
                if (!Vertices.Contains(edge.StartPoint))
                {
                    Vertices.Add(edge.StartPoint);
                }

                if (!Vertices.Contains(edge.EndPoint))
                {
                    Vertices.Add(edge.EndPoint);
                }
            }
        }

        public List<Edge> Edges { get; private set; }
        
        public List<Vertex> Vertices { get; private set; }

        public bool IsClosed { get; private set; }

        private bool IsMeshClosed()
        {
            bool result = true;
            
            Vertex startPoint = Edges[0].StartPoint;
            Vertex lastEndPoint = Edges[0].EndPoint;
            
            for (int i = 1; i < Edges.Count; i++)
            {
                // If last edge
                if (i == Edges.Count - 1)
                {
                    result = Edges[i].StartPoint.Value == lastEndPoint.Value &&
                             Edges[i].EndPoint.Value == startPoint.Value;
                    break;
                }
                
                // We check that the currently evaluated edge has a start point similar to last edge endpoint
                // (Do they connect ? If not the face is not closed ...)
                if (Edges[i].StartPoint.Value != lastEndPoint.Value)
                {
                    result = false;
                    break;
                }
                
                //If the edge connects to the last one, we take the current edge EndPoint to check the next edge.
                lastEndPoint = Edges[i].EndPoint;
            }

            return result;
        }
    }

    public class Geometry
    {
        public List<Face> Faces { get; private set; }
        
        public Geometry(List<Face> faces)
        {
            Faces = faces;
        }

        public static Geometry GetCube()
        {
            List<Vertex> vertices = new List<Vertex>()
            {
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f)), // 0: back-left-bottom
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f)), // 1: back-right-bottom
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f)), // 2: front-right-bottom
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f)), // 3: front-left-bottom

                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f)), // 4: back-left-top
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f)), // 5: back-right-top
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f)), // 6: front-right-top
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f))  // 7: front-left-top
            };
            
            List<Face> faces = new List<Face>
            {
                new Face(new List<Edge>() {new Edge(vertices[6],vertices[7]), new Edge(vertices[7], vertices[3]), new Edge(vertices[3], vertices[6])}),
                new Face(new List<Edge>() {new Edge(vertices[6],vertices[3]), new Edge(vertices[3], vertices[2]), new Edge(vertices[2], vertices[6])}),
                
                new Face(new List<Edge>() {new Edge(vertices[4],vertices[5]), new Edge(vertices[5], vertices[1]), new Edge(vertices[1], vertices[4])}),
                new Face(new List<Edge>() {new Edge(vertices[4],vertices[1]), new Edge(vertices[1], vertices[0]), new Edge(vertices[0], vertices[4])}),
                
                new Face(new List<Edge>() {new Edge(vertices[5],vertices[6]), new Edge(vertices[6], vertices[2]), new Edge(vertices[2], vertices[5])}),
                new Face(new List<Edge>() {new Edge(vertices[5],vertices[2]), new Edge(vertices[2], vertices[1]), new Edge(vertices[1], vertices[5])}),
                
                new Face(new List<Edge>() {new Edge(vertices[7],vertices[4]), new Edge(vertices[4], vertices[0]), new Edge(vertices[0], vertices[7])}),
                new Face(new List<Edge>() {new Edge(vertices[7],vertices[0]), new Edge(vertices[0], vertices[3]), new Edge(vertices[3], vertices[7])}),
                
                new Face(new List<Edge>() {new Edge(vertices[5],vertices[4]), new Edge(vertices[4], vertices[7]), new Edge(vertices[7], vertices[5])}),
                new Face(new List<Edge>() {new Edge(vertices[5],vertices[7]), new Edge(vertices[7], vertices[6]), new Edge(vertices[6], vertices[5])}),
                
                new Face(new List<Edge>() {new Edge(vertices[2],vertices[3]), new Edge(vertices[3], vertices[1]), new Edge(vertices[1], vertices[2])}),
                new Face(new List<Edge>() {new Edge(vertices[3],vertices[0]), new Edge(vertices[0], vertices[1]), new Edge(vertices[1], vertices[3])}),
            };
            Geometry cube = new Geometry(faces);

            return cube;
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            
            List<Vertex> vertices = new List<Vertex>();
            List<int> triangles = new List<int>();

            foreach (Face face in Faces)
            {
                List<int> usedIndices = new List<int>();

                foreach (Vertex vertex in face.Vertices)
                {
                    var idx = vertices.FindIndex(v => v.Value == vertex.Value);
                    if (idx == -1)
                    {
                        vertices.Add(vertex);
                        triangles.Add(vertices.Count - 1);
                        usedIndices.Add(vertices.Count - 1);
                    }
                    else if (!usedIndices.Contains(idx))
                    {
                        triangles.Add(idx);
                        usedIndices.Add(idx);
                    }
                }
            }

            mesh.vertices = vertices.Select(v => v.Value).ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }

}