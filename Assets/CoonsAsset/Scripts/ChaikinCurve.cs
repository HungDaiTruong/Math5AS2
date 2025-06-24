using System.Collections.Generic;
using UnityEngine;

public class ChaikinCurve : MonoBehaviour
{
    public int iterations = 3; // Number of times to refine the curve
    public Shader lineShader;
    public PointHandlerV2 pointHandler;

    private GameObject curveObject;
    private LineRenderer lineRenderer;

    public bool chaikin = false;

    private List<GameObject> points = new List<GameObject>();

    // Activate the Chaikin curve drawing mode
    public void ActivateChaikin()
    {
        chaikin = true;
    }

    // Handle keyboard input to increase or decrease iteration count
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            iterations++;
            UpdateCurve(points);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            iterations = Mathf.Max(1, iterations - 1);
            UpdateCurve(points);
        }
    }

    // Draw the Chaikin curve based on input control points
    public void DrawChaikinCurve(List<GameObject> controlPoints, GameObject parent)
    {
        points = controlPoints;

        if (controlPoints.Count < 2)
        {
            Debug.LogError("At least two control points are required.");
            return;
        }

        List<Vector3> refinedPoints = GetChaikinCurvePoints(controlPoints, iterations);

        // Create the curve object
        GameObject chaikinCurveObj = new GameObject("ChaikinCurve");
        curveObject = chaikinCurveObj;
        chaikinCurveObj.transform.SetParent(parent.transform);
        chaikinCurveObj.transform.SetSiblingIndex(0);
        lineRenderer = chaikinCurveObj.AddComponent<LineRenderer>();

        Material mat = new Material(lineShader);
        lineRenderer.material = mat;

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.numCapVertices = 10;
        lineRenderer.numCornerVertices = 10;
        lineRenderer.textureMode = LineTextureMode.Tile;

        lineRenderer.startColor = pointHandler.currentColor;
        lineRenderer.endColor = pointHandler.currentColor;
        lineRenderer.positionCount = refinedPoints.Count;
        lineRenderer.SetPositions(refinedPoints.ToArray());

        pointHandler.drawable.paintInPixels(refinedPoints);
    }

    // Redraw the curve with updated control points or iteration level
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

    // Core algorithm: performs Chaikin's Corner Cutting
    public List<Vector3> GetChaikinCurvePoints(List<GameObject> controlPoints, int numIterations)
    {
        // Convert GameObjects to their positions
        List<Vector3> currentPoints = new List<Vector3>();
        foreach (var obj in controlPoints)
        {
            currentPoints.Add(obj.transform.position);
        }

        // Perform iterative refinement
        for (int it = 0; it < numIterations; it++)
        {
            List<Vector3> newPoints = new List<Vector3>();

            // Apply Chaikin's algorithm between each pair of points
            for (int i = 0; i < currentPoints.Count - 1; i++)
            {
                Vector3 p0 = currentPoints[i];
                Vector3 p1 = currentPoints[i + 1];

                // Q is 25% from p0 toward p1
                Vector3 Q = Vector3.Lerp(p0, p1, 0.25f);

                // R is 75% from p0 toward p1 (or 25% from p1 toward p0)
                Vector3 R = Vector3.Lerp(p0, p1, 0.75f);

                // These two new points replace the original edge
                newPoints.Add(Q);
                newPoints.Add(R);
            }

            // Replace current points with new ones for next iteration
            currentPoints = newPoints;
        }

        return currentPoints;
    }

    // Clear the curve and canvas
    public void ClearCurve()
    {
        if (curveObject != null)
        {
            Destroy(curveObject);
            pointHandler.drawable.ClearCanvas();
        }
    }
}
