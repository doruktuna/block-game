using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    BlockObject selectedBlock = null;
    GridPuzzle grid = null;
    Vector3 offset;

    LevelFinishedChecker levelFinishedChecker;

    float minZForABlock = 0;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag(Util.Tags.grid).GetComponent<GridPuzzle>();
        levelFinishedChecker = FindObjectOfType<LevelFinishedChecker>();
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] targets = Physics2D.OverlapPointAll(mousePosition);
            foreach (var target in targets)
            {
                if (target.CompareTag(Util.Tags.block))
                {
                    selectedBlock = target.transform.GetComponent<BlockObject>();
                    offset = selectedBlock.transform.position - mousePosition;
                    minZForABlock -= 0.0001f;
                    UpdateBlockZ(selectedBlock, minZForABlock);
                    break;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedBlock)
        {
            PlaceSelectedInGrid();
            levelFinishedChecker.BlockPlaced(selectedBlock);
            selectedBlock = null;
        }

        if (selectedBlock)
        {
            selectedBlock.transform.position = mousePosition + offset;
            UpdateBlockZ(selectedBlock, minZForABlock);
        }
    }

    private void PlaceSelectedInGrid()
    {
        if (!selectedBlock.IsOriginInGrid(grid.GetExtendedBounds())) { return; }

        Vector3 piecePosition = selectedBlock.transform.position;
        Vector3 gridPosition = grid.transform.position;
        Vector3 pieceRelToGrid = piecePosition - gridPosition;
        float stepSize = grid.GridStepSize;
        float wholeSize = grid.GridUnitySize;

        pieceRelToGrid.x = RoundToGridPosition(pieceRelToGrid.x, wholeSize, stepSize);
        pieceRelToGrid.y = RoundToGridPosition(pieceRelToGrid.y, wholeSize, stepSize);

        selectedBlock.transform.position = gridPosition + pieceRelToGrid;
    }

    private float RoundToGridPosition(float val, float wholeSize, float stepSize)
    {
        if (val <= 0) { return 0f; }
        if (val >= wholeSize) { return wholeSize; }

        float diff = val % stepSize;
        val -= diff;
        if (diff > (stepSize / 2))
        {
            val += stepSize;
        }
        return val;
    }

    private void UpdateBlockZ(BlockObject selectedBlock, float minZForABlock)
    {
        Vector3 newPos = selectedBlock.transform.position;
        newPos.z = minZForABlock;
        selectedBlock.transform.position = newPos;
    }

}
