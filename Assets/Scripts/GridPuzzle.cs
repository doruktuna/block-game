using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPuzzle : MonoBehaviour
{
    [SerializeField] float gridUnitySize = 4f;
    [SerializeField] float gridSnapExtensionSize = .8f;
    [SerializeField] int gridBoardSize = 4;
    [SerializeField] float gridDotScale = .2f;
    [SerializeField] GameObject gridDot = null;

    GameObject gridDotsParent = null;

    float oldBoardSize = 0;
    float oldGridDotScale = 0;

    public float GridStepSize { get { return gridUnitySize / gridBoardSize; } }
    public float GridUnitySize { get { return gridUnitySize; } }
    public int GridBoardSize { get { return gridBoardSize; } }

    Bounds gridBounds;
    Bounds gridExtendedBounds;

    void Awake()
    {
        ReadSettings();
    }

    void Start()
    {
        DeleteDots();
        CreateDots();
        UpdateBounds();
    }

    private void ReadSettings()
    {
        LevelSettings levelSettings = LevelSettings.getInstance();
        if (levelSettings)
        {
            gridBoardSize = levelSettings.gridSize;
        }
    }

    public Bounds GetBounds()
    {
        return gridBounds;
    }

    public Bounds GetExtendedBounds()
    {
        return gridExtendedBounds;
    }

    private void UpdateBounds()
    {
        Vector2 center = transform.position;
        center += (gridUnitySize / 2) * Vector2.one;
        Vector2 size = gridUnitySize * Vector2.one;
        Vector2 extensionSize = gridSnapExtensionSize * Vector2.one;
        gridBounds = new Bounds(center, size);
        gridExtendedBounds = new Bounds(center, size + extensionSize);
    }

    private void DeleteDots()
    {
        foreach (Transform child in transform)
        {
            if (child.name == Util.Names.gridDotsParent)
            {
                if (!Application.IsPlaying(gameObject))
                {
                    DestroyImmediate(child.gameObject);
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
        }

        gridDotsParent = new GameObject(Util.Names.gridDotsParent);
        gridDotsParent.transform.parent = transform;
        gridDotsParent.transform.localPosition = Vector2.zero;
    }

    void CreateDots()
    {
        if (gridDot == null) { return; }

        float stepSize = gridUnitySize / gridBoardSize;
        for (int i = 1; i < gridBoardSize; i++)
        {
            for (int j = 1; j < gridBoardSize; j++)
            {
                float x = i * stepSize;
                float y = j * stepSize;
                GameObject newDot = Instantiate(gridDot, gridDotsParent.transform);
                newDot.transform.localPosition = new Vector3(x, y, 1);
                newDot.transform.localScale = new Vector3(gridDotScale, gridDotScale, 1);
                newDot.name = String.Format("Dot ({0}, {1})", i, j);
            }
        }

        oldBoardSize = gridBoardSize;
        oldGridDotScale = gridDotScale;
    }
}
