using System;
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
            Faces = new List<Face>();
        }
        
        public Vector3 Value { get; set; }
        public List<Edge> Edges { get; }
        public List<Face> Faces { get; }

        // Fixed boundary detection - a vertex is boundary if ANY of its edges are boundary
        public bool IsBoundary => Edges.Any(e => e.Faces.Count < 2);
    }
    
    public class Edge
    {
        public Edge(Vertex startPoint, Vertex endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            startPoint.Edges.Add(this);
            endPoint.Edges.Add(this);
            Faces = new List<Face>();
        }
        
        public Vertex StartPoint { get; }
        public Vertex EndPoint { get; }
        public List<Face> Faces { get; }

        public Vector3 GetMidpoint() => (StartPoint.Value + EndPoint.Value) * 0.5f;

        public Vertex GetOtherVertex(Vertex vertex)
        {
            if (vertex == StartPoint) return EndPoint;
            if (vertex == EndPoint) return StartPoint;
            throw new ArgumentException("Vertex not part of edge");
        }
    }

    public class Face
    {
        public Face(List<Edge> edges, List<Vertex> vertices)
        {
            Edges = edges;
            Vertices = vertices;
            
            foreach (var edge in edges)
            {
                edge.Faces.Add(this);
            }
            
            foreach (var vertex in vertices)
            {
                vertex.Faces.Add(this);
            }
        }

        public List<Edge> Edges { get; }
        public List<Vertex> Vertices { get; }

        public Vector3 GetCentroid()
        {
            Vector3 centroid = Vector3.zero;
            foreach (var vertex in Vertices)
            {
                centroid += vertex.Value;
            }
            return centroid / Vertices.Count;
        }

        public Vector3 CalculateNormal()
        {
            if (Vertices.Count < 3) return Vector3.zero;
            
            // Use Newell's method for robust normal calculation
            Vector3 normal = Vector3.zero;
            
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector3 current = Vertices[i].Value;
                Vector3 next = Vertices[(i + 1) % Vertices.Count].Value;
                
                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }
            
            return normal.normalized;
        }
    }

    public class Geometry
    {
        public List<Face> Faces { get; }
        public List<Edge> Edges { get; }
        public List<Vertex> Vertices { get; }

        public Geometry(List<Face> faces)
        {
            Faces = faces;
            Edges = new List<Edge>();
            Vertices = new List<Vertex>();

            // Build unified edge list and vertex list
            var edgeSet = new HashSet<Edge>();
            var vertexSet = new HashSet<Vertex>();

            foreach (var face in faces)
            {
                foreach (var edge in face.Edges)
                {
                    edgeSet.Add(edge);
                    vertexSet.Add(edge.StartPoint);
                    vertexSet.Add(edge.EndPoint);
                }
            }

            Edges.AddRange(edgeSet);
            Vertices.AddRange(vertexSet);
        }

        public static Geometry GetCube()
        {
            List<Vertex> vertices = new List<Vertex>()
            {
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f)), // 0
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f)), // 1
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f)), // 2
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f)), // 3
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f)), // 4
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f)), // 5
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f)), // 6
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f))  // 7
            };

            List<Edge> edges = new List<Edge>()
            {
                new Edge(vertices[0], vertices[1]),  // 0
                new Edge(vertices[1], vertices[2]),  // 1
                new Edge(vertices[2], vertices[3]),  // 2
                new Edge(vertices[3], vertices[0]),  // 3
                new Edge(vertices[4], vertices[5]),  // 4
                new Edge(vertices[5], vertices[6]),  // 5
                new Edge(vertices[6], vertices[7]),  // 6
                new Edge(vertices[7], vertices[4]),  // 7
                new Edge(vertices[0], vertices[4]),  // 8
                new Edge(vertices[1], vertices[5]),  // 9
                new Edge(vertices[2], vertices[6]),  // 10
                new Edge(vertices[3], vertices[7])   // 11
            };

            List<Face> faces = new List<Face>()
            {
                // Bottom face (y = -0.5, normal pointing down: 0, -1, 0)
                new Face(new List<Edge> { edges[3], edges[2], edges[1], edges[0] }, 
                    new List<Vertex> { vertices[0], vertices[3], vertices[2], vertices[1] }),
                
                // Top face (y = 0.5, normal pointing up: 0, 1, 0)
                new Face(new List<Edge> { edges[4], edges[5], edges[6], edges[7] }, 
                    new List<Vertex> { vertices[4], vertices[5], vertices[6], vertices[7] }),
                
                // Front face (z = -0.5, normal pointing forward: 0, 0, -1)
                new Face(new List<Edge> { edges[0], edges[9], edges[4], edges[8] }, 
                    new List<Vertex> { vertices[0], vertices[1], vertices[5], vertices[4] }),
                
                // Right face (x = 0.5, normal pointing right: 1, 0, 0)
                new Face(new List<Edge> { edges[1], edges[10], edges[5], edges[9] }, 
                    new List<Vertex> { vertices[1], vertices[2], vertices[6], vertices[5] }),
                
                // Back face (z = 0.5, normal pointing back: 0, 0, 1)
                new Face(new List<Edge> { edges[2], edges[11], edges[6], edges[10] }, 
                    new List<Vertex> { vertices[2], vertices[3], vertices[7], vertices[6] }),
                
                // Left face (x = -0.5, normal pointing left: -1, 0, 0)
                new Face(new List<Edge> { edges[3], edges[8], edges[7], edges[11] }, 
                    new List<Vertex> { vertices[3], vertices[0], vertices[4], vertices[7] })
            };

            return new Geometry(faces);
        }

        public Geometry CatmullClarkSubdivision()
        {
            // Step 1: Create face points (centroids)
            Dictionary<Face, Vertex> facePoints = new Dictionary<Face, Vertex>();
            foreach (var face in Faces)
            {
                facePoints[face] = new Vertex(face.GetCentroid());
            }

            // Step 2: Create edge points
            Dictionary<Edge, Vertex> edgePoints = new Dictionary<Edge, Vertex>();
            foreach (var edge in Edges)
            {
                if (edge.Faces.Count == 2)
                {
                    // Interior edge - average of endpoints and adjacent face points
                    Vector3 edgePointPos = (edge.StartPoint.Value + edge.EndPoint.Value + 
                                           facePoints[edge.Faces[0]].Value + facePoints[edge.Faces[1]].Value) / 4f;
                    edgePoints[edge] = new Vertex(edgePointPos);
                }
                else
                {
                    // Boundary edge - midpoint of endpoints
                    edgePoints[edge] = new Vertex(edge.GetMidpoint());
                }
            }

            // Step 3: Update original vertices
            Dictionary<Vertex, Vertex> updatedVertices = new Dictionary<Vertex, Vertex>();
            foreach (var vertex in Vertices)
            {
                if (vertex.IsBoundary)
                {
                    // Boundary vertex handling
                    var boundaryEdges = vertex.Edges.Where(e => e.Faces.Count == 1).ToList();
                    if (boundaryEdges.Count == 2)
                    {
                        // Smooth boundary vertex
                        Vector3 newPos = (vertex.Value * 6f + 
                                        boundaryEdges[0].GetOtherVertex(vertex).Value + 
                                        boundaryEdges[1].GetOtherVertex(vertex).Value) / 8f;
                        updatedVertices[vertex] = new Vertex(newPos);
                    }
                    else
                    {
                        // Corner vertex - keep original position
                        updatedVertices[vertex] = new Vertex(vertex.Value);
                    }
                }
                else
                {
                    // Interior vertex - Catmull-Clark formula
                    Vector3 faceAvg = Vector3.zero;
                    foreach (var face in vertex.Faces)
                    {
                        faceAvg += facePoints[face].Value;
                    }
                    faceAvg /= vertex.Faces.Count;

                    Vector3 edgeAvg = Vector3.zero;
                    foreach (var edge in vertex.Edges)
                    {
                        edgeAvg += edge.GetMidpoint();
                    }
                    edgeAvg /= vertex.Edges.Count;

                    float n = vertex.Faces.Count;
                    Vector3 newPos = (faceAvg + 2f * edgeAvg + (n - 3f) * vertex.Value) / n;
                    updatedVertices[vertex] = new Vertex(newPos);
                }
            }

            // Step 4: Create new faces - FIXED to avoid duplicate edges
            List<Face> newFaces = new List<Face>();
            
            // Track all edges to avoid duplicates
            Dictionary<(Vertex, Vertex), Edge> edgeMap = new Dictionary<(Vertex, Vertex), Edge>();
            
            // Helper function to get or create edge
            Edge GetOrCreateEdge(Vertex v1, Vertex v2)
            {
                var key1 = (v1, v2);
                var key2 = (v2, v1);
                
                if (edgeMap.ContainsKey(key1))
                    return edgeMap[key1];
                if (edgeMap.ContainsKey(key2))
                    return edgeMap[key2];
                
                var newEdge = new Edge(v1, v2);
                edgeMap[key1] = newEdge;
                return newEdge;
            }

            foreach (var face in Faces)
            {
                Vertex facePoint = facePoints[face];
                Vector3 originalNormal = face.CalculateNormal();
                
                for (int i = 0; i < face.Vertices.Count; i++)
                {
                    Vertex currentVertex = face.Vertices[i];
                    Vertex nextVertex = face.Vertices[(i + 1) % face.Vertices.Count];
                    Vertex prevVertex = face.Vertices[i == 0 ? face.Vertices.Count - 1 : i - 1];
                    
                    // Find edges
                    Edge currentEdge = FindEdgeBetweenVertices(currentVertex, nextVertex);
                    Edge previousEdge = FindEdgeBetweenVertices(prevVertex, currentVertex);

                    if (currentEdge != null && previousEdge != null)
                    {
                        Vertex updatedVertex = updatedVertices[currentVertex];
                        Vertex currentEdgePoint = edgePoints[currentEdge];
                        Vertex previousEdgePoint = edgePoints[previousEdge];

                        // Create quad vertices in CCW order to maintain outward normal
                        List<Vertex> quadVertices = new List<Vertex> 
                        { 
                            updatedVertex, 
                            currentEdgePoint, 
                            facePoint, 
                            previousEdgePoint 
                        };

                        // Check if the quad normal matches the original face normal direction
                        Vector3 testNormal = CalculateQuadNormal(quadVertices);
                        if (Vector3.Dot(testNormal, originalNormal) < 0)
                        {
                            // Reverse the order if normal is flipped
                            quadVertices.Reverse();
                        }

                        // Create edges for the quad using the shared edge map
                        Edge edge1 = GetOrCreateEdge(quadVertices[0], quadVertices[1]);
                        Edge edge2 = GetOrCreateEdge(quadVertices[1], quadVertices[2]);
                        Edge edge3 = GetOrCreateEdge(quadVertices[2], quadVertices[3]);
                        Edge edge4 = GetOrCreateEdge(quadVertices[3], quadVertices[0]);

                        Face newFace = new Face(
                            new List<Edge> { edge1, edge2, edge3, edge4 },
                            quadVertices
                        );

                        newFaces.Add(newFace);
                    }
                }
            }

            return new Geometry(newFaces);
        }

        private Edge FindEdgeBetweenVertices(Vertex v1, Vertex v2)
        {
            foreach (var edge in v1.Edges)
            {
                if ((edge.StartPoint == v1 && edge.EndPoint == v2) ||
                    (edge.StartPoint == v2 && edge.EndPoint == v1))
                {
                    return edge;
                }
            }
            return null;
        }

        private Vector3 CalculateQuadNormal(List<Vertex> vertices)
        {
            if (vertices.Count != 4) return Vector3.zero;
            
            // Use Newell's method for robust normal calculation
            Vector3 normal = Vector3.zero;
            
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i].Value;
                Vector3 next = vertices[(i + 1) % vertices.Count].Value;
                
                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }
            
            return normal.normalized;
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
    
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> triangles = new List<int>();
    
            int vertexIndex = 0;

            foreach (var face in Faces)
            {
                if (face.Vertices.Count < 3) continue;

                // Add all face vertices to mesh
                List<int> faceIndices = new List<int>();
                for (int i = 0; i < face.Vertices.Count; i++)
                {
                    meshVertices.Add(face.Vertices[i].Value);
                    faceIndices.Add(vertexIndex++);
                }

                // Triangulate with correct winding order
                if (face.Vertices.Count == 3)
                {
                    // Triangle - add directly (already in correct CCW order)
                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[1]);
                    triangles.Add(faceIndices[2]);
                }
                else if (face.Vertices.Count == 4)
                {
                    // Quad - split into two triangles maintaining CCW order
                    // First triangle: 0-1-2
                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[1]);
                    triangles.Add(faceIndices[2]);
            
                    // Second triangle: 0-2-3
                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[2]);
                    triangles.Add(faceIndices[3]);
                }
                else
                {
                    // N-gon - fan triangulation from first vertex
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        triangles.Add(faceIndices[0]);
                        triangles.Add(faceIndices[i]);
                        triangles.Add(faceIndices[i + 1]);
                    }
                }
            }

            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}