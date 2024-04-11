using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casteljau : MonoBehaviour
{
    public Transform[] controlPoints;
    public int curveResolution = 50;

    public PointHandler pointHandler;
    private LineRenderer lineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        // Create a new GameObject with LineRenderer component for drawing the curve
        GameObject lineObject = new GameObject("BezierCurve");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f; // Adjust line width as needed
        lineRenderer.endWidth = 0.1f;

    }

    // Method to extract positions from GameObjects and convert them to Vector2Int
    private Vector2Int[] ConvertPointsToVector2Int(List<GameObject> pointObjects)
    {
        Vector2Int[] vectorPoints = new Vector2Int[pointObjects.Count];

        for (int i = 0; i < pointObjects.Count; i++)
        {
            // Get the position of the GameObject and convert it to Vector2Int
            Vector2Int position = new Vector2Int((int)pointObjects[i].transform.position.x, (int)pointObjects[i].transform.position.y);
            vectorPoints[i] = position;
        }

        return vectorPoints;
    }

    public void DrawBezierCurve(Vector2Int[] controlPoints, int curveResolution)
    {
        if (controlPoints.Length < 2)
        {
            Debug.LogError("At least two control points are required for drawing a Bezier curve.");
            return;
        }

        // Set the number of vertices in the line renderer
        lineRenderer.positionCount = curveResolution + 1;

        // Calculate and set the points on the Bezier curve
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector2Int point = CalculateBezierPoint(t, controlPoints);
            lineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0)); // Set the position of the vertex
        }

        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector2Int point = CalculateBezierPoint(t, controlPoints);
            // Draw or mark the point on the canvas
            //MarkPixelToChange(point.x, point.y, Pen_Colour);
        }
    }

    private Vector2Int CalculateBezierPoint(float t, Vector2Int[] controlPoints)
    {
        Vector2[] points = new Vector2[controlPoints.Length];

        for (int i = 0; i < controlPoints.Length; i++)
        {
            points[i] = controlPoints[i];
        }

        for (int j = 1; j < controlPoints.Length; j++)
        {
            for (int k = 0; k < controlPoints.Length - j; k++)
            {
                points[k] = points[k] * (1 - t) + points[k + 1] * t;
            }
        }

        Vector2Int result = new Vector2Int((int)points[0].x, (int)points[0].y);
        return result;
    }

    public void DrawCurve()
    {
        print(pointHandler.points);
        // Convert GameObject points to Vector2Int
        Vector2Int[] controlPoints = ConvertPointsToVector2Int(pointHandler.points);

        // Draw the Bezier curve
        DrawBezierCurve(controlPoints, curveResolution); // Adjust curve resolution as needed
    }
}
