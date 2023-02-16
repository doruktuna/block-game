using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinishedChecker : MonoBehaviour
{
    int numPieces;
    int piecesFullyInGrid = 0;
    GameObject blocksContainer;
    GridPuzzle grid;
    LevelGenerator levelGenerator;

    GameObject UIFinishedTexts;

    bool isFinished = false;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag(Util.Tags.grid).GetComponent<GridPuzzle>();
        blocksContainer = GameObject.FindGameObjectWithTag(Util.Tags.blocksContainer);
        UIFinishedTexts = GameObject.FindGameObjectWithTag(Util.Tags.finishedTexts);
        UIFinishedTexts.SetActive(false);
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    void Update()
    {
        if (UIFinishedTexts.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            levelGenerator.PresentPieces();
            UIFinishedTexts.SetActive(false);
        }
    }


    public void LevelResetted(int numPieces)
    {
        isFinished = false;
        piecesFullyInGrid = 0;
        this.numPieces = numPieces;
    }

    public void BlockPlaced(Block piece)
    {
        // We have to wait for a physics update for bounds to get updated
        StartCoroutine(CheckBlockPlaceAfterFixedUpdate(piece));
    }

    private IEnumerator CheckBlockPlaceAfterFixedUpdate(Block piece)
    {
        yield return new WaitForFixedUpdate();

        // TODO: Use the commented version after solving the bug
        // bool isPlacedFullyInGrid = piece.IsFullyInGrid(grid.GetBounds());
        bool isPlacedFullyInGrid = piece.IsFullyInGrid(grid.GetExtendedBounds());

        if (isPlacedFullyInGrid) { BlockPlacedInGrid(piece); }
        if (!isPlacedFullyInGrid) { BlockPlacedOutOfGrid(piece); }
        piece.isFullyInGrid = isPlacedFullyInGrid;
    }

    void BlockPlacedInGrid(Block piece)
    {
        if (!piece.isFullyInGrid)
        {
            piecesFullyInGrid++;
        }
        print("Block placed in grid, total: " + piecesFullyInGrid);

        if (piecesFullyInGrid == numPieces)
        {
            print("Checking whether the level is finished");
            bool isFinished = CheckLevelFinished();
            if (isFinished)
            {
                print("Congrats mate, you have done it");
                UIFinishedTexts.SetActive(true);
                levelGenerator.Generate();
            }
            else
            {
                print("Not finished, something is out of place");
            }
        }
    }

    void BlockPlacedOutOfGrid(Block piece)
    {
        if (piece.isFullyInGrid)
        {
            piecesFullyInGrid--;
        }
        print("Block placed out grid, total: " + piecesFullyInGrid);
    }

    bool CheckLevelFinished()
    {
        Grid gridGen = grid.GetComponent<Grid>();
        int numPoints = 2 * grid.GridBoardSize;
        float stepSize = grid.GridStepSize / 2;

        for (int i = 0; i < numPoints; i++)
        {
            float x = grid.transform.position.x + i * stepSize;
            for (int j = 0; j < numPoints; j++)
            {
                float y = grid.transform.position.y + j * stepSize;
                Vector2 gridPoint = new Vector2(x, y);

                // TODO: This approach seems more readable
                // Vector2 offset = stepSize * new Vector2(i, j);
                // Vector2 gridPoint2 = ((Vector2) grid.transform.position) + offset;

                Collider2D hit = Physics2D.OverlapCircle(gridPoint, stepSize / 4);
                if (hit == null || !hit.gameObject.CompareTag(Util.Tags.block))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
