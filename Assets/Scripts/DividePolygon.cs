using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DividePolygon : DrawingRelatedAlgo
{
    [SerializeField, Range(-1, 10)] private int _depth = 1;
    public override void Fill(int x, int y, List<Vector2Int> polygon)
    {
        if (polygon == null)
            return;
        var c = Drawing.Pen_Colour;
        Drawing.Pen_Colour = _additionnalDrawingColor;
        Triangulate(polygon);
        Drawing.Pen_Colour = c;
    }


    public void Triangulate(List<Vector2Int> pol)
    {
        if (_depth < 0)
        {
            while (pol.Count > 3)
                pol = TriangulateOnce(pol);
        }
        else
        {
            for (int i = 0; i < _depth && pol.Count > 3; i++)
            {
                pol = TriangulateOnce(pol);
                Draw(pol, true);
            }
        }
    }

    private List<Vector2Int> TriangulateOnce(List<Vector2Int> pol)
    {
        var newPol = new List<Vector2Int>();
        for (int i = 0; i < pol.Count - 1; i += 2)
        {
            newPol.Add(pol[i]);
        }
        //Intermediary last point
        if (pol.Count % 2 != 0)
            newPol.Add(pol[^1]);
        else
            newPol.Add(newPol[0]);
        return newPol;
        //return pol.Select(v => v - new Vector2Int(50, 30)).ToList();
    }

    private void Draw(List<Vector2Int> pol, bool refresh = true)
    {
        for (int i = 0; i < pol.Count - 1; i++)
        {
            _drawer.DrawLineSimple(pol[i], pol[i + 1]);
        }
        if (refresh)
            _drawer.ApplyMarkedPixelChanges();
    }
}
