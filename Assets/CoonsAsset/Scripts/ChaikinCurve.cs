using System.Collections.Generic;
using UnityEngine;

public class ChaikinCurve : MonoBehaviour
{
    public int iterations = 3;
    public Shader lineShader;
    public PointHandlerV2 pointHandler;

    private GameObject curveObject;
    private LineRenderer lineRenderer;

    public bool chaikin = false;

    private List<GameObject> points = new List<GameObject>();

    public void ActivateChaikin()
    {
        chaikin = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            iterations++;
            UpdateCurve(points);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            iterations = Mathf.Max(1, iterations - 1);
            UpdateCurve(points);
        }
    }

    public void DrawChaikinCurve(List<GameObject> controlPoints, GameObject parent)
    {
        points = controlPoints;

        if (controlPoints.Count < 2)
        {
            Debug.LogError("At least two control points are required.");
            return;
        }

        List<Vector3> refinedPoints = GetChaikinCurvePoints(controlPoints, iterations);

        if (curveObject == null)
        {
            curveObject = new GameObject("ChaikinCurve");
            curveObject.transform.SetParent(parent.transform);
            curveObject.transform.SetSiblingIndex(0);
            lineRenderer = curveObject.AddComponent<LineRenderer>();

            Material mat = new Material(lineShader);
            lineRenderer.material = mat;

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.numCapVertices = 10;
            lineRenderer.numCornerVertices = 10;
            lineRenderer.textureMode = LineTextureMode.Tile;
        }

        lineRenderer.startColor = pointHandler.currentColor;
        lineRenderer.endColor = pointHandler.currentColor;
        lineRenderer.positionCount = refinedPoints.Count;
        lineRenderer.SetPositions(refinedPoints.ToArray());

        pointHandler.drawable.paintInPixels(refinedPoints);
    }

    public void UpdateCurve(List<GameObject> updatedPoints)
    {
        if (curveObject == null || updatedPoints.Count < 2) return;

        points = updatedPoints;

        pointHandler.drawable.ClearCanvas();

        List<Vector3> refinedPoints = GetChaikinCurvePoints(points, iterations);
        lineRenderer.positionCount = refinedPoints.Count;
        lineRenderer.SetPositions(refinedPoints.ToArray());

        pointHandler.drawable.paintInPixels(refinedPoints);
    }

    public List<Vector3> GetChaikinCurvePoints(List<GameObject> controlPoints, int numIterations)
    {
        List<Vector3> currentPoints = new List<Vector3>();
        foreach (var obj in controlPoints)
        {
            currentPoints.Add(obj.transform.position);
        }

        for (int it = 0; it < numIterations; it++)
        {
            List<Vector3> newPoints = new List<Vector3>();
            for (int i = 0; i < currentPoints.Count - 1; i++)
            {
                Vector3 p0 = currentPoints[i];
                Vector3 p1 = currentPoints[i + 1];

                Vector3 Q = Vector3.Lerp(p0, p1, 0.25f);
                Vector3 R = Vector3.Lerp(p0, p1, 0.75f);

                newPoints.Add(Q);
                newPoints.Add(R);
            }
            currentPoints = newPoints;
        }

        return currentPoints;
    }

    public void ClearCurve()
    {
        if (curveObject != null)
        {
            Destroy(curveObject);
            pointHandler.drawable.ClearCanvas();
        }
    }
}
