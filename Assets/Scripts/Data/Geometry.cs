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
            List<Face> faces = new List<Face>
            {
                //Add actual geometry
            };
            Geometry cube = new Geometry(faces);

            return cube;
        }
    }

}