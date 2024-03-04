using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class FillAlgoBase : DrawingRelatedAlgo
{
    [SerializeField, Range(-1f, 1f)] protected float _ticks;
    [SerializeField, Range(1, 1000000)] protected int _batches = 10000;
    [SerializeField, Range(1, 1000000)] protected int _displayBatches = 10000;
    protected Color32[] border;
    [SerializeField]
    protected Color32 fill;
    public override void Fill(int x, int y, List<Vector2Int> l)
    {
        fill = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256));
        //fill = Color.green;
        //Fill(Random.Range(0, _drawer.W), Random.Range(0, _drawer.H));
        border = _drawer.penColors.Select<Color, Color32>(c => c).Append(_additionnalDrawingColor).ToArray();
        //Fill(_drawer.W/2, _drawer.H/2);
        Debug.Log("in operate, x: " + _drawer.x + ", " + _drawer.y);
        FillA(x, y, l);
        //Fill(200, 500);
    }

    protected abstract void FillA(int xI, int yI, List<Vector2Int> polygonToFill);
    protected bool PixelBorderOrOut(int x, int y, out Color32 found)
    {
        return PixelColorOrOut(x, y, border, out found);
    }
    protected bool Safe(ref int x, ref int y, List<Vector2Int> polygon) {
        if (x < 0 || y < 0)
        {
            if (polygon == null)
                return false;
            var pointInside = Drawing.PointInsidePoly(polygon);
            if (!pointInside.HasValue)
                return false;
            x = pointInside.Value.x;
            y = pointInside.Value.y;
        }
        return true;
    }
}