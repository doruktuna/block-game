using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector2Int
{
    public int x, y;

    public SerializableVector2Int(Vector2Int vector)
    {
        x = vector.x;
        y = vector.y;
    }

    public Vector2Int ToVector()
    {
        return new Vector2Int(x, y);
    }
}
