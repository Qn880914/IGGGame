#region Namespace

using UnityEngine;

#endregion

public static class RectExtern
{
    /// <summary>
    /// 如果r1完全包含r2,则返回true
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    public static bool Contains(this Rect r1, Rect r2)
    {
        if (r2.xMin >= r1.xMin && r2.xMax <= r1.xMax &&
            r2.yMin >= r1.yMin && r2.yMax <= r1.yMax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 把rect看成是一个包围盒,加入一个新的点,如果点在rect
    /// 范围外,则扩展rect的范围.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Rect Extern(this Rect rect, Vector2 pos)
    {
        if (!rect.Contains(pos))
        {
            Rect newRect = new Rect(rect);
            newRect.xMin = newRect.xMin > pos.x ? pos.x : newRect.xMin;
            newRect.xMax = newRect.xMax < pos.x ? pos.x : newRect.xMax;
            newRect.yMin = newRect.yMin > pos.y ? pos.y : newRect.yMin;
            newRect.yMax = newRect.yMax < pos.y ? pos.y : newRect.yMax;
            return newRect;
        }
        else
        {
            return rect;
        }
    }
}