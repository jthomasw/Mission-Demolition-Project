using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour
{

    public void StartGame()
    {
        Debug.Log("StartGame function called.");
        SceneManager.LoadScene("_Scene_0");
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame function called.");
        UnityEditor.EditorApplication.ExitPlaymode();
        Application.Quit();
    }

    public void GameOver()
    {
        Debug.Log("GameOver function called.");
        SceneManager.LoadScene("_Scene_GameOver");
    }
}
