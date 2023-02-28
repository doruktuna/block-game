using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator
{
    int gridSize;
    float triangleBreakProbability;
    int seed = 0;

    List<Block> blocks;
    public List<Block> Blocks { get { return blocks; } }

    public PuzzleGenerator(int gridSize, float triangleBreakProbability, int seed = 0)
    {
        this.gridSize = gridSize;
        this.triangleBreakProbability = triangleBreakProbability;
        UpdateRandomSeed(seed);
    }

    private void UpdateRandomSeed(int seed)
    {
        if (seed == 0)
        {
            seed = Guid.NewGuid().GetHashCode();
        }
        seed = Math.Abs(seed) % 1000000;

        Random.InitState(seed);
        this.seed = seed;
    }

    public void GenerateLevel(int numberOfPieces)
    {
        blocks = new List<Block>();

        GenerateSquareBlocks();

        //     yield return new WaitForFixedUpdate();
        //     print("Assigning neighbours to blocks");
        //     AssignNeighboursOfBlocks();

        //     print("Randomly breaking some of the squares into triangles");
        //     BreakSquaresIntoTriangles();
        //     print("Total number of blocks: " + blocks.Count);

        //     // TODO: Carry block size calculation to block
        //     maxSizeBlock = blocks[0];
        //     maxBlockSize = CalculateBlockSize(blocks[0]);
        //     numPieces = Random.Range(minPieces, maxPieces);
        //     print("Randomly merging blocks to reduce the number of blocks to " + numPieces);

        //     while (blocks.Count > numPieces)
        //     {
        //         Block block = SelectRandomBlockExcludingMax();
        //         Block toBeMerged = SelectSmallestBlock(block.Neighbours);
        //         block.MergeWith(toBeMerged);
        //         if (demoMode)
        //         {
        //             textDemo.text = String.Format("Block at {0} is going to merge with block at {1}", block.placeOnGrid, toBeMerged.placeOnGrid);
        //             yield return null;
        //         }

        //         // TODO: Try to get rid of this, I use this because I use bounds of colliders
        //         yield return new WaitForFixedUpdate();

        //         UpdateMaxBlockIfBigger(block);

        //         block.CheckAndAddNewNeighbours(toBeMerged.Neighbours);
        //         EraseFromNeighboursList(toBeMerged);

        //         Vector2 toBeMergedPlace = toBeMerged.placeOnGrid;
        //         RemoveAndDestroy(toBeMerged);

        //         block.name = "Merged block: " + block.placeOnGrid;
        //     }
        //     print("Total number of blocks: " + blocks.Count);

        //     print("Assigning pastel colors to final blocks: ");
        //     AssignSelectedColorsToBlocks(Util.ColorsForBlocks);

        //     float genTime = Time.fixedTime - genStartTime;
        //     print(String.Format("Generated in {0:0.00} seconds", genTime));

        //     NotifyLevelFinishedChecker();
        // }
    }

    private void GenerateSquareBlocks()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Block newBlock = new Block(new Vector2Int(i, j));
                newBlock.blockColor = GetRandomColor();
                newBlock.GenerateSquareBlock();
                blocks.Add(newBlock);
            }
        }
    }

    private Color GetRandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        return new Color(r, g, b);
    }
}
