using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pascal : MonoBehaviour
{
    public PointHandler controlPoints;

    public LineRenderer lineRenderer;

    public Shader lineShader;

    public bool pascal = false;

    List<GameObject> points = new List<GameObject>();
    GameObject curve;
    public Casteljau casteljauScript;

    public void ActivatePascal()
    {
        pascal = true;
    }
    public void DrawCurve(List<GameObject> controlPointsList, GameObject parent)
    {
        points = controlPointsList;
        if (controlPointsList.Count < 2)
        {
            Debug.LogError("Se necesitan al menos 2 puntos de control para una curva de Bezier.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / casteljauScript.stepSize);

        List<Vector3> curvePoints = new List<Vector3>();

        for (int j = 0; j <= numPoints; j++)
        {
            float t = j * casteljauScript.stepSize;

            Vector3 point = CalculateBezierPointUsingPascal(t, controlPointsList);
            curvePoints.Add(point);
        }

        GameObject bezierCurveObj = new GameObject("PascalBezierCurve");
        curve = bezierCurveObj;
        bezierCurveObj.transform.SetParent(parent.transform);

        lineRenderer = bezierCurveObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;
        lineRenderer.startColor = controlPoints.currentColor;
        lineRenderer.endColor = controlPoints.currentColor;
        lineRenderer.SetPositions(curvePoints.ToArray());


        Material lineMaterial = new Material(lineShader);

        lineRenderer.material = lineMaterial;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 10;
        lineRenderer.numCornerVertices = 10;
    }

    public void UpdateCurve(List<GameObject> controlPointsList, GameObject bezierCurveObj)
    {
        points = controlPointsList;
        if (controlPointsList.Count < 2)
        {
            Debug.LogError("Se necesitan al menos 2 puntos de control para una curva de Bezier.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / casteljauScript.stepSize);

        List<Vector3> curvePoints = new List<Vector3>();

        for (int j = 0; j <= numPoints; j++)
        {
            float t = j * casteljauScript.stepSize;

            Vector3 point = CalculateBezierPointUsingPascal(t, controlPointsList);
            curvePoints.Add(point);
        }

        curve = bezierCurveObj;
        LineRenderer lineRenderer = bezierCurveObj.GetComponent<LineRenderer>();

        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;
        lineRenderer.startColor = controlPoints.currentColor;
        lineRenderer.endColor = controlPoints.currentColor;
        lineRenderer.SetPositions(curvePoints.ToArray());


        Material lineMaterial = new Material(lineShader);

        lineRenderer.material = lineMaterial;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 10;
        lineRenderer.numCornerVertices = 10;

    }

    public void UpdateStep()
    {
        if (curve != null)
        {
            if (points.Count < 2)
            {
                Debug.LogError("Se necesitan al menos 2 puntos de control para una curva de Bezier.");
                return;
            }

            int numPoints = Mathf.CeilToInt(1f / casteljauScript.stepSize);

            List<Vector3> curvePoints = new List<Vector3>();

            for (int j = 0; j <= numPoints; j++)
            {
                float t = j * casteljauScript.stepSize;

                Vector3 point = CalculateBezierPointUsingPascal(t, points);
                curvePoints.Add(point);
            }


            LineRenderer lineRenderer = curve.GetComponent<LineRenderer>();
            lineRenderer.positionCount = curvePoints.Count;
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = controlPoints.currentColor;
            lineRenderer.endColor = controlPoints.currentColor;
            lineRenderer.SetPositions(curvePoints.ToArray());


            Material lineMaterial = new Material(lineShader);

            lineRenderer.material = lineMaterial;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.numCapVertices = 10;
            lineRenderer.numCornerVertices = 10;

        }
    }

    private Vector3 CalculateBezierPointUsingPascal(float t, List<GameObject> points)
    {
        int numPoints = points.Count;
        int lastIndex = numPoints - 1;

        List<Vector3> controlPositions = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            controlPositions.Add(points[i].transform.position);
        }

        List<int> coefficients = CalculateBinomialCoefficients(numPoints - 1);

        Vector3 result = Vector3.zero;

        for (int i = 0; i < numPoints; i++)
        {
            float term = coefficients[i] * Mathf.Pow(t, i) * Mathf.Pow(1 - t, numPoints - 1 - i);
            result += controlPositions[i] * term;
        }

        return result;
    }

    private List<int> CalculateBinomialCoefficients(int n)
    {
        List<int> coefficients = new List<int>();
        coefficients.Add(1);

        for (int i = 1; i <= n; i++)
        {
            int nextCoefficient = coefficients[i - 1] * (n - i + 1) / i;
            coefficients.Add(nextCoefficient);
        }

        return coefficients;
    }

}
