using UnityEngine;

public static class ExtensionFunctions
{
    public static bool IsMoreLeftBottomThan(this Vector2 original, Vector2 target)
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

    public static float ClockwiseAngle(this Vector2 original, Vector2 reference)
    {
        // We are looking for clockwise angle, so we can use signed angle in this way
        float angle = Vector2.SignedAngle(original, reference);

        if (angle < 0) { angle += 360; }
        return angle;
    }
}