using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casteljau : MonoBehaviour
{
    public Transform[] controlPoints;
    public int curveResolution = 45;

    public PointHandler pointHandler;
    private LineRenderer BezierLineRenderer;
    private LineRenderer controlLineRenderer;

    public Shader lineShader;
    public float stepSize = 0.01f;
    public float stepSizeChangeAmount = 0.001f; 


    // Start is called before the first frame update
    void Start()
    {

        GameObject controlLineObject = new GameObject("ControlLines");
        controlLineRenderer = controlLineObject.AddComponent<LineRenderer>();
        controlLineRenderer.startWidth = 0.3f; 
        controlLineRenderer.endWidth = 0.3f;

        GameObject lineObject = new GameObject("BezierCurve");
        BezierLineRenderer = lineObject.AddComponent<LineRenderer>();
        Material lineMaterial = new Material(lineShader);

        BezierLineRenderer.material = lineMaterial;
        controlLineRenderer.material = lineMaterial;

        BezierLineRenderer.widthCurve = AnimationCurve.Linear(0.9f, 0.5f, 1, 0.5f);
        BezierLineRenderer.textureMode = LineTextureMode.Tile;
        BezierLineRenderer.numCapVertices = 10;
        BezierLineRenderer.numCornerVertices = 10;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            stepSize += stepSizeChangeAmount;
            DrawCurve();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            stepSize -= stepSizeChangeAmount;
            stepSize = Mathf.Max(stepSize, 0.001f);
            DrawCurve();
        }
    }

    private Vector2Int[] ConvertPointsToVector2Int(List<GameObject> pointObjects)
    {
        Vector2Int[] vectorPoints = new Vector2Int[pointObjects.Count];

        for (int i = 0; i < pointObjects.Count; i++)
        {
            Vector2Int position = new Vector2Int((int)pointObjects[i].transform.position.x, (int)pointObjects[i].transform.position.y);
            vectorPoints[i] = position;
        }

        return vectorPoints;
    }

    private void DrawControlLines(Vector2Int[] controlPoints)
    {
        controlLineRenderer.positionCount = controlPoints.Length;

        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlLineRenderer.SetPosition(i, new Vector3(controlPoints[i].x, controlPoints[i].y, 0));
        }
    }

    public void DrawBezierCurve(Vector2Int[] controlPoints, int curveResolution)
    {
        if (controlPoints.Length < 2)
        {
            Debug.LogError("At least two control points are required for drawing a Bezier curve.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / stepSize);

        BezierLineRenderer.positionCount = numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float t = i * stepSize;
            Vector2Int point = CalculateBezierPoint(t, controlPoints);
            BezierLineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0));

            BezierLineRenderer.startColor = pointHandler.currentColor;
            BezierLineRenderer.endColor = pointHandler.currentColor;

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

        Vector2Int result = new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].y));
        return result;
    }

    public void DrawCurve()
    {
        print(pointHandler.points);

        Vector2Int[] controlPoints = ConvertPointsToVector2Int(pointHandler.points);

        DrawControlLines(controlPoints);

        DrawBezierCurve(controlPoints, curveResolution); 
    }

    public void clearCurve()
    {
        LineRenderer[] lineRenderers = FindObjectsOfType<LineRenderer>();

        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = 0;
        }
    }
}
