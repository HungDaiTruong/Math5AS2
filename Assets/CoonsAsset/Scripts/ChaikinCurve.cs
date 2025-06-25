using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ChaikinCurve : MonoBehaviour
{
    public int iterations = 1;
    public Shader lineShader;
    public PointHandlerV2 pointHandler;

    public GameObject curveObject;
    private LineRenderer lineRenderer;

    public bool chaikin = false;

    public List<GameObject> points = new List<GameObject>();
    public List<Vector3> refinedPoints = new List<Vector3>();

    // NEW: Store all curves
    private Dictionary<GameObject, CurveData> curveRegistry = new Dictionary<GameObject, CurveData>();

    // CurveData will store the info we need for lookup
    private class CurveData
    {
        public GameObject curveObj;
        public List<GameObject> controlPoints;

        public CurveData(GameObject curveObj, List<GameObject> controlPoints)
        {
            this.curveObj = curveObj;
            this.controlPoints = controlPoints;
        }
    }

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
            RedrawAllCurves();
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            iterations = Mathf.Max(1, iterations - 1);
            RedrawAllCurves();
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

        refinedPoints = GetChaikinCurvePoints(controlPoints, iterations);

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

        // Register this curve
        GameObject polygonParent = controlPoints[0].transform.parent.gameObject;
        if (!curveRegistry.ContainsKey(polygonParent))
        {
            curveRegistry.Add(polygonParent, new CurveData(chaikinCurveObj, new List<GameObject>(controlPoints)));
        }
    }


    // Redraw the curve with updated control points or iteration level
    public void UpdateCurve(List<GameObject> updatedPoints, GameObject curveObj)
    {
        lineRenderer = curveObj.GetComponent<LineRenderer>();
        curveObject = curveObj;

        if (curveObject == null || updatedPoints.Count < 2) return;

        points = updatedPoints;

        pointHandler.drawable.ClearCanvas();

        refinedPoints = GetChaikinCurvePoints(points, iterations);
        lineRenderer.positionCount = refinedPoints.Count;
        lineRenderer.SetPositions(refinedPoints.ToArray());

        pointHandler.drawable.paintInPixels(refinedPoints);
    }

    private void RedrawAllCurves()
    {
        foreach (var entry in curveRegistry)
        {
            GameObject parent = entry.Key;
            CurveData data = entry.Value;
            List<GameObject> controlPoints = data.controlPoints;
            GameObject curveObj = data.curveObj;

            UpdateCurve(controlPoints, curveObj);
        }
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

    public List<GameObject> GetControlPointsForPolygon(GameObject polygonObj)
    {
        if (curveRegistry.TryGetValue(polygonObj, out CurveData data))
        {
            return data.controlPoints;
        }
        return null;
    }

    public bool TryGetCurveByPolygon(GameObject polygonObj, out List<Vector3> refinedCurve, out GameObject curveGO)
    {
        if (curveRegistry.TryGetValue(polygonObj, out CurveData data))
        {
            refinedCurve = GetChaikinCurvePoints(data.controlPoints, iterations);
            curveGO = data.curveObj;
            return true;
        }

        refinedCurve = null;
        curveGO = null;
        return false;
    }

    public void CreateCoonMesh(List<GameObject> curveParents)
    {
        if (curveParents == null || curveParents.Count != 4)
        {
            Debug.LogWarning("Exactly 4 Chaikin curves must be selected.");
            return;
        }

        // Retrieve refined curves
        List<List<Vector3>> boundaryCurves = new List<List<Vector3>>();
        foreach (GameObject parent in curveParents)
        {
            if (!TryGetCurveByPolygon(parent, out List<Vector3> refined, out _))
            {
                Debug.LogWarning("One of the curves is invalid or missing.");
                return;
            }
            boundaryCurves.Add(refined);
        }

        // Assign boundaries
        List<Vector3> C0 = boundaryCurves[0]; // bottom (s: 0-1)
        List<Vector3> C1 = boundaryCurves[1]; // right  (t: 0-1)
        List<Vector3> C2 = boundaryCurves[2]; // top    (s: 0-1)
        List<Vector3> C3 = boundaryCurves[3]; // left   (t: 0-1)

        int resolutionU = Mathf.Min(C0.Count, C2.Count);
        int resolutionV = Mathf.Min(C1.Count, C3.Count);

        // Utility to resample a curve into N points
        List<Vector3> Resample(List<Vector3> input, int count)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float floatIndex = t * (input.Count - 1);
                int index = Mathf.FloorToInt(floatIndex);
                float frac = floatIndex - index;

                if (index >= input.Count - 1)
                {
                    result.Add(input[input.Count - 1]);
                }
                else
                {
                    result.Add(Vector3.Lerp(input[index], input[index + 1], frac));
                }
            }
            return result;
        }

        // Resample and enforce direction:
        List<Vector3> c0 = EnsureDirection(Resample(C0, resolutionU), desiredStart: C3[0]);
        List<Vector3> c2 = EnsureDirection(Resample(C2, resolutionU), desiredStart: C1[C1.Count - 1]);
        List<Vector3> c3 = EnsureDirection(Resample(C3, resolutionV), desiredStart: C0[0]);
        List<Vector3> c1 = EnsureDirection(Resample(C1, resolutionV), desiredStart: C0[C0.Count - 1]);

        Vector3[,] grid = new Vector3[resolutionU, resolutionV];

        for (int u = 0; u < resolutionU; u++)
        {
            float s = u / (float)(resolutionU - 1);
            for (int v = 0; v < resolutionV; v++)
            {
                float t = v / (float)(resolutionV - 1);

                // Interpolate edges
                Vector3 A = Vector3.Lerp(c3[v], c1[v], s); // vertical edge interpolation (left to right)
                Vector3 B = Vector3.Lerp(c0[u], c2[u], t); // horizontal edge interpolation (bottom to top)

                // Bilinear blend of corners
                Vector3 bilinear =
                    (1 - s) * (1 - t) * c0[0] +
                    s * (1 - t) * c0[resolutionU - 1] +
                    (1 - s) * t * c2[0] +
                    s * t * c2[resolutionU - 1];

                // Coons patch formula
                Vector3 S = A + B - bilinear;
                grid[u, v] = S;
            }
        }

        // Flatten vertices
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int u = 0; u < resolutionU; u++)
        {
            for (int v = 0; v < resolutionV; v++)
            {
                vertices.Add(grid[u, v]);
            }
        }

        // Build triangle indices
        for (int u = 0; u < resolutionU - 1; u++)
        {
            for (int v = 0; v < resolutionV - 1; v++)
            {
                int i00 = u * resolutionV + v;
                int i10 = (u + 1) * resolutionV + v;
                int i01 = u * resolutionV + (v + 1);
                int i11 = (u + 1) * resolutionV + (v + 1);

                triangles.Add(i00); triangles.Add(i10); triangles.Add(i11);
                triangles.Add(i00); triangles.Add(i11); triangles.Add(i01);
            }
        }

        // Create mesh
        GameObject meshGO = new GameObject("CoonSurface");
        MeshFilter mf = meshGO.AddComponent<MeshFilter>();
        MeshRenderer mr = meshGO.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        mf.mesh = mesh;

        Debug.Log("Coon surface successfully created.");
    }
    List<Vector3> EnsureDirection(List<Vector3> curve, Vector3 desiredStart)
    {
        if ((curve[0] - desiredStart).sqrMagnitude > (curve[curve.Count - 1] - desiredStart).sqrMagnitude)
        {
            curve.Reverse();
        }
        return curve;
    }

    private List<Vector3> Resample(List<Vector3> points, int targetCount)
    {
        List<Vector3> resampled = new List<Vector3>();

        float totalLength = 0f;
        for (int i = 0; i < points.Count - 1; i++)
            totalLength += Vector3.Distance(points[i], points[i + 1]);

        float segmentLength = totalLength / (targetCount - 1);
        resampled.Add(points[0]);

        int currentIndex = 0;
        float distanceAccum = 0f;

        for (int i = 1; i < targetCount - 1; i++)
        {
            float targetDist = i * segmentLength;

            while (currentIndex < points.Count - 1 && distanceAccum + Vector3.Distance(points[currentIndex], points[currentIndex + 1]) < targetDist)
            {
                distanceAccum += Vector3.Distance(points[currentIndex], points[currentIndex + 1]);
                currentIndex++;
            }

            float remaining = targetDist - distanceAccum;
            Vector3 dir = (points[currentIndex + 1] - points[currentIndex]).normalized;
            Vector3 newPoint = points[currentIndex] + dir * remaining;
            resampled.Add(newPoint);
        }

        resampled.Add(points[points.Count - 1]);
        return resampled;
    }


    // Clear the curve and canvas
    public void ClearCurve()
    {
        LineRenderer[] lineRenderers = FindObjectsOfType<LineRenderer>();

        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = 0;
        }
    }
}
