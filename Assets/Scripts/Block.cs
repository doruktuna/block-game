using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Vector2Int placeOnGrid;
    public Color blockColor;

    public List<Vector2Int> corners = null;
    public List<Vector2Int> Corners { get { return corners; } }
    public List<Vector2Int> CornersInGridSpace
    {
        get
        {
            List<Vector2Int> cornersGrid = new List<Vector2Int>();
            foreach (Vector2Int corner in corners)
            {
                cornersGrid.Add(placeOnGrid + corner);
            }
            return cornersGrid;
        }
    }

    HashSet<Block> neighbours = null;
    public HashSet<Block> Neighbours { get { return neighbours; } }

    public Block(Vector2Int placeOnGrid)
    {
        this.placeOnGrid = placeOnGrid;
    }


    // --- Methods that are only used for level generation --- ///
    #region 
    public void AddNeighbour(Block candidate)
    {
        if (candidate == this) { return; }
        if (neighbours == null)
        {
            neighbours = new HashSet<Block>();
        }

        neighbours.Add(candidate);
        if (!candidate.HasNeighbour(this))
        {
            candidate.AddNeighbour(this);
        }
    }

    public int EncapsulatingRectangleSize()
    {
        Vector2Int bottomLeft = new Vector2Int(corners[0].x, corners[0].y);
        Vector2Int topRight = new Vector2Int(corners[0].x, corners[0].y);

        foreach (Vector2Int corner in corners)
        {
            if (corner.x < bottomLeft.x) { bottomLeft.x = corner.x; }
            if (corner.y < bottomLeft.y) { bottomLeft.y = corner.y; }
            if (corner.x > topRight.x) { topRight.x = corner.x; }
            if (corner.y > topRight.y) { topRight.y = corner.y; }
        }

        Vector2Int diff = topRight - bottomLeft;
        return diff.x * diff.y;
    }

    public void GenerateSquareBlock(int size = 1)
    {
        corners = new List<Vector2Int>();
        corners.Add(Vector2Int.zero);
        corners.Add(new Vector2Int(0, size));
        corners.Add(new Vector2Int(size, size));
        corners.Add(new Vector2Int(size, 0));
    }

    private bool HasNeighbour(Block block)
    {
        if (neighbours == null) { return false; }
        return neighbours.Contains(block);
    }

    public bool IsNeighbourTo(Block neighbour)
    {
        int numIntersects = 0;
        foreach (Vector2Int corner in CornersInGridSpace)
        {
            foreach (Vector2Int nbCorner in neighbour.CornersInGridSpace)
            {
                if (corner == nbCorner)
                {
                    numIntersects++;
                }
                if (numIntersects >= 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckAndAddNewNeighbours(HashSet<Block> candidates)
    {
        foreach (Block candidate in candidates)
        {
            if (IsNeighbourTo(candidate))
            {
                AddNeighbour(candidate);
                candidate.AddNeighbour(this);
            }
        }
    }

    public void RemoveNeighbour(Block block)
    {
        neighbours?.Remove(block);
    }

    public void MergeWith(Block merged)
    {
        List<Vector2Int> newCorners = new List<Vector2Int>();
        List<Vector2Int> primary = corners;
        List<Vector2Int> secondary = merged.Corners;
        int pInd = 0;
        int sInd = 0;

        Vector2Int offset = merged.placeOnGrid - placeOnGrid;
        if (merged.placeOnGrid.IsMoreLeftBottomThan(placeOnGrid))
        {
            SwapVariables(ref primary, ref secondary);
            offset = -offset;
            placeOnGrid = merged.placeOnGrid;
        }

        for (int i = 0; i < secondary.Count; i++)
        {
            secondary[i] += offset;
        }

        Vector2Int startCorner = primary[0];
        Vector2Int lastDirection = Vector2Int.up;
        Vector2Int corner = primary[0];
        do
        {
            newCorners.Add(corner);

            if (secondary.Contains(corner))
            {
                sInd = secondary.IndexOf(corner);
                Vector2Int pNewCorner = primary[(pInd + 1) % primary.Count];
                Vector2Int sNewCorner = secondary[(sInd + 1) % secondary.Count];

                float pAngle = (pNewCorner - corner).ClockwiseAngle(-lastDirection);
                float sAngle = (sNewCorner - corner).ClockwiseAngle(-lastDirection);

                // Prioritize moving less clockwise wrt to last step direction
                if (sAngle < pAngle)
                {
                    SwapVariables(ref primary, ref secondary);
                    SwapVariables(ref pInd, ref sInd);
                }
            }

            pInd = (pInd + 1) % primary.Count;
            lastDirection = primary[pInd] - corner;
            corner = primary[pInd];

        } while (corner != startCorner);

        corners = newCorners;
    }

    private void SwapVariables<T>(ref T primary, ref T secondary)
    {
        T temp = primary;
        primary = secondary;
        secondary = temp;
    }

    #endregion


}
