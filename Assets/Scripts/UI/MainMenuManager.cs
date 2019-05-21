﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public Button loadGameButton;
    public Button continueButton;
    public Button newGameButton;

    private Settings settings;
    private EventSystem eventSystem;

    private void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();

        settings = SaveManager.LoadSettings();
        if (settings == null)
        {
            settings = new Settings(-1, 0, true);
            SaveManager.SaveSettings(settings);
            continueButton.interactable = false;
        }

        SaveManager.ValidateSaves();
        if (SaveManager.profileCount <= 0)
            loadGameButton.interactable = false;

        SetFirstButtonActive();
    }

    private void SetProfileToLoad(int profileID)
    {
        settings.profileToLoad = profileID;
        SaveManager.SaveSettings(settings);
    }

    public void ContinueGame()
    {
        settings.firstTimeLoadingProfile = false;
        SetProfileToLoad(settings.lastSavedProfile);
        LoadSceneById(1);
    }

    public void LoadNewGame()
    {
        settings.firstTimeLoadingProfile = true;
        SetProfileToLoad(SaveManager.profileCount);
        LoadSceneById(2);
    }

    public void LoadGame(int profileID)
    {
        settings.firstTimeLoadingProfile = false;
        SetProfileToLoad(profileID);
        LoadSceneById(1);
    }

    public void LoadSceneById(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }

    public void SetFirstButtonActive()
    {
        if (continueButton.interactable)
            eventSystem.SetSelectedGameObject(continueButton.gameObject);
        else
            eventSystem.SetSelectedGameObject(newGameButton.gameObject);
    }
}
