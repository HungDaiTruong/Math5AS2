using System.Collections;
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
    public override void Operate()
    {
        fill = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256));
        fill = Color.green;
        //Fill(Random.Range(0, xMax), Random.Range(0, yMax));
        border = _drawer.penColors.Select<Color, Color32>(c => c).Append(_additionnalDrawingColor).ToArray();
        Fill(_drawer.W/2, _drawer.H/2);
    }

    protected abstract void Fill(int xI, int yI);
    protected bool PixelBorderOrOut(int x, int y, out Color32 found)
    {
        return PixelColorOrOut(x, y, border, out found);
    }
}