using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void EasyClicked()
    {
        LevelSettings.getInstance().InitEasySettings();
        LoadGameScene();
    }

    public void MediumClicked()
    {
        LevelSettings.getInstance().InitMediumSettings();
        LoadGameScene();
    }

    public void HardClicked()
    {
        LevelSettings.getInstance().InitHardSettings();
        LoadGameScene();
    }

    public void LevelGenerationClicked()
    {
        SceneManager.LoadScene(Util.SceneIndices.levelGenerationDemo);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(Util.SceneIndices.game);
    }
}
