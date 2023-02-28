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
    [SerializeField] BlockObject blockObjectPrefab;
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

    int seed = 0;
    public int Seed { get { return seed; } }

    PuzzleGenerator puzzleGenerator;

    GridPuzzle grid;
    LevelFinishedChecker levelFinishedChecker;

    List<BlockObject> blockObjects;
    public List<BlockObject> BlockObjects { get { return blockObjects; } }

    List<BlockObject> oldBlockObjects;

    float blockFallingMinY;
    float blockFallingMaxY;

    bool demoMode = false;
    IEnumerator demoEnumerator = null;
    bool demoFinished = false;
    bool demoPlayContinously = false;

    void Awake()
    {
        ReadSettings();
        puzzleGenerator = new PuzzleGenerator(gridSize, triangleBreakProbability, seed);
        blockObjects = new List<BlockObject>();
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
            gridSize = levelSettings.gridSize;
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

    // TODO: With the new system we should not need two grids
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
        // TODO: Enable demo mode
        if (demoMode)
        {
            // demoEnumerator = GenerateLevelDemo();
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

        print("Generating level");
        numPieces = Random.Range(minPieces, maxPieces);
        puzzleGenerator.GenerateLevel(numPieces);

        float genTime = Time.fixedTime - genStartTime;
        print(String.Format("Generated in {0:0.00} seconds", genTime));

        CreateBlockObjects();
        AssignSelectedColorsToBlocks(Util.ColorsForBlocks);
        NotifyLevelFinishedChecker();

        yield return null;
    }

    private void CreateBlockObjects()
    {
        DeleteAllBlocks();
        foreach (Block block in puzzleGenerator.Blocks)
        {
            Vector2 position = grid.transform.position;
            position += grid.GridStepSize * (Vector2)block.placeOnGrid;

            BlockObject blockObject = Instantiate(blockObjectPrefab, this.transform);
            blockObject.transform.position = position;

            blockObject.block = block;
            blockObject.GenerateShapeAndCollider();

            blockObjects.Add(blockObject);
        }
    }


    // Almost the same as GenerateLevel but with yield returns 
    // so that it can be traced step by step
    // private IEnumerator GenerateLevelDemo(int seed = 0)
    // {
    //     float genStartTime = Time.fixedTime;
    //     gridSize = grid.GridBoardSize;

    //     oldBlocks = blocks;
    //     blocks = new List<BlockObject>();

    //     textDemo.text = "Welcome to level generation demo. You can use the buttons below buttons to proceed";
    //     yield return null;

    //     UpdateRandomSeed(seed);

    //     textDemo.text = "Generating square blocks";
    //     yield return null;

    //     GenerateSquareBlocks();
    //     textDemo.text = blocks.Count + " squares generated. Neighbour lists will be updated.";
    //     yield return null;

    //     AssignNeighboursOfBlocks();

    //     textDemo.text = "Randomly breaking some of the squares into triangles. Break probability: " + triangleBreakProbability;
    //     yield return null;

    //     BreakSquaresIntoTriangles();
    //     print("Total number of blocks: " + blocks.Count);

    //     // TODO: Carry block size calculation to block

    //     maxSizeBlock = blocks[0];
    //     maxBlockSize = CalculateBlockSize(blocks[0]);
    //     numPieces = Random.Range(minPieces, maxPieces);

    //     textDemo.text = "Randomly merging blocks to reduce the number of blocks to " + numPieces;
    //     yield return null;

    //     EnableRedDots();
    //     while (blocks.Count > numPieces)
    //     {
    //         BlockObject block = SelectRandomBlockExcludingMax();
    //         BlockObject toBeMerged = SelectSmallestBlock(block.Neighbours);
    //         textDemo.text = String.Format("Block at {0} is going to merge with block at {1}", block.placeOnGrid, toBeMerged.placeOnGrid);
    //         UpdateRedDotPositions(block.transform.position, toBeMerged.transform.position);
    //         yield return null;

    //         block.MergeWith(toBeMerged);

    //         // TODO: Try to get rid of this, I use this because I use bounds of colliders
    //         yield return new WaitForFixedUpdate();

    //         UpdateMaxBlockIfBigger(block);

    //         block.CheckAndAddNewNeighbours(toBeMerged.Neighbours);
    //         EraseFromNeighboursList(toBeMerged);

    //         Vector2 toBeMergedPlace = toBeMerged.placeOnGrid;
    //         RemoveAndDestroy(toBeMerged);

    //         block.name = "Merged block: " + block.placeOnGrid;
    //     }
    //     DisableRedDots();
    //     textDemo.text = String.Format("Assigning pre defined pastel colors to blocks");
    //     yield return null;

    //     AssignSelectedColorsToBlocks(Util.ColorsForBlocks);

    //     float genTime = Time.fixedTime - genStartTime;

    //     textDemo.text = String.Format("Level creation finished. Proceed one more step to start a new level generation.");
    //     demoFinished = true;
    //     demoPlayContinously = false;
    //     TMP_Text buttonText = buttonPlayPauseButton.GetComponentInChildren<TMP_Text>();
    //     buttonText.text = "Play";
    //     yield break;
    // }

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
            // TODO: Demo code, check here
            // demoEnumerator = GenerateLevelDemo();
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
        foreach (BlockObject block in blockObjects)
        {
            Destroy(block.gameObject);
        }
        blockObjects = new List<BlockObject>();
    }

    private void DeleteOldBlocks()
    {
        if (oldBlockObjects != null)
        {
            foreach (BlockObject block in oldBlockObjects)
            {
                Destroy(block.gameObject);
            }
            oldBlockObjects = new List<BlockObject>();
        }
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
        this.seed = seed;
    }

    private void AssignSelectedColorsToBlocks(List<Color> colors)
    {
        int ind = Random.Range(0, colors.Count - 1);
        foreach (BlockObject blockObject in blockObjects)
        {
            blockObject.BlockColor = colors[ind];
            ind = (ind + 1) % colors.Count;
        }
    }

    // TODO: Use generic type instead of Block
    private BlockObject SelectRandomFromSet(HashSet<BlockObject> neighbours)
    {
        int ind = Random.Range(1, neighbours.Count);
        foreach (BlockObject block in neighbours)
        {
            ind--;
            if (ind == 0) { return block; }
        }
        return null;
    }



    private void RemoveAndDestroy(BlockObject block)
    {
        blockObjects.Remove(block);
        Destroy(block.gameObject);
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

        foreach (BlockObject block in blockObjects)
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
            levelFinishedChecker.LevelResetted(blockObjects.Count);
        }
    }
}
