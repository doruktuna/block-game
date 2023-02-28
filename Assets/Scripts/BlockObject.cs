using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockObject : MonoBehaviour
{
    [SerializeField] Shader shader = null;

    [HideInInspector] public Block block = null;
    public List<Vector2Int> Corners
    {
        get { return block.Corners; }
    }
    public Color BlockColor
    {
        set
        {
            block.blockColor = value;
            GetComponent<MeshRenderer>().material.color = value;
        }
        get { return block.blockColor; }
    }

    [HideInInspector] public bool isFullyInGrid;

    PolygonCollider2D polygonCollider = null;

    bool isFalling = false;
    float fallSpeed;
    float fallStopY;

    public Vector3 CenterPosition
    {
        get
        {
            Vector2 center = Vector2.zero;
            foreach (Vector2 corner in polygonCollider.points)
            {
                center += corner;
            }
            center /= polygonCollider.points.Length;

            return transform.position + new Vector3(center.x, center.y, 0);
        }
    }

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

    public void SetCorners(List<Vector2Int> corners)
    {
        block.corners = corners;
    }

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

    public void GenerateShapeAndCollider()
    {
        if (block == null) { return; }

        GridPuzzle grid = FindObjectOfType<GridPuzzle>();
        float stepSize = grid.GridStepSize;

        Vector3 originalPosition = transform.position;
        transform.position = Vector3.zero;

        List<Vector2> worldCorners = new List<Vector2>();
        foreach (Vector2 corner in block.corners)
        {
            worldCorners.Add(stepSize * corner);
        }

        if (polygonCollider != null)
        {
            Destroy(polygonCollider);
        }

        polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        polygonCollider.SetPath(0, worldCorners);
        GetComponent<MeshFilter>().mesh = polygonCollider.CreateMesh(false, false);
        GetComponent<MeshRenderer>().material.color = block.blockColor;

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
