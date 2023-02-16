using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Block : MonoBehaviour
{
    [SerializeField] Shader shader = null;
    [SerializeField] List<Vector2> corners = null;
    [SerializeField] Color blockColor;
    public List<Vector2> Corners
    {
        get { return corners; }
    }

    public Color BlockColor
    {
        set
        {
            blockColor = value;
            GetComponent<MeshRenderer>().material.color = blockColor;
        }
        get { return blockColor; }
    }

    [HideInInspector] public bool isFullyInGrid;

    PolygonCollider2D polygonCollider = null;

    bool isFalling = false;
    float fallSpeed;
    float fallStopY;

    // --- Variables that are used only for level generation --- ///
    #region 
    // TODO: Use another block class for these variables (use inheritance)
    public Vector2 placeOnGrid;

    HashSet<Block> neighbours = null;
    public HashSet<Block> Neighbours { get { return neighbours; } }
    public List<Vector2> CornersInWorldSpace
    {
        get
        {
            List<Vector2> cornersWorld = new List<Vector2>();
            Vector2 origin = transform.position;
            foreach (Vector2 unityCorner in polygonCollider.points)
            {
                cornersWorld.Add(origin + unityCorner);
            }
            return cornersWorld;
        }
    }
    #endregion

    void Start()
    {
        GetComponent<MeshRenderer>().material = new Material(shader);
        GenerateShapeAndCollider();
    }

    void FixedUpdate()
    {
        ContinueFalling();
    }

    public Bounds GetBounds()
    {
        return polygonCollider.bounds;
    }

    public void SetCorners(List<Vector2> corners)
    {
        this.corners = corners;
    }

    // --- Variables that are used only for level generation --- ///
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

    private bool HasNeighbour(Block block)
    {
        if (neighbours == null) { return false; }
        return neighbours.Contains(block);
    }

    public bool IsNeighbourTo(Block neighbour)
    {
        int numIntersects = 0;
        foreach (Vector2 corner in CornersInWorldSpace)
        {
            foreach (Vector2 nbCorner in neighbour.CornersInWorldSpace)
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
        List<Vector2> newCorners = new List<Vector2>();
        List<Vector2> primary = corners;
        List<Vector2> secondary = merged.Corners;
        int pInd = 0;
        int sInd = 0;

        Vector2 offset = merged.placeOnGrid - placeOnGrid;
        if (merged.placeOnGrid.IsMoreLeftBottomThan(placeOnGrid))
        {
            SwapVariables(ref primary, ref secondary);
            offset = -offset;
            placeOnGrid = merged.placeOnGrid;
            transform.position = merged.transform.position;
        }

        for (int i = 0; i < secondary.Count; i++)
        {
            secondary[i] += offset;
        }

        Vector2 startCorner = primary[0];
        Vector2 lastDirection = Vector2.up;
        Vector2 corner = primary[0];
        do
        {
            newCorners.Add(corner);

            if (secondary.Contains(corner))
            {
                sInd = secondary.IndexOf(corner);
                Vector2 pNewCorner = primary[(pInd + 1) % primary.Count];
                Vector2 sNewCorner = secondary[(sInd + 1) % secondary.Count];

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
        GenerateShapeAndCollider();
    }

    private void SwapVariables<T>(ref T primary, ref T secondary)
    {
        T temp = primary;
        primary = secondary;
        secondary = temp;
    }

    #endregion

    public bool IsOriginInGrid(Bounds gridBounds)
    {
        Vector3 checkPoint = transform.position;
        checkPoint.z = gridBounds.center.z;
        return gridBounds.Contains(checkPoint);
    }

    public bool IsFullyInGrid(Bounds gridBounds)
    {
        Bounds blockBounds = GetBounds();
        Vector3 min = blockBounds.min;
        Vector3 max = blockBounds.max;
        min.z = gridBounds.center.z;
        max.z = gridBounds.center.z;
        return gridBounds.Contains(min) && gridBounds.Contains(max);
    }

    public void GenerateSquareBlock(float size = 1f)
    {
        corners = new List<Vector2>();
        corners.Add(Vector2.zero);
        corners.Add(new Vector2(0, size));
        corners.Add(new Vector2(size, size));
        corners.Add(new Vector2(size, 0));
    }

    public void GenerateShapeAndCollider()
    {
        GridPuzzle grid = FindObjectOfType<GridPuzzle>();
        float stepSize = grid.GridStepSize;

        Vector3 originalPosition = transform.position;
        transform.position = Vector3.zero;

        List<Vector2> gridCorners = new List<Vector2>();
        foreach (Vector2 corner in corners)
        {
            gridCorners.Add(stepSize * corner);
        }

        if (polygonCollider != null)
        {
            Destroy(polygonCollider);
        }

        polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        polygonCollider.SetPath(0, gridCorners);
        GetComponent<MeshFilter>().mesh = polygonCollider.CreateMesh(false, false);
        GetComponent<MeshRenderer>().material.color = blockColor;

        transform.position = originalPosition;
    }

    public void StartFalling(float speed, float stopY)
    {
        isFalling = true;
        fallSpeed = speed;
        fallStopY = stopY;
    }

    public void StopFalling()
    {
        isFalling = false;
    }

    private void ContinueFalling()
    {
        if (!isFalling) { return; }
        Vector3 newPos = transform.position;
        newPos.y -= Time.deltaTime * fallSpeed;
        transform.position = newPos;

        if (newPos.y <= fallStopY) { StopFalling(); }
    }

}
