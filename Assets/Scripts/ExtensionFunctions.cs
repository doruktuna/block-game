using System.Collections.Generic;
using UnityEngine;

public static class ExtensionFunctions
{
    public static List<T> CopyList<T>(this List<T> original)
    {
        List<T> newList = new List<T>();
        foreach (T el in original)
        {
            newList.Add(el);
        }

        return newList;
    }

    public static bool IsMoreLeftBottomThan(this Vector2Int original, Vector2Int target)
    {
        if (original.x < target.x)
        {
            return true;
        }
        else if (original.x == target.x && original.y < target.y)
        {
            return true;
        }
        return false;
    }

    public static float ClockwiseAngle(this Vector2Int original, Vector2Int reference)
    {
        // We are looking for clockwise angle, so we can use signed angle in this way
        float angle = Vector2.SignedAngle(original, reference);

        if (angle < 0) { angle += 360; }
        return angle;
    }
}