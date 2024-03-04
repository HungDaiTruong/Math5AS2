using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class DrawingRelatedAlgo : MonoBehaviour
{
    [SerializeField] protected Drawing _drawer;
    [SerializeField] Button _button;
    protected static Color _additionnalDrawingColor = new Color32(113, 113, 113, 255);
    protected bool PixelColorOrOut(int x, int y, Color32[] color, out Color32 found)
    {
        if (!_drawer.TryGetCurColor(x, y, out found))
            return true;
        //We need this cause out cannot be used in lambda
        var f = found;
        return color.Any(c => c.Equals(f));
    }
    protected List<Vector2Int> InsidePolygon(int x, int y)
    {
        return _drawer.polygons.FirstOrDefault((l) => IsInsidePolygon(l, x, y));
    }
    private bool IsInsidePolygon(List<Vector2Int> polygone, int xI, int yI)
    {
        Vector2Int point = new Vector2Int(xI, yI);
        float angleSum = 0;
        int n = polygon.Count;

        for (int i = 0; i < n; i++)
            {
                Vector2Int v1 = polygon[i] - point;
                Vector2Int v2 = polygon[(i + 1) % n] - point;
                
                float dot = Vector2Int.Dot(v1, v2);
                float magV1 = v1.Magnitude();
                float magV2 = v2.Magnitude();
                float cosTheta = dot / (magV1 * magV2);
                
                // Avoid division by zero in case of coincident points
                if(magV1 == 0 || magV2 == 0)
                    return false;
                
                float angle = (float)Math.Acos(cosTheta);
                angleSum += angle;
            }

            // Convert radians to degrees and check if sum is approximately 360
            float angleSumDegrees = angleSum * (180f / (float)Math.PI);
            return Math.Abs(angleSumDegrees - 360) < 0.01;

        throw new System.NotImplementedException();
    }

    // Method to calculate the dot product of two vectors
    public static float Dot(Vector2Int v1, Vector2Int v2)
    {
        return v1.x * v2.x + v1.y * v2.y;
    }
    
    protected bool PixelColorOrOut(int x, int y, Color32 color, out Color32 found)
    {
        return PixelColorOrOut(x, y, new Color32[] { color }, out found);
    }
    private void Awake()
    {
        //_button.onClick.AddListener(Operate);
        _button.onClick.AddListener(() => _drawer.fillAlgoInstance = this);
        _button.onClick.AddListener(_drawer.fill);
    }

    public abstract void Operate();
}