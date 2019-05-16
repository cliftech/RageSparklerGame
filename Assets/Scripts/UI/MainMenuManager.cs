using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button loadGameButton;

    private void Start()
    {
        SaveManager.ValidateSaves();
        if (SaveManager.profileCount <= 0)
            loadGameButton.interactable = false;
    }

    private void SetProfileToLoad(int profileID)
    {
        PlayerPrefs.SetInt("SaveProfileToLoad", profileID);
        PlayerPrefs.Save();
    }

    public void LoadNewGame()
    {
        SetProfileToLoad(SaveManager.profileCount);
        LoadSceneById(2);
    }

    public void LoadGame(int profileID)
    {
        SetProfileToLoad(profileID);
        LoadSceneById(1);
    }

    public void LoadSceneById(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
