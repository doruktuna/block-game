using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    static LevelSettings instance = null;

    [HideInInspector] public int gridSize;
    [HideInInspector] public int minPieces;
    [HideInInspector] public int maxPieces;
    [HideInInspector] public float triangleBreakProbability;

    [SerializeField] int easyGridSize = 4;
    [SerializeField] int easyMinPieces = 5;
    [SerializeField] int easyMaxPieces = 6;
    [SerializeField][Range(0f, 1f)] float easyTriangleBreakProbability = .3f;

    [SerializeField] int mediumGridSize = 5;
    [SerializeField] int mediumMinPieces = 7;
    [SerializeField] int mediumMaxPieces = 8;
    [SerializeField][Range(0f, 1f)] float mediumTriangleBreakProbability = .6f;

    [SerializeField] int hardGridSize = 6;
    [SerializeField] int hardMinPieces = 10;
    [SerializeField] int hardMaxPieces = 11;
    [SerializeField][Range(0f, 1f)] float hardTriangleBreakProbability = .8f;

    void Start()
    {
        if (instance == null) { instance = this; }
        if (instance != this) { Destroy(gameObject); }
        DontDestroyOnLoad(this);
    }

    public static LevelSettings getInstance()
    {
        return instance;
    }

    public void InitEasySettings()
    {
        gridSize = easyGridSize;
        minPieces = easyMinPieces;
        maxPieces = easyMaxPieces;
        triangleBreakProbability = easyTriangleBreakProbability;
    }

    public void InitMediumSettings()
    {
        gridSize = mediumGridSize;
        minPieces = mediumMinPieces;
        maxPieces = mediumMaxPieces;
        triangleBreakProbability = mediumTriangleBreakProbability;
    }

    public void InitHardSettings()
    {
        gridSize = hardGridSize;
        minPieces = hardMinPieces;
        maxPieces = hardMaxPieces;
        triangleBreakProbability = hardTriangleBreakProbability;
    }

}
