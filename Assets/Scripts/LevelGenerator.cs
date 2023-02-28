using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int minPieces;
    [SerializeField] int maxPieces;
    [SerializeField] Block blockPrefab;
    [SerializeField][Range(0f, 1f)] float triangleBreakProbability;

    [SerializeField] float minFallSpeed = 5f;
    [SerializeField] float maxFallSpeed = 10f;

    [SerializeField][Range(.1f, .5f)] float stepTimeForDemo = .25f;

    [SerializeField] TMP_Text textDemo;
    [SerializeField] Button buttonPlayPauseButton;
    [SerializeField] Button buttonNextStep;

    [SerializeField] GameObject redDot;
    [SerializeField] GameObject redDot2;

    int gridSize;
    public int GridSize { get { return gridSize; } }

    int numPieces;

    int levelSeed;
    public int LevelSeed { get { return levelSeed; } }

    GridPuzzle grid;
    LevelFinishedChecker levelFinishedChecker;

    List<Block> blocks;
    public List<Block> Blocks { get { return blocks; } }

    List<Block> oldBlocks;

    Block maxSizeBlock;
    float maxBlockSize;

    float blockFallingMinY;
    float blockFallingMaxY;

    bool demoMode = false;
    IEnumerator demoEnumerator = null;
    bool demoFinished = false;
    bool demoPlayContinously = false;

    void Awake()
    {
        ReadSettings();
    }

    void Start()
    {
        AssignBlockFallingYRange();
        AssignGrid();

        levelFinishedChecker = FindObjectOfType<LevelFinishedChecker>();
        DeleteOldBlocks();

        demoMode = !grid.CompareTag(Util.Tags.gridForGeneration);
        StartLevelGeneration();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(GenerateAndPresentPieces());
        }

        if (Input.GetKeyDown(KeyCode.N) && demoEnumerator != null)
        {
            ProceedOneStepInDemo();
        }
    }

    private void ReadSettings()
    {
        LevelSettings levelSettings = LevelSettings.getInstance();
        if (levelSettings)
        {
            minPieces = levelSettings.minPieces;
            maxPieces = levelSettings.maxPieces;
            triangleBreakProbability = levelSettings.triangleBreakProbability;
        }
    }

    private void AssignBlockFallingYRange()
    {
        GameObject fallStopper = GameObject.FindGameObjectWithTag(Util.Tags.fallStopper);
        if (fallStopper != null)
        {
            Bounds fallBounds = fallStopper.GetComponent<BoxCollider2D>().bounds;
            blockFallingMinY = fallBounds.min.y;
            blockFallingMaxY = fallBounds.max.y;
        }
    }

    private void AssignGrid()
    {
        GameObject gridObject = GameObject.FindGameObjectWithTag(Util.Tags.gridForGeneration);
        if (gridObject != null)
        {
            grid = gridObject.GetComponent<GridPuzzle>();
        }
        else
        {
            grid = GameObject.FindGameObjectWithTag(Util.Tags.grid).GetComponent<GridPuzzle>();
        }
    }

    private void StartLevelGeneration()
    {
        if (demoMode)
        {
            demoEnumerator = GenerateLevelDemo();
            demoEnumerator.MoveNext();
            demoFinished = false;
        }
        else
        {
            StartCoroutine(GenerateAndPresentPieces());
        }
    }

    public void Generate(int seed = 0)
    {
        StartCoroutine(GenerateLevel(seed));
    }

    private IEnumerator GenerateAndPresentPieces(int seed = 0)
    {
        yield return StartCoroutine(GenerateLevel(seed));
        PresentPieces();
    }

    public void PresentPieces()
    {
        DeleteOldBlocks();
        if (grid.CompareTag(Util.Tags.gridForGeneration))
        {
            MakePiecesFall();
        }
    }

    private IEnumerator GenerateLevel(int seed = 0)
    {
        float genStartTime = Time.fixedTime;
        gridSize = grid.GridBoardSize;

        oldBlocks = blocks;
        blocks = new List<Block>();

        print("Generating level");
        UpdateRandomSeed(seed);

        print("Generating square blocks");
        GenerateSquareBlocks();
        print("Total number of blocks: " + blocks.Count);

        yield return new WaitForFixedUpdate();
        print("Assigning neighbours to blocks");
        AssignNeighboursOfBlocks();

        print("Randomly breaking some of the squares into triangles");
        BreakSquaresIntoTriangles();
        print("Total number of blocks: " + blocks.Count);

        // TODO: Carry block size calculation to block
        maxSizeBlock = blocks[0];
        maxBlockSize = CalculateBlockSize(blocks[0]);
        numPieces = Random.Range(minPieces, maxPieces);
        print("Randomly merging blocks to reduce the number of blocks to " + numPieces);

        while (blocks.Count > numPieces)
        {
            Block block = SelectRandomBlockExcludingMax();
            Block toBeMerged = SelectSmallestBlock(block.Neighbours);
            block.MergeWith(toBeMerged);
            if (demoMode)
            {
                textDemo.text = String.Format("Block at {0} is going to merge with block at {1}", block.placeOnGrid, toBeMerged.placeOnGrid);
                yield return null;
            }

            // TODO: Try to get rid of this, I use this because I use bounds of colliders
            yield return new WaitForFixedUpdate();

            UpdateMaxBlockIfBigger(block);

            block.CheckAndAddNewNeighbours(toBeMerged.Neighbours);
            EraseFromNeighboursList(toBeMerged);

            Vector2 toBeMergedPlace = toBeMerged.placeOnGrid;
            RemoveAndDestroy(toBeMerged);

            block.name = "Merged block: " + block.placeOnGrid;
        }
        print("Total number of blocks: " + blocks.Count);

        print("Assigning pastel colors to final blocks: ");
        AssignSelectedColorsToBlocks(Util.ColorsForBlocks);

        float genTime = Time.fixedTime - genStartTime;
        print(String.Format("Generated in {0:0.00} seconds", genTime));

        NotifyLevelFinishedChecker();
    }


    // Almost the same as GenerateLevel but with yield returns 
    // so that it can be traced step by step
    private IEnumerator GenerateLevelDemo(int seed = 0)
    {
        float genStartTime = Time.fixedTime;
        gridSize = grid.GridBoardSize;

        oldBlocks = blocks;
        blocks = new List<Block>();

        textDemo.text = "Welcome to level generation demo. You can use the buttons below buttons to proceed";
        yield return null;

        UpdateRandomSeed(seed);

        textDemo.text = "Generating square blocks";
        yield return null;

        GenerateSquareBlocks();
        textDemo.text = blocks.Count + " squares generated. Neighbour lists will be updated.";
        yield return null;

        AssignNeighboursOfBlocks();

        textDemo.text = "Randomly breaking some of the squares into triangles. Break probability: " + triangleBreakProbability;
        yield return null;

        BreakSquaresIntoTriangles();
        print("Total number of blocks: " + blocks.Count);

        // TODO: Carry block size calculation to block

        maxSizeBlock = blocks[0];
        maxBlockSize = CalculateBlockSize(blocks[0]);
        numPieces = Random.Range(minPieces, maxPieces);

        textDemo.text = "Randomly merging blocks to reduce the number of blocks to " + numPieces;
        yield return null;

        EnableRedDots();
        while (blocks.Count > numPieces)
        {
            Block block = SelectRandomBlockExcludingMax();
            Block toBeMerged = SelectSmallestBlock(block.Neighbours);
            textDemo.text = String.Format("Block at {0} is going to merge with block at {1}", block.placeOnGrid, toBeMerged.placeOnGrid);
            UpdateRedDotPositions(block.transform.position, toBeMerged.transform.position);
            yield return null;

            block.MergeWith(toBeMerged);

            // TODO: Try to get rid of this, I use this because I use bounds of colliders
            yield return new WaitForFixedUpdate();

            UpdateMaxBlockIfBigger(block);

            block.CheckAndAddNewNeighbours(toBeMerged.Neighbours);
            EraseFromNeighboursList(toBeMerged);

            Vector2 toBeMergedPlace = toBeMerged.placeOnGrid;
            RemoveAndDestroy(toBeMerged);

            block.name = "Merged block: " + block.placeOnGrid;
        }
        DisableRedDots();
        textDemo.text = String.Format("Assigning pre defined pastel colors to blocks");
        yield return null;

        AssignSelectedColorsToBlocks(Util.ColorsForBlocks);

        float genTime = Time.fixedTime - genStartTime;

        textDemo.text = String.Format("Level creation finished. Proceed one more step to start a new level generation.");
        demoFinished = true;
        demoPlayContinously = false;
        TMP_Text buttonText = buttonPlayPauseButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "Play";
        yield break;
    }

    private void EnableRedDots()
    {
        redDot.SetActive(true);
        redDot2.SetActive(true);
    }

    private void DisableRedDots()
    {
        redDot.SetActive(false);
        redDot2.SetActive(false);
    }

    private void UpdateRedDotPositions(Vector3 position1, Vector3 position2)
    {
        position1.z = -1;
        position2.z = -1;
        redDot.transform.position = position1;
        redDot2.transform.position = position2;
    }

    private void ProceedOneStepInDemo()
    {
        if (!demoFinished)
        {
            demoEnumerator.MoveNext();
        }
        else
        {
            DeleteAllBlocks();
            demoEnumerator = GenerateLevelDemo();
            demoEnumerator.MoveNext();
            demoFinished = false;
            demoPlayContinously = false;
            TMP_Text buttonText = buttonPlayPauseButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "Play";
        }
    }

    public void PlayPauseToggled()
    {
        demoPlayContinously = !demoPlayContinously;
        TMP_Text buttonText = buttonPlayPauseButton.GetComponentInChildren<TMP_Text>();
        if (demoPlayContinously)
        {
            buttonText.text = "Pause";
            StartCoroutine(MoveToNextStepRegularly());
        }
        else
        {
            buttonText.text = "Play";
        }
    }

    private IEnumerator MoveToNextStepRegularly()
    {
        while (!demoFinished && demoPlayContinously)
        {
            ProceedOneStepInDemo();
            yield return new WaitForSeconds(stepTimeForDemo);
        }
    }

    public void NextPressed()
    {
        ProceedOneStepInDemo();
    }


    private void DeleteAllBlocks()
    {
        DeleteOldBlocks();
        foreach (Block block in blocks)
        {
            Destroy(block.gameObject);
        }
        blocks = new List<Block>();
    }

    private void DeleteOldBlocks()
    {
        if (oldBlocks != null)
        {
            foreach (Block block in oldBlocks)
            {
                Destroy(block.gameObject);
            }
            oldBlocks = new List<Block>();
        }
    }

    private void UpdateMaxBlockIfBigger(Block block)
    {
        float blockSize = CalculateBlockSize(block);
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
            float blockSize = CalculateBlockSize(block);
            if (blockSize < minSize)
            {
                minSize = blockSize;
                minAreaBlock = block;
            }
        }

        return minAreaBlock;
    }

    private float CalculateBlockSize(Block block)
    {
        return block.GetBounds().size.sqrMagnitude;
    }

    private void UpdateRandomSeed(int seed)
    {
        if (seed == 0)
        {
            seed = Guid.NewGuid().GetHashCode();
        }
        seed = Math.Abs(seed) % 1000000;
        print("Setting seed to: " + seed);

        Random.InitState(seed);
        levelSeed = seed;
    }

    private void GenerateSquareBlocks()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 position = grid.transform.position;
                position += grid.GridStepSize * new Vector2(i, j);

                Block newBlock = Instantiate(blockPrefab, position, Quaternion.identity, this.transform);
                newBlock.BlockColor = GetRandomColor();
                newBlock.GenerateSquareBlock();
                newBlock.GenerateShapeAndCollider();
                newBlock.placeOnGrid = new Vector2Int(i, j);
                newBlock.name = String.Format("Square: {0}, {1}", i, j);
                blocks.Add(newBlock);
            }
        }
    }

    private void GiveRandomColorsToBlocks()
    {
        foreach (Block block in blocks)
        {
            block.BlockColor = GetRandomColor();
        }
    }

    private void AssignSelectedColorsToBlocks(List<Color> colors)
    {
        int ind = Random.Range(0, colors.Count - 1);
        foreach (Block block in blocks)
        {
            block.BlockColor = colors[ind];
            ind = (ind + 1) % colors.Count;
        }
    }

    private void AssignNeighboursOfBlocks()
    {
        // TODO: Not efficient for checking neighbours, but not a big deal
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

            Vector3 pos1 = block.transform.position;
            Vector3 pos2;
            Vector2Int placeOnGrid1 = block.placeOnGrid;
            Vector2Int placeOnGrid2;
            List<Vector2Int> corners1 = CopyVectorList(block.Corners);
            List<Vector2Int> corners2 = CopyVectorList(block.Corners);

            // Top-left to bottom right diagonal
            if (Random.Range(0f, 1f) < 0.5f)
            {
                corners1.RemoveAt(2);
                corners2.RemoveAt(0);
                corners2 = ResetOriginForCorners(corners2);
                pos2 = block.CornersInWorldSpace[1];
                placeOnGrid2 = block.placeOnGrid + block.Corners[1];
            }
            // Top-right to bottom left diagonal
            else
            {
                corners1.RemoveAt(3);
                corners2.RemoveAt(1);
                pos2 = block.transform.position;
                placeOnGrid2 = block.placeOnGrid;
            }
            Block triangle1 = Instantiate(blockPrefab, pos1, Quaternion.identity, this.transform);
            Block triangle2 = Instantiate(blockPrefab, pos2, Quaternion.identity, this.transform);

            triangle1.placeOnGrid = placeOnGrid1;
            triangle2.placeOnGrid = placeOnGrid2;

            triangle1.SetCorners(corners1);
            triangle2.SetCorners(corners2);

            triangle1.BlockColor = GetRandomColor();
            triangle2.BlockColor = GetRandomColor();

            triangle1.GenerateShapeAndCollider();
            triangle2.GenerateShapeAndCollider();

            triangle1.name = String.Format("Tri 1 from: {0}", block.name);
            triangle2.name = String.Format("Tri 2 from: {0}", block.name);

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
            RemoveAndDestroy(block);
        }
    }

    // TODO: Use generic type instead of Block
    private Block SelectRandomFromSet(HashSet<Block> neighbours)
    {
        int ind = Random.Range(1, neighbours.Count);
        foreach (Block block in neighbours)
        {
            ind--;
            if (ind == 0) { return block; }
        }
        return null;
    }

    private void EraseFromNeighboursList(Block removal)
    {
        foreach (Block block in blocks)
        {
            block.RemoveNeighbour(removal);
        }
    }

    private void RemoveAndDestroy(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
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

    private List<T> CopyVectorList<T>(List<T> original)
    {
        List<T> newList = new List<T>();
        foreach (T el in original)
        {
            newList.Add(el);
        }

        return newList;
    }

    private Color GetRandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        return new Color(r, g, b);
    }

    private void MakePiecesFall()
    {
        Vector3 origin = grid.transform.position;
        float addRange = grid.GridUnitySize - grid.GridStepSize;

        foreach (Block block in blocks)
        {
            float x = origin.x + Random.Range(0, addRange);
            float y = origin.y + Random.Range(0, addRange);
            float z = block.transform.position.z;
            block.transform.position = new Vector3(x, y, z);
            float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
            float stopY = Random.Range(blockFallingMinY, blockFallingMaxY);
            block.StartFalling(fallSpeed, stopY);
        }
    }

    private void NotifyLevelFinishedChecker()
    {
        if (levelFinishedChecker != null)
        {
            levelFinishedChecker.LevelResetted(blocks.Count);
        }
    }
}
