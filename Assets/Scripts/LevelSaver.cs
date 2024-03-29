using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class LevelSaver : MonoBehaviour
{
    LevelGenerator levelGenerator;

    [Serializable]
    struct LevelSaveData
    {
        public int gridSize;
        public int levelSeed;
        public int numPieces;
        public List<BlockData> blocks;
    }

    [Serializable]
    struct BlockData
    {
        public Color color;
        public List<SerializableVector2Int> corners;
    }

    void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveLevel();
        }
    }

    private void SaveLevel()
    {
        LevelSaveData data = new LevelSaveData();

        data.gridSize = levelGenerator.GridSize;
        data.levelSeed = levelGenerator.Seed;
        data.numPieces = levelGenerator.BlockObjects.Count;

        data.blocks = new List<BlockData>();
        foreach (BlockObject block in levelGenerator.BlockObjects)
        {
            BlockData blockData = new BlockData();
            blockData.corners = GetCorners(block.Corners);
            blockData.color = block.BlockColor;
            data.blocks.Add(blockData);
        }

        string filename = String.Format("level_{0}.json", data.levelSeed);
        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        SaveToJsonFile(data, fullPath);
        print("Level saved to: " + fullPath);
    }

    private void SaveToJsonFile(LevelSaveData data, string fullPath)
    {
        string jsonString = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, jsonString);
    }

    private List<SerializableVector2Int> GetCorners(List<Vector2Int> corners)
    {
        List<SerializableVector2Int> cornersSV2 = new List<SerializableVector2Int>();
        foreach (Vector2Int corner in corners)
        {
            cornersSV2.Add(new SerializableVector2Int(corner));
        }
        return cornersSV2;
    }
}
