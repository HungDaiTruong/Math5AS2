using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilleLineSeed : FillAlgoBase
{
    protected override void FillA(int xI, int yI, List<Vector2Int> l)
    {
        if (!Safe(ref xI, ref yI, l))
            return;
        StartCoroutine(FillByLines(xI, yI));
    }

    protected IEnumerator FillByLines(int xI, int yI)
    {

        Stack<(int x, int y)> stack = new();
        stack.Push((xI, yI));
        int lines = -1;
        //We just need to store one current color, we don't use leftColor or rightColor we can use same variable
        int xd;
        int xg;
        while (stack.Count > 0)
        {
            lines++;
            //Debug.Log("Has blue : " + (pix.First(p => p.b > 0f && p.b < 255)));
            var (x, y) = stack.Pop();
            for (xd = x; !PixelBorderOrOut(xd + 1, y, out _); xd++)
            {
            }
            for (xg = x; !PixelBorderOrOut(xg - 1, y, out _); xg--)
            {
            }
            for (int i = xg; i <= xd; i++)
            {
                _drawer.MarkPixelToChange(i, y, fill);
            }
            if (_ticks > 0f)
            {
                _drawer.ApplyMarkedPixelChanges();
                yield return new WaitForSeconds(_ticks);
            }
            else if (lines % _batches == 0)
            {
                if (lines % _displayBatches == 0)
                    _drawer.ApplyMarkedPixelChanges();
                yield return new WaitForEndOfFrame();
            }
            CheckLine(stack, xg, xd, y + 1);
            CheckLine(stack, xg, xd, y - 1);
        }
        _drawer.ApplyMarkedPixelChanges();
    }

    private void CheckLine(Stack<(int x, int y)> stack, int xg, int xd, int y)
    {
        //It means y is out of bounds anyway so we shortcut avoiding to check all x that would return "out"
        if (!_drawer.TryGetCurColor(0, y, out _))
            return;
        for (int x = xd; x >= xg;)
        {
            for (; (PixelBorderOrOut(x, y, out _) || PixelColorOrOut(x, y, fill, out _)) && x >= xg; x--)
            {
            }
            //We know that this pixel isn't fill or border already if we match this based on precedent condition
            if (x >= xg)
            {
                stack.Push((x, y));
            }
            x--;
            //We are not in a border, aither intermediary either in middle of shape, or we already reached our leftest pixel which is a border, then we'll break out of main fort
            for (; !PixelBorderOrOut(x, y, out _) && x >= xg; x--)
            {

            }
        }
    }


}
