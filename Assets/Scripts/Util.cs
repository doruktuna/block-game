using System.Collections.Generic;
using UnityEngine;

public class Util
{
    static List<Color> colorsForBlocks = new List<Color>
    {
        rgb(255, 207, 210),
        rgb(142, 236, 245),
        rgb(255, 228, 94),
        rgb(255, 99, 146),
        rgb(96, 211, 148),
        rgb(90, 169, 230),
        rgb(210, 231, 115),
        rgb(177, 143, 207),
        rgb(227, 101, 193),
        rgb(191, 10, 38),
        rgb(251, 133, 0),
        rgb(153, 0, 102),
    };
    static public List<Color> ColorsForBlocks { get { return colorsForBlocks; } }

    static Color rgb(int r, int g, int b)
    {
        float rf = ((float)r) / 255;
        float gf = ((float)g) / 255;
        float bf = ((float)b) / 255;
        return new Color(rf, gf, bf);
    }

    public static class Tags
    {
        public const string block = "Block";
        public const string grid = "Grid";
        public const string gridForGeneration = "GenerationGrid";
        public const string gridDot = "GridDot";
        public const string blocksContainer = "Blocks Container";
        public const string fallStopper = "FallStopper";
        public const string UICanvas = "UICanvas";
        public const string finishedTexts = "FinishedTexts";
    }

    public static class Names
    {
        public const string gridDotsParent = "Dots";
    }

    public static class SceneIndices
    {
        public const int mainMenu = 0;
        public const int game = 1;
        public const int levelGenerationDemo = 2;
    }
}
