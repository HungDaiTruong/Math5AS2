using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FillSeed : FillAlgoBase
{
    [SerializeField] private bool _rec;

    protected override void FillA(int xI, int yI,List<Vector2Int> polygon)
    {
        //Debug.Log("in operate, x: " + xI + ", " + yI);
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
        if (PixelBorderOrOut(x,y,out Color32 currC) || currC.Equals(fill))
            return;
        _drawer.MarkPixelToChange(x, y, fill);
        //Debug.Log($"filled {x},{y} intialy was {currC}, compared with {fill} and {border}, {currC.Equals(fill)},{currC.Equals(border)}");
        Fill4Rec(x + 1, y);
        Fill4Rec(x, y + 1);
        Fill4Rec(x - 1, y);
        Fill4Rec(x, y - 1);
    }

    protected IEnumerator Fill4(int xI, int yI)
    {
        Stack<(int x, int y)> stack = new();
        stack.Push((xI, yI));
        while (stack.Count > 0)
        {
            //Debug.Log("Has blue : " + (pix.First(p => p.b > 0f && p.b < 255)));
            var (x, y) = stack.Pop();
            if (PixelBorderOrOut(x, y, out Color32 currC) || currC.Equals(fill))
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
