using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FillSeed : MonoBehaviour
{
    [SerializeField] Drawing _drawer;
    [SerializeField] Button _button;
    [SerializeField, Range(-1f, 1f)] private float _ticks;
    [SerializeField, Range(1, 1000000)] private int _batches = 10000;
    [SerializeField, Range(1, 1000000)] private int _displayBatches = 10000;
    [SerializeField]
    private Color32 fill;
    [SerializeField] private bool _rec;
    Color32 border;
    private void Awake()
    {
        _button.onClick.AddListener(FillRandom);
    }
    public void FillRandom()
    {
        fill = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256));
        fill = Color.green;
        //Fill(Random.Range(0, xMax), Random.Range(0, yMax));
        Fill(0, 0);
    }
    void Fill(int xI, int yI)
    {
        border = Drawing.Pen_Colour;
        if (_rec)
        {
            Fill4Rec(xI, yI);
            _drawer.ApplyMarkedPixelChanges();
        }
        else
            StartCoroutine(Fill4(xI, yI));

    }

    void Fill4Rec(int x, int y)
    {
        var currC = _drawer.GetCurColor(x, y);
        if (!currC.HasValue || currC.Equals(fill) || currC.Equals(border))
            return;
        _drawer.MarkPixelToChange(x, y, fill);
        //Debug.Log($"filled {x},{y} intialy was {currC}, compared with {fill} and {border}, {currC.Equals(fill)},{currC.Equals(border)}");
        Fill4Rec(x + 1, y);
        Fill4Rec(x, y + 1);
        Fill4Rec(x - 1, y);
        Fill4Rec(x, y - 1);
    }
    IEnumerator Fill4(int xI, int yI)
    {
        Stack<(int x, int y)> stack = new();
        stack.Push((xI, yI));
        while (stack.Count > 0)
        {
            //Debug.Log("Has blue : " + (pix.First(p => p.b > 0f && p.b < 255)));
            var (x, y) = stack.Pop();
            var currC = _drawer.GetCurColor(x, y);
            if (!currC.HasValue || currC.Equals(fill) || currC.Equals(border))
                continue;
            _drawer.MarkPixelToChange(x, y, fill);

            //Debug.Log($"filled {x},{y} intialy was {currC}, compared with {fill} and {border}, {currC.Equals(fill)},{currC.Equals(border)}");
            stack.Push((x + 1, y));
            stack.Push((x, y + 1));
            stack.Push((x - 1, y));
            stack.Push((x, y - 1));
            const int maxAddPerVisit = 4;
            if (_ticks > 0f)
            {
                _drawer.ApplyMarkedPixelChanges();
                yield return new WaitForSeconds(_ticks);
            }
            else if (stack.Count % _batches < maxAddPerVisit)
            {
                if (stack.Count % _displayBatches < maxAddPerVisit)
                    _drawer.ApplyMarkedPixelChanges();
                yield return new WaitForEndOfFrame();
            }
        }
        _drawer.ApplyMarkedPixelChanges();
    }
}
