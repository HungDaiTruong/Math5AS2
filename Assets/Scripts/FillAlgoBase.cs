using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class FillAlgoBase : MonoBehaviour
{
    [SerializeField] protected Drawing _drawer;
    [SerializeField] Button _button;
    [SerializeField, Range(-1f, 1f)] protected float _ticks;
    [SerializeField, Range(1, 1000000)] protected int _batches = 10000;
    [SerializeField, Range(1, 1000000)] protected int _displayBatches = 10000;
    protected Color32 border;
    [SerializeField]
    protected Color32 fill;
    public void FillRandom()
    {
        fill = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256));
        fill = Color.green;
        //Fill(Random.Range(0, xMax), Random.Range(0, yMax));
        border = Drawing.Pen_Colour;
        Fill(0, 0);
    }
    private void Awake()
    {
        _button.onClick.AddListener(FillRandom);
    }

    protected abstract void Fill(int xI, int yI);
    protected bool PixelBorderOrOut(int x, int y, out Color32 found)
    {
        return PixelColorOrOut(x, y, border, out found);
    }
    protected bool PixelColorOrOut(int x, int y, Color32 color, out Color32 found)
    {
        return !_drawer.TryGetCurColor(x, y, out found) || found.Equals(color);
    }
}