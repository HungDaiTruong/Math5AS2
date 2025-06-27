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

        public Geometry(Mesh mesh)
        {
            Vector3[] meshVertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Merge vertices that are at the same position (with small tolerance)
            const float tolerance = 1e-6f;
            Dictionary<int, int> vertexRemap = new Dictionary<int, int>();
            List<Vector3> uniquePositions = new List<Vector3>();

            for (int i = 0; i < meshVertices.Length; i++)
            {
                int existingIndex = -1;
                for (int j = 0; j < uniquePositions.Count; j++)
                {
                    if (Vector3.Distance(meshVertices[i], uniquePositions[j]) < tolerance)
                    {
                        existingIndex = j;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    vertexRemap[i] = existingIndex;
                }
                else
                {
                    vertexRemap[i] = uniquePositions.Count;
                    uniquePositions.Add(meshVertices[i]);
                }
            }

            // Create vertex objects for unique positions only
            Vertices = new List<Vertex>();
            for (int i = 0; i < uniquePositions.Count; i++)
            {
                Vertices.Add(new Vertex(uniquePositions[i]));
            }

            // Track edges to avoid duplicates
            Dictionary<(int, int), Edge> edgeDict = new Dictionary<(int, int), Edge>();

            Edge GetOrCreateEdge(int v1Index, int v2Index)
            {
                var key = v1Index < v2Index ? (v1Index, v2Index) : (v2Index, v1Index);

                if (!edgeDict.ContainsKey(key))
                {
                    edgeDict[key] = new Edge(Vertices[v1Index], Vertices[v2Index]);
                }

                return edgeDict[key];
            }

            // Create faces from triangles using remapped indices
            Faces = new List<Face>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = vertexRemap[triangles[i]];
                int v1 = vertexRemap[triangles[i + 1]];
                int v2 = vertexRemap[triangles[i + 2]];

                // Skip degenerate triangles
                if (v0 == v1 || v1 == v2 || v2 == v0) continue;

                Edge edge0 = GetOrCreateEdge(v0, v1);
                Edge edge1 = GetOrCreateEdge(v1, v2);
                Edge edge2 = GetOrCreateEdge(v2, v0);

                List<Edge> faceEdges = new List<Edge> { edge0, edge1, edge2 };
                List<Vertex> faceVertices = new List<Vertex> { Vertices[v0], Vertices[v1], Vertices[v2] };

                Faces.Add(new Face(faceEdges, faceVertices));
            }

            Edges = new List<Edge>(edgeDict.Values);
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
                new Face(new List<Edge> { edges[3], edges[2], edges[1], edges[0] },
                    new List<Vertex> { vertices[0], vertices[3], vertices[2], vertices[1] }),
                new Face(new List<Edge> { edges[4], edges[5], edges[6], edges[7] },
                    new List<Vertex> { vertices[4], vertices[5], vertices[6], vertices[7] }),
                new Face(new List<Edge> { edges[0], edges[9], edges[4], edges[8] },
                    new List<Vertex> { vertices[0], vertices[1], vertices[5], vertices[4] }),
                new Face(new List<Edge> { edges[1], edges[10], edges[5], edges[9] },
                    new List<Vertex> { vertices[1], vertices[2], vertices[6], vertices[5] }),
                new Face(new List<Edge> { edges[2], edges[11], edges[6], edges[10] },
                    new List<Vertex> { vertices[2], vertices[3], vertices[7], vertices[6] }),
                new Face(new List<Edge> { edges[3], edges[8], edges[7], edges[11] },
                    new List<Vertex> { vertices[3], vertices[0], vertices[4], vertices[7] })
            };

            return new Geometry(faces);
        }

        public Geometry LoopSubdivision()
        {
            // Loop subdivision only works on triangle meshes
            if (Faces.Any(f => f.Vertices.Count != 3))
            {
                throw new InvalidOperationException("Loop subdivision only works on triangle meshes");
            }

            // Step 1: Compute new edge points
            Dictionary<Edge, Vertex> edgePoints = new Dictionary<Edge, Vertex>();
            foreach (var edge in Edges)
            {
                Vector3 newEdgePoint;

                if (edge.Faces.Count == 2)
                {
                    // Interior edge: e = 3/8 * (v1 + v2) + 1/8 * (vleft + vright)
                    var v1 = edge.StartPoint.Value;
                    var v2 = edge.EndPoint.Value;

                    // Find the other two vertices (vleft and vright)
                    var face1 = edge.Faces[0];
                    var face2 = edge.Faces[1];

                    var vleft = GetThirdVertex(face1, edge.StartPoint, edge.EndPoint);
                    var vright = GetThirdVertex(face2, edge.StartPoint, edge.EndPoint);

                    newEdgePoint = (3f / 8f) * (v1 + v2) + (1f / 8f) * (vleft + vright);
                }
                else
                {
                    // Boundary edge: simple midpoint
                    newEdgePoint = edge.GetMidpoint();
                }

                edgePoints[edge] = new Vertex(newEdgePoint);
            }

            // Step 2: Compute new vertex points
            Dictionary<Vertex, Vertex> newVertexPoints = new Dictionary<Vertex, Vertex>();
            foreach (var vertex in Vertices)
            {
                Vector3 newVertexPoint;

                if (vertex.IsBoundary)
                {
                    // Boundary vertex: v' = 3/4 * v + 1/8 * (neighbor1 + neighbor2)
                    var boundaryEdges = vertex.Edges.Where(e => e.Faces.Count == 1).ToList();
                    if (boundaryEdges.Count == 2)
                    {
                        var neighbor1 = boundaryEdges[0].GetOtherVertex(vertex).Value;
                        var neighbor2 = boundaryEdges[1].GetOtherVertex(vertex).Value;
                        newVertexPoint = (3f / 4f) * vertex.Value + (1f / 8f) * (neighbor1 + neighbor2);
                    }
                    else
                    {
                        newVertexPoint = vertex.Value; // Keep unchanged for complex boundary cases
                    }
                }
                else
                {
                    // Interior vertex: v' = (1-nα) * v + α * Σ(adjacent vertices)
                    int n = vertex.Edges.Count;
                    float alpha;

                    if (n == 3)
                    {
                        alpha = 3f / 16f;
                    }
                    else
                    {
                        // α = (1/n) * [5/8 - (3/8 + 1/4 * cos(2π/n))²]
                        float cosValue = Mathf.Cos(2f * Mathf.PI / n);
                        float temp = 3f / 8f + (1f / 4f) * cosValue;
                        alpha = (1f / n) * (5f / 8f - temp * temp);
                    }

                    Vector3 neighborSum = Vector3.zero;
                    foreach (var edge in vertex.Edges)
                    {
                        neighborSum += edge.GetOtherVertex(vertex).Value;
                    }

                    newVertexPoint = (1f - n * alpha) * vertex.Value + alpha * neighborSum;
                }

                newVertexPoints[vertex] = new Vertex(newVertexPoint);
            }

            // Step 3: Create new faces (1-to-4 subdivision)
            List<Face> newFaces = new List<Face>();
            Dictionary<(Vertex, Vertex), Edge> edgeMap = new Dictionary<(Vertex, Vertex), Edge>();

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
                // Get the three original vertices and their new positions
                var v0 = newVertexPoints[face.Vertices[0]];
                var v1 = newVertexPoints[face.Vertices[1]];
                var v2 = newVertexPoints[face.Vertices[2]];

                // Get the three edge points - find edges in correct order
                var e01 = edgePoints[FindEdgeBetweenVertices(face.Vertices[0], face.Vertices[1])];
                var e12 = edgePoints[FindEdgeBetweenVertices(face.Vertices[1], face.Vertices[2])];
                var e20 = edgePoints[FindEdgeBetweenVertices(face.Vertices[2], face.Vertices[0])];

                // Create 4 new triangles with consistent winding order

                // Corner triangle 1: v0, e01, e20
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, v0, e01, e20);

                // Corner triangle 2: v1, e12, e01
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, v1, e12, e01);

                // Corner triangle 3: v2, e20, e12  
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, v2, e20, e12);

                // Center triangle: e01, e12, e20
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, e01, e12, e20);
            }

            return new Geometry(newFaces);
        }

        private void CreateTriangleFace(List<Face> newFaces, Dictionary<(Vertex, Vertex), Edge> edgeMap,
            Func<Vertex, Vertex, Edge> getOrCreateEdge, Vertex v1, Vertex v2, Vertex v3)
        {
            var edge1 = getOrCreateEdge(v1, v2);
            var edge2 = getOrCreateEdge(v2, v3);
            var edge3 = getOrCreateEdge(v3, v1);

            newFaces.Add(new Face(
                new List<Edge> { edge1, edge2, edge3 },
                new List<Vertex> { v1, v2, v3 }
            ));
        }

        private Vector3 GetThirdVertex(Face face, Vertex v1, Vertex v2)
        {
            foreach (var vertex in face.Vertices)
            {
                if (vertex != v1 && vertex != v2)
                {
                    return vertex.Value;
                }
            }
            throw new ArgumentException("Could not find third vertex in triangle");
        }

        public Geometry KobbeltSubdivision()
        {
            // Kobbelt subdivision (√3-subdivision) only works on triangle meshes
            if (Faces.Any(f => f.Vertices.Count != 3))
            {
                throw new InvalidOperationException("Kobbelt subdivision only works on triangle meshes");
            }

            // Step 1: Create face points (centroids of triangles)
            Dictionary<Face, Vertex> facePoints = new Dictionary<Face, Vertex>();
            foreach (var face in Faces)
            {
                facePoints[face] = new Vertex(face.GetCentroid());
            }

            // Step 2: Update original vertex positions using relaxation
            Dictionary<Vertex, Vertex> updatedVertices = new Dictionary<Vertex, Vertex>();
            foreach (var vertex in Vertices)
            {
                Vector3 newVertexPoint;

                if (vertex.IsBoundary)
                {
                    // Boundary vertices: simple average of boundary neighbors
                    var boundaryEdges = vertex.Edges.Where(e => e.Faces.Count == 1).ToList();
                    if (boundaryEdges.Count == 2)
                    {
                        var neighbor1 = boundaryEdges[0].GetOtherVertex(vertex).Value;
                        var neighbor2 = boundaryEdges[1].GetOtherVertex(vertex).Value;
                        newVertexPoint = (4f * vertex.Value + neighbor1 + neighbor2) / 6f;
                    }
                    else
                    {
                        newVertexPoint = vertex.Value; // Keep unchanged for complex boundary cases
                    }
                }
                else
                {
                    // Interior vertices: relaxation formula
                    int n = vertex.Faces.Count; // valence (number of adjacent faces)
                    float alpha_n = (4f - 2f * Mathf.Cos(2f * Mathf.PI / n)) / 9f;

                    // Sum of adjacent vertex positions
                    Vector3 neighborSum = Vector3.zero;
                    HashSet<Vertex> neighbors = new HashSet<Vertex>();

                    foreach (var edge in vertex.Edges)
                    {
                        neighbors.Add(edge.GetOtherVertex(vertex));
                    }

                    foreach (var neighbor in neighbors)
                    {
                        neighborSum += neighbor.Value;
                    }

                    newVertexPoint = (1f - alpha_n) * vertex.Value + (alpha_n / n) * neighborSum;
                }

                updatedVertices[vertex] = new Vertex(newVertexPoint);
            }

            // Step 3: Create new triangular faces - one for each original triangle
            // Each original triangle is replaced by 3 triangles connecting the face point to the updated vertices
            List<Face> newFaces = new List<Face>();
            Dictionary<(Vertex, Vertex), Edge> edgeMap = new Dictionary<(Vertex, Vertex), Edge>();

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

            // Store original edge information before creating new faces
            Dictionary<(Vertex, Vertex), bool> originalEdges = new Dictionary<(Vertex, Vertex), bool>();
            foreach (var edge in Edges)
            {
                var v1 = updatedVertices[edge.StartPoint];
                var v2 = updatedVertices[edge.EndPoint];
                originalEdges[(v1, v2)] = true;
                originalEdges[(v2, v1)] = true;
            }

            foreach (var face in Faces)
            {
                Vertex facePoint = facePoints[face];

                // Get the three updated vertices of the original triangle
                var v0 = updatedVertices[face.Vertices[0]];
                var v1 = updatedVertices[face.Vertices[1]];
                var v2 = updatedVertices[face.Vertices[2]];

                // Create three new triangles: face point connected to each edge of the original triangle
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, facePoint, v0, v1);
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, facePoint, v1, v2);
                CreateTriangleFace(newFaces, edgeMap, GetOrCreateEdge, facePoint, v2, v0);
            }

            // Step 4: Edge flipping phase
            // Collect edges that connect updated original vertices and need to be flipped
            List<Edge> edgesToFlip = new List<Edge>();
            foreach (var kvp in edgeMap)
            {
                var edge = kvp.Value;
                var key = kvp.Key;

                // Check if this edge connects two original vertices (not face points)
                bool connectsOriginalVertices = originalEdges.ContainsKey(key);

                if (connectsOriginalVertices && edge.Faces.Count == 2)
                {
                    // Only flip if both adjacent faces contain a face point
                    bool hasFacePoints = edge.Faces.All(f => f.Vertices.Any(v => facePoints.ContainsValue(v)));
                    if (hasFacePoints)
                    {
                        edgesToFlip.Add(edge);
                    }
                }
            }

            // Perform edge flipping with proper cleanup
            foreach (var edge in edgesToFlip)
            {
                if (edge.Faces.Count == 2)
                {
                    FlipEdgeProper(edge, newFaces, edgeMap);
                }
            }

            return new Geometry(newFaces);
        }

        private void FlipEdgeProper(Edge edge, List<Face> faces, Dictionary<(Vertex, Vertex), Edge> edgeMap)
        {
            if (edge.Faces.Count != 2) return;

            var face1 = edge.Faces[0];
            var face2 = edge.Faces[1];

            // Find the vertices that are not part of the edge
            Vertex opposite1 = null, opposite2 = null;

            foreach (var vertex in face1.Vertices)
            {
                if (vertex != edge.StartPoint && vertex != edge.EndPoint)
                {
                    opposite1 = vertex;
                    break;
                }
            }

            foreach (var vertex in face2.Vertices)
            {
                if (vertex != edge.StartPoint && vertex != edge.EndPoint)
                {
                    opposite2 = vertex;
                    break;
                }
            }

            if (opposite1 == null || opposite2 == null) return;

            // Remove the old faces from the list
            faces.Remove(face1);
            faces.Remove(face2);

            // Clear the face references from the old edge
            edge.Faces.Clear();

            // Remove all edges of the old faces from vertex edge lists and edge map
            CleanupFaceEdges(face1, edgeMap);
            CleanupFaceEdges(face2, edgeMap);

            // Helper function to get or create edge with proper cleanup
            Edge GetOrCreateEdgeClean(Vertex v1, Vertex v2)
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

            // Create two new triangular faces with the flipped edge
            // Triangle 1: opposite1, edge.StartPoint, opposite2
            var e1 = GetOrCreateEdgeClean(opposite1, edge.StartPoint);
            var e2 = GetOrCreateEdgeClean(edge.StartPoint, opposite2);
            var e3 = GetOrCreateEdgeClean(opposite2, opposite1);

            var newFace1 = new Face(
                new List<Edge> { e1, e2, e3 },
                new List<Vertex> { opposite1, edge.StartPoint, opposite2 }
            );

            // Triangle 2: opposite1, opposite2, edge.EndPoint  
            var e4 = GetOrCreateEdgeClean(opposite1, opposite2);
            var e5 = GetOrCreateEdgeClean(opposite2, edge.EndPoint);
            var e6 = GetOrCreateEdgeClean(edge.EndPoint, opposite1);

            var newFace2 = new Face(
                new List<Edge> { e4, e5, e6 },
                new List<Vertex> { opposite1, opposite2, edge.EndPoint }
            );

            faces.Add(newFace1);
            faces.Add(newFace2);
        }

        private void CleanupFaceEdges(Face face, Dictionary<(Vertex, Vertex), Edge> edgeMap)
        {
            foreach (var edge in face.Edges)
            {
                // Remove this face from the edge's face list
                edge.Faces.Remove(face);

                // If the edge has no more faces, remove it from vertices and edge map
                if (edge.Faces.Count == 0)
                {
                    edge.StartPoint.Edges.Remove(edge);
                    edge.EndPoint.Edges.Remove(edge);

                    // Remove from edge map
                    var key1 = (edge.StartPoint, edge.EndPoint);
                    var key2 = (edge.EndPoint, edge.StartPoint);
                    edgeMap.Remove(key1);
                    edgeMap.Remove(key2);
                }
            }

            // Clear vertex face references
            foreach (var vertex in face.Vertices)
            {
                vertex.Faces.Remove(face);
            }
        }

        public Geometry CatmullClarkSubdivision()
        {
            Dictionary<Face, Vertex> facePoints = new Dictionary<Face, Vertex>();
            foreach (var face in Faces)
            {
                facePoints[face] = new Vertex(face.GetCentroid());
            }

            Dictionary<Edge, Vertex> edgePoints = new Dictionary<Edge, Vertex>();
            foreach (var edge in Edges)
            {
                if (edge.Faces.Count == 2)
                {
                    Vector3 edgePointPos = (edge.StartPoint.Value + edge.EndPoint.Value +
                                           facePoints[edge.Faces[0]].Value + facePoints[edge.Faces[1]].Value) / 4f;
                    edgePoints[edge] = new Vertex(edgePointPos);
                }
                else
                {
                    edgePoints[edge] = new Vertex(edge.GetMidpoint());
                }
            }

            Dictionary<Vertex, Vertex> updatedVertices = new Dictionary<Vertex, Vertex>();
            foreach (var vertex in Vertices)
            {
                if (vertex.IsBoundary)
                {
                    var boundaryEdges = vertex.Edges.Where(e => e.Faces.Count == 1).ToList();
                    if (boundaryEdges.Count == 2)
                    {
                        Vector3 newPos = (vertex.Value * 6f +
                                        boundaryEdges[0].GetOtherVertex(vertex).Value +
                                        boundaryEdges[1].GetOtherVertex(vertex).Value) / 8f;
                        updatedVertices[vertex] = new Vertex(newPos);
                    }
                    else
                    {
                        updatedVertices[vertex] = new Vertex(vertex.Value);
                    }
                }
                else
                {
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

            List<Face> newFaces = new List<Face>();
            Dictionary<(Vertex, Vertex), Edge> edgeMap = new Dictionary<(Vertex, Vertex), Edge>();

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

                    Edge currentEdge = FindEdgeBetweenVertices(currentVertex, nextVertex);
                    Edge previousEdge = FindEdgeBetweenVertices(prevVertex, currentVertex);

                    if (currentEdge != null && previousEdge != null)
                    {
                        Vertex updatedVertex = updatedVertices[currentVertex];
                        Vertex currentEdgePoint = edgePoints[currentEdge];
                        Vertex previousEdgePoint = edgePoints[previousEdge];

                        List<Vertex> quadVertices = new List<Vertex>
                        {
                            updatedVertex,
                            currentEdgePoint,
                            facePoint,
                            previousEdgePoint
                        };

                        Vector3 testNormal = CalculateQuadNormal(quadVertices);
                        if (Vector3.Dot(testNormal, originalNormal) < 0)
                        {
                            quadVertices.Reverse();
                        }

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

                List<int> faceIndices = new List<int>();
                for (int i = 0; i < face.Vertices.Count; i++)
                {
                    meshVertices.Add(face.Vertices[i].Value);
                    faceIndices.Add(vertexIndex++);
                }

                if (face.Vertices.Count == 3)
                {
                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[1]);
                    triangles.Add(faceIndices[2]);
                }
                else if (face.Vertices.Count == 4)
                {
                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[1]);
                    triangles.Add(faceIndices[2]);

                    triangles.Add(faceIndices[0]);
                    triangles.Add(faceIndices[2]);
                    triangles.Add(faceIndices[3]);
                }
                else
                {
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