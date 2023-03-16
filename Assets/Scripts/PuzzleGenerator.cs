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

    Block maxSizeBlock;
    float maxBlockSize;

    IEnumerator generatorEnumerator;
    bool isGenerationFinished = false;
    public bool IsGenerationFinished { get { return isGenerationFinished; } }

    public struct DemoInfo
    {
        public string message;
        public bool isMergingStage;
        public int merge1Ind;
        public int merge2Ind;
    }
    public DemoInfo demoInfo;

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

    public void GenerateLevel(int numPieces, bool demoMode = false)
    {
        generatorEnumerator = GenerateLevelEnumerator(numPieces);
        isGenerationFinished = false;

        if (!demoMode)
        {
            while (!isGenerationFinished)
            {
                ProceedToNextStep();
            }
        }
    }

    public void ProceedToNextStep()
    {
        generatorEnumerator.MoveNext();
    }

    private IEnumerator GenerateLevelEnumerator(int numPieces)
    {
        blocks = new List<Block>();

        demoInfo.isMergingStage = false;
        demoInfo.message = "Generating square blocks";
        yield return null;
        GenerateSquareBlocks();
        demoInfo.message = blocks.Count + " squares generated. Neighbour lists will be updated.";
        yield return null;

        AssignNeighboursOfBlocks();

        demoInfo.message = "Randomly breaking some of the squares into triangles. Break probability: " + triangleBreakProbability;
        yield return null;
        BreakSquaresIntoTriangles();

        maxSizeBlock = blocks[0];
        maxBlockSize = blocks[0].EncapsulatingRectangleSize();

        demoInfo.message = "Randomly merging blocks to reduce the number of blocks to " + numPieces;
        yield return null;
        demoInfo.isMergingStage = true;
        while (blocks.Count > numPieces)
        {
            Block block = SelectRandomBlockExcludingMax();
            Block toBeMerged = SelectSmallestBlock(block.Neighbours);

            demoInfo.merge1Ind = blocks.IndexOf(block);
            demoInfo.merge2Ind = blocks.IndexOf(toBeMerged);
            demoInfo.message = String.Format("Block at {0} is going to merge with block at {1}", block.placeOnGrid, toBeMerged.placeOnGrid);
            yield return null;

            block.MergeWith(toBeMerged);

            UpdateMaxBlockIfBigger(block);
            block.CheckAndAddNewNeighbours(toBeMerged.Neighbours);

            EraseFromNeighboursList(toBeMerged);
            blocks.Remove(toBeMerged);
        }
        demoInfo.isMergingStage = false;
        demoInfo.message = String.Format("Assigning pre defined pastel colors to blocks");
        yield return null;

        AssignSelectedColorsToBlocks(Util.ColorsForBlocks);

        demoInfo.message = "Level generation completed.";
        isGenerationFinished = true;
        yield return null;
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

    private void AssignNeighboursOfBlocks()
    {
        // Not efficient for checking neighbours, but not a big deal
        foreach (Block block in blocks)
        {
            foreach (Block neighbour in blocks)
            {
                if (block == neighbour) { continue; }
                if (block.IsNeighbourTo(neighbour))
                {
                    block.AddNeighbour(neighbour);
                }
            }
        }
    }

    private void BreakSquaresIntoTriangles()
    {
        List<Block> squaresToDestroy = new List<Block>();
        List<Block> newTriangles = new List<Block>();

        foreach (Block block in blocks)
        {
            if (block.Corners.Count != 4) { continue; }
            if (Random.Range(0f, 1f) > triangleBreakProbability) { continue; }

            Vector2Int placeOnGrid1 = block.placeOnGrid;
            Vector2Int placeOnGrid2;
            List<Vector2Int> corners1 = block.Corners.CopyList();
            List<Vector2Int> corners2 = block.Corners.CopyList();

            // Top-left to bottom right diagonal
            if (Random.Range(0f, 1f) < 0.5f)
            {
                corners1.RemoveAt(2);
                corners2.RemoveAt(0);
                corners2 = ResetOriginForCorners(corners2);
                placeOnGrid2 = block.placeOnGrid + block.Corners[1];
            }
            // Top-right to bottom left diagonal
            else
            {
                corners1.RemoveAt(3);
                corners2.RemoveAt(1);
                placeOnGrid2 = block.placeOnGrid;
            }
            Block triangle1 = new Block(placeOnGrid1);
            Block triangle2 = new Block(placeOnGrid2);

            triangle1.corners = corners1;
            triangle2.corners = corners2;

            triangle1.blockColor = GetRandomColor();
            triangle2.blockColor = GetRandomColor();

            triangle1.AddNeighbour(triangle2);
            triangle2.AddNeighbour(triangle1);
            triangle1.CheckAndAddNewNeighbours(block.Neighbours);
            triangle2.CheckAndAddNewNeighbours(block.Neighbours);

            newTriangles.Add(triangle1);
            newTriangles.Add(triangle2);
            squaresToDestroy.Add(block);
        }

        foreach (Block block in newTriangles)
        {
            blocks.Add(block);
        }

        foreach (Block block in squaresToDestroy)
        {
            EraseFromNeighboursList(block);
            blocks.Remove(block);
        }
    }

    private void UpdateMaxBlockIfBigger(Block block)
    {
        float blockSize = block.EncapsulatingRectangleSize();
        if (blockSize > maxBlockSize)
        {
            maxBlockSize = blockSize;
            maxSizeBlock = block;
        }
    }

    private Block SelectRandomBlockExcludingMax()
    {
        Block block = null;
        do
        {
            int index = Random.Range(0, blocks.Count);
            block = blocks[index];
        } while (block == maxSizeBlock);

        return block;
    }

    private Block SelectSmallestBlock(HashSet<Block> candidates)
    {
        float minSize = float.MaxValue;
        Block minAreaBlock = null;

        foreach (Block block in candidates)
        {
            float blockSize = block.EncapsulatingRectangleSize();
            if (blockSize < minSize)
            {
                minSize = blockSize;
                minAreaBlock = block;
            }
        }

        return minAreaBlock;
    }

    private List<Vector2Int> ResetOriginForCorners(List<Vector2Int> corners)
    {
        List<Vector2Int> newCorners = new List<Vector2Int>();

        Vector2Int leftBottom = corners[0];
        foreach (Vector2Int corner in corners)
        {
            if (corner.IsMoreLeftBottomThan(leftBottom))
            {
                leftBottom = corner;
            }
        }

        foreach (Vector2Int corner in corners)
        {
            newCorners.Add(corner - leftBottom);
        }
        return newCorners;
    }

    private void EraseFromNeighboursList(Block removal)
    {
        foreach (Block block in blocks)
        {
            block.RemoveNeighbour(removal);
        }
    }

    private void AssignSelectedColorsToBlocks(List<Color> colors)
    {
        int ind = Random.Range(0, colors.Count - 1);
        foreach (Block block in blocks)
        {
            block.blockColor = colors[ind];
            ind = (ind + 1) % colors.Count;
        }
    }

    private static Color GetRandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        return new Color(r, g, b);
    }
}
