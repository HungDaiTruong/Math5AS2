using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class CasteljauV2 : MonoBehaviour
{
    public Transform[] controlPoints;
    public int curveResolution = 45;

    public PointHandlerV2 pointHandler;
    private LineRenderer BezierLineRenderer;

    public Shader lineShader;
    public float stepSize = 0.01f;
    public float stepSizeChangeAmount = 0.001f;

    public bool decasteljau = false;

    List<GameObject> points = new List<GameObject>();
    GameObject curve;

    public void ActivateCasteljau()
    {
        decasteljau = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            stepSize += stepSizeChangeAmount;
            UpdateStep();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            stepSize -= stepSizeChangeAmount;
            stepSize = Mathf.Max(stepSize, 0.001f);
            UpdateStep();
        }
    }

    public void DrawBezierCurve(List<GameObject> controlPoints, GameObject parent)
    {
        points = controlPoints;
        if (controlPoints.Count < 2)
        {
            Debug.LogError("At least two control points are required for drawing a Bezier curve.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / stepSize);

        List<Vector3> curvePoints = new List<Vector3>();


        for (int i = 0; i < numPoints; i++)
        {
            float t = i * stepSize;
            Vector3 point = CalculateBezierPoint(t, controlPoints);
            curvePoints.Add(point);

        }

        GameObject bezierCurveObj = new GameObject("CasteljauBezierCurve");
        curve = bezierCurveObj;
        bezierCurveObj.transform.SetParent(parent.transform);
        bezierCurveObj.transform.SetSiblingIndex(0);
        BezierLineRenderer = bezierCurveObj.AddComponent<LineRenderer>();

        BezierLineRenderer.positionCount = curvePoints.Count;
        BezierLineRenderer.startColor = pointHandler.currentColor;
        BezierLineRenderer.endColor = pointHandler.currentColor;
        BezierLineRenderer.SetPositions(curvePoints.ToArray());

        Material lineMaterial = new Material(lineShader);

        BezierLineRenderer.material = lineMaterial;

        BezierLineRenderer.startWidth = 0.05f;
        BezierLineRenderer.endWidth = 0.05f;
        BezierLineRenderer.textureMode = LineTextureMode.Tile;
        BezierLineRenderer.numCapVertices = 10;
        BezierLineRenderer.numCornerVertices = 10;


        pointHandler.drawable.paintInPixels(curvePoints);
    }

    public void UpdateDecasteljau(List<GameObject> controlPoints, GameObject bezierCurveObj)
    {

        pointHandler.drawable.ClearCanvas();
        points = controlPoints;
        if (controlPoints.Count < 2)
        {
            Debug.LogError("At least two control points are required for drawing a Bezier curve.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / stepSize);

        List<Vector3> curvePoints = new List<Vector3>();


        for (int i = 0; i < numPoints; i++)
        {
            float t = i * stepSize;
            Vector3 point = CalculateBezierPoint(t, controlPoints);
            curvePoints.Add(point);

        }

        BezierLineRenderer = bezierCurveObj.GetComponent<LineRenderer>();
        curve = bezierCurveObj;
        BezierLineRenderer.positionCount = curvePoints.Count;
        BezierLineRenderer.startColor = pointHandler.currentColor;
        BezierLineRenderer.endColor = pointHandler.currentColor;
        BezierLineRenderer.SetPositions(curvePoints.ToArray());

        Material lineMaterial = new Material(lineShader);

        BezierLineRenderer.material = lineMaterial;

        BezierLineRenderer.startWidth = 0.05f;
        BezierLineRenderer.endWidth = 0.05f;
        BezierLineRenderer.textureMode = LineTextureMode.Tile;
        BezierLineRenderer.numCapVertices = 10;
        BezierLineRenderer.numCornerVertices = 10;

        BezierLineRenderer.sortingOrder = 0;


        pointHandler.drawable.paintInPixels(curvePoints);
    }

    public void UpdateStep()
    {
        if (curve != null)
        {

            pointHandler.drawable.ClearCanvas();
            if (points.Count < 2)
            {
                Debug.LogError("At least two control points are required for drawing a Bezier curve.");
                return;
            }

            int numPoints = Mathf.CeilToInt(1f / stepSize);

            List<Vector3> curvePoints = new List<Vector3>();


            for (int i = 0; i < numPoints; i++)
            {
                float t = i * stepSize;
                Vector3 point = CalculateBezierPoint(t, points);
                curvePoints.Add(point);

            }

            BezierLineRenderer = curve.GetComponent<LineRenderer>();

            BezierLineRenderer.positionCount = curvePoints.Count;
            BezierLineRenderer.startColor = pointHandler.currentColor;
            BezierLineRenderer.endColor = pointHandler.currentColor;
            BezierLineRenderer.SetPositions(curvePoints.ToArray());

            Material lineMaterial = new Material(lineShader);

            BezierLineRenderer.material = lineMaterial;

            BezierLineRenderer.startWidth = 0.05f;
            BezierLineRenderer.endWidth = 0.05f;
            BezierLineRenderer.textureMode = LineTextureMode.Tile;
            BezierLineRenderer.numCapVertices = 10;
            BezierLineRenderer.numCornerVertices = 10;

            BezierLineRenderer.sortingOrder = 0;

            pointHandler.drawable.paintInPixels(curvePoints);
        }
    }

    public List<Vector3> GetCurvePoints(List<GameObject> controlPoints)
    {
        int numPoints = Mathf.CeilToInt(1f / stepSize);
        List<Vector3> curvePoints = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            float t = i * stepSize;
            Vector3 point = CalculateBezierPoint(t, controlPoints);
            curvePoints.Add(point);
        }

        return curvePoints;
    }

    private Vector3 CalculateBezierPoint(float t, List<GameObject> controlPoints)
    {
        int numPoints = controlPoints.Count;
        int lastIndex = numPoints - 1;

        List<Vector3> controlPositions = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            controlPositions.Add(controlPoints[i].transform.position);
        }

        for (int j = 1; j < numPoints; j++)
        {
            for (int k = 0; k < numPoints - j; k++)
            {
                controlPositions[k] = controlPositions[k] * (1 - t) + controlPositions[k + 1] * t;
            }
        }
        return controlPositions[0];
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
