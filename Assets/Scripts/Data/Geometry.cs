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
            startPoint.Edges.Add(this);
            endPoint.Edges.Add(this);
            Faces = new List<Face>(MaxFaces);
        }
        
        public Vertex StartPoint { get; private set; }
        public Vertex EndPoint { get; private set; }
        
        public List<Face> Faces { get; private set; }

        public Vector3 GetMidpoint()
        {
            return (StartPoint.Value + EndPoint.Value) * 0.5f;
        }

        bool IsPartOfFace(Face face)
        {
            foreach (Edge edge in face.Edges)
            {
                if (edge.StartPoint == StartPoint && edge.EndPoint == EndPoint ||
                    edge.StartPoint == EndPoint && edge.EndPoint == StartPoint)
                {
                    edge.Faces.Add(face);
                    return true;
                }
            }
            return false;
        }
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

        public Vector3 GetCentroid()
        {
            Vector3 result = Vector3.zero;
            foreach (Vertex vertex in Vertices)
            {
                result += vertex.Value;
            }
            result /= Vertices.Count;
            return result;
        }
    }

    public class Geometry
    {
        public List<Face> Faces { get; private set; }
        
        public Geometry(List<Face> faces)
        {
            Faces = faces;

            List<Edge> existingEdges = new List<Edge>();
            foreach (Face face in Faces)
            {
                existingEdges.AddRange(face.Edges);
            }

            foreach (Edge existingEdge in existingEdges)
            {
                
            }
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

        public Geometry LoopSubdivision()
        {
            List<Face> faces = new List<Face>();
            Geometry geometry = new Geometry(faces);
            
            return geometry;
        }
        
        private struct VertexPoint
        {
            public Vertex RelatedVertex;
            public List<Edge> RelatedEdges;
            public Vector3 Point;

            public VertexPoint(Vertex vertex, Dictionary<Face, FacePoint> facesToFacePoints)
            {
                RelatedVertex = vertex;

                Vector3 facePointsAverage = Vector3.zero;
                foreach (Face face in vertex.Faces)
                {
                    facePointsAverage += facesToFacePoints[face].Point;
                }
                facePointsAverage /= vertex.Faces.Count;
                    
                Vector3 edgeMidpointsAverage = Vector3.zero;
                foreach (Edge edge in vertex.Edges)
                {
                    edgeMidpointsAverage += edge.GetMidpoint();
                }
                edgeMidpointsAverage /= vertex.Edges.Count;
                    
                Vector3 vertexValue = (vertex.Edges.Count > 3) ? ((float)(vertex.Edges.Count - 3) / vertex.Edges.Count) * vertex.Value : Vector3.zero;

                Point = (1.0f / vertex.Edges.Count) * facePointsAverage + (2.0f / vertex.Edges.Count) *
                                                                        edgeMidpointsAverage
                                                                        + vertexValue;
                RelatedEdges = vertex.Edges;
            }
        }

        private struct EdgePoint
        {
            public Edge RelatedEdge;
            public List<Face> RelatedFaces;
            public Vector3 Point;

            public EdgePoint(Edge edge)
            {
                RelatedEdge = edge;
                Point = (edge.StartPoint.Value + edge.EndPoint.Value + edge.Faces[0].GetCentroid() + edge.Faces[1].GetCentroid()) / 4;
                RelatedFaces = new List<Face>();
                RelatedFaces.Add(edge.Faces[0]);
                RelatedFaces.Add(edge.Faces[1]);
            }
        }

        private struct FacePoint
        {
            public Face RelatedFace;
            public Vector3 Point;

            public FacePoint(Face face)
            {
                RelatedFace = face;
                Point = face.GetCentroid();
            }
        }
        
        
        public Geometry CatmullClarkSubdivision()
        {
            List<Face> faces = new List<Face>();
            
            List<FacePoint> facePoints = new List<FacePoint>();
            List<EdgePoint> edgePoints = new List<EdgePoint>();
            List<VertexPoint> vertexPoints = new List<VertexPoint>();

            Dictionary<Face, List<Vector3>> edgePointsFromFaces = new Dictionary<Face, List<Vector3>>();
            
            foreach (Face face in Faces)
            {
                facePoints.Add(new FacePoint(face));
                foreach (Edge edge in face.Edges)
                {
                    edgePoints.Add(new EdgePoint(edge));
                }
            }

            var facesToFacePoints = facePoints.ToDictionary(fp => fp.RelatedFace, fp => fp);
            
            foreach (FacePoint facePoint in facePoints)
            {
                
                foreach (Vertex vertex in facePoint.RelatedFace.Vertices)
                {
                    vertexPoints.Add(new VertexPoint(vertex, facesToFacePoints));
                }
            }

            foreach (FacePoint facePoint in facePoints)
            {
                Vector3 origin = facePoint.Point;
                Vector3 firstPoint;
                Vector3 secondPoint;
                Vector3 thirdPoint;
                List<EdgePoint> relatedEdgePoints = edgePoints.Where(p => p.RelatedFaces.Contains(facePoint.RelatedFace)).ToList();
                for (var i = 0; i < relatedEdgePoints.Count; i++)
                {
                    if (i == relatedEdgePoints.Count - 1)
                    {
                        firstPoint = relatedEdgePoints[i].Point;
                        thirdPoint = relatedEdgePoints[0].Point;
                    }
                    else
                    {
                        firstPoint = relatedEdgePoints[i].Point;
                        thirdPoint = relatedEdgePoints[i + 1].Point;
                    }

                    foreach (VertexPoint vertexPoint in vertexPoints)
                    {
                        // if (vertexPoint.RelatedEdges.)
                    }
                    // faces.Add(new Face(new List<Edge>
                    //     {
                    //         new Edge(new Vertex(origin), new Vertex(firstPoint)),
                    //         new Edge(new Vertex(firstPoint), new Vertex(secondPoint)),
                    //         new Edge(new Vertex(secondPoint), new Vertex(thirdPoint)),
                    //         new Edge(new Vertex(thirdPoint), new Vertex(origin))
                    //     }
                    // ));
                }
                
                
            }
            
            Geometry geometry = new Geometry(faces);
            
            return geometry;
        }
        
    }

}