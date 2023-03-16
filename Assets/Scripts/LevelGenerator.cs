using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PuzzleGenerator;
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

    float blockFallingMinY;
    float blockFallingMaxY;

    bool demoMode = false;
    bool demoPlayContinously = false;
    bool isAtEndOfDemo = false;

    void Awake()
    {
        ReadSettings();
    }

    void Start()
    {
        AssignBlockFallingYRange();
        AssignGrid();

        puzzleGenerator = new PuzzleGenerator(gridSize, triangleBreakProbability, seed);
        blockObjects = new List<BlockObject>();

        levelFinishedChecker = FindObjectOfType<LevelFinishedChecker>();

        demoMode = !grid.CompareTag(Util.Tags.gridForGeneration);
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateLevel();
        }

        if (Input.GetKeyDown(KeyCode.N))
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

    public void GenerateLevel()
    {
        DeleteBlockObjects();

        if (demoMode)
        {
            isAtEndOfDemo = false;
            textDemo.text = "Welcome to level generation demo. You can use the buttons below buttons to proceed";
        }

        numPieces = Random.Range(minPieces, maxPieces);
        puzzleGenerator.GenerateLevel(numPieces, demoMode);

        if (demoMode) { return; }

        CreateBlockObjects();
        PresentPieces();
        NotifyLevelFinishedChecker();
    }

    private void CreateBlockObjects()
    {
        DeleteBlockObjects();

        int count = 1;
        foreach (Block block in puzzleGenerator.Blocks)
        {
            Vector2 position = grid.transform.position;
            position += grid.GridStepSize * (Vector2)block.placeOnGrid;

            BlockObject blockObject = Instantiate(blockObjectPrefab, this.transform);
            blockObject.transform.position = position;
            blockObject.name = String.Format("Block {0} ({1})", count++, block.placeOnGrid);

            blockObject.block = block;
            blockObject.GenerateShapeAndCollider();

            blockObjects.Add(blockObject);
        }
    }

    public void PresentPieces()
    {
        if (grid.CompareTag(Util.Tags.gridForGeneration))
        {
            MakePiecesFall();
        }
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
        if (!puzzleGenerator.IsGenerationFinished)
        {
            puzzleGenerator.ProceedToNextStep();
            DemoInfo demoInfo = puzzleGenerator.demoInfo;

            CreateBlockObjects();

            if (!demoInfo.isMergingStage)
            {
                textDemo.text = puzzleGenerator.demoInfo.message;
                DisableRedDots();
            }
            else
            {
                EnableRedDots();
                BlockObject blockObject1 = blockObjects[demoInfo.merge1Ind];
                BlockObject blockObject2 = blockObjects[demoInfo.merge2Ind];

                textDemo.text = String.Format("Merging {0} with {1}", blockObject1.name, blockObject2.name);
                UpdateRedDotPositions(blockObject1.CenterPosition, blockObject2.CenterPosition);
            }
        }
        else if (!isAtEndOfDemo)
        {
            demoPlayContinously = false;
            TMP_Text buttonText = buttonPlayPauseButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "Play";
            isAtEndOfDemo = true;
        }
        else
        {
            DeleteBlockObjects();
            GenerateLevel();
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
        while (!isAtEndOfDemo && demoPlayContinously)
        {
            ProceedOneStepInDemo();
            yield return new WaitForSeconds(stepTimeForDemo);
        }
    }

    public void NextPressed()
    {
        ProceedOneStepInDemo();
    }

    private void DeleteBlockObjects()
    {
        foreach (BlockObject block in blockObjects)
        {
            Destroy(block.gameObject);
        }
        blockObjects = new List<BlockObject>();
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
