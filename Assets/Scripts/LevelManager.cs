using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private AreaNotificationText areaNotificationText;
    private CameraController cameraController;
    private Player player;
    private ScreenCover screenCover;

    public GameObject zerothArea;
    public GameObject hubArea;
    public enum OnStartLoadType { loadArea0, loadHub }
    public OnStartLoadType onStartLoadType = OnStartLoadType.loadHub;

    private GameObject levelToLoad;
    private Level currentLevel;
    void Awake()
    {
        areaNotificationText = FindObjectOfType<AreaNotificationText>();
        cameraController = FindObjectOfType<CameraController>();
        player = FindObjectOfType<Player>();
        screenCover = FindObjectOfType<ScreenCover>();
    }

    void Start()
    {
        if(onStartLoadType == OnStartLoadType.loadArea0)
            LoadLevel(zerothArea);
        else if(onStartLoadType == OnStartLoadType.loadHub)
            LoadLevel(hubArea);
    }

    // loading levels in 3 steps --- cover screen -> load level -> uncover screen

    public void LoadLevel(GameObject level)
    {
        levelToLoad = level;
        if (currentLevel != null)
            screenCover.CoverScreen(.1f, () => LoadLevel());
        else
            LoadLevel();
    }

    private void LoadLevel()
    {
        if (currentLevel != null)
            DestroyCurrentLevel();
        currentLevel = Instantiate(levelToLoad).GetComponent<Level>();

        player.transform.position = currentLevel.spawnPoint.position;
        player.SetRespawnPos(currentLevel.spawnPoint.position);
        cameraController.SetBounds(currentLevel.LeftBound, currentLevel.TopBound, currentLevel.RightBound, currentLevel.BottomBound);
        areaNotificationText.ShowNotification(currentLevel.title);
        screenCover.UncoverScreen(.1f);
    }

    private void DestroyCurrentLevel()
    {
        Destroy(currentLevel.gameObject);
    }
}
