using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUIController : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Util.SceneIndices.mainMenu);
    }
}
