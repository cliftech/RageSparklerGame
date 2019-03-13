using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void LoadLevel(GameObject level, int potalId = -1)
    {
        levelToLoad = level;
        if (currentLevel != null)
            screenCover.CoverScreen(.1f, () => LoadLevel(potalId));
        else
            LoadLevel(potalId);
    }

    private void LoadLevel(int checkPointIndex)
    {
        if (currentLevel != null)
            DestroyCurrentLevel();
        currentLevel = Instantiate(levelToLoad).GetComponent<Level>();

        Vector2 pos;
        if (checkPointIndex == -1)
        {
             pos = currentLevel.spawnPoint.position;
        }
        else
        {
            pos = currentLevel.checkPoints.First(p => p.portalId == checkPointIndex).transform.position;
        }
        player.transform.position = pos;
        player.SetRespawnPos(pos);
        cameraController.SetBounds(currentLevel.LeftBound, currentLevel.TopBound, currentLevel.RightBound, currentLevel.BottomBound);
        areaNotificationText.ShowNotification(currentLevel.title);
        screenCover.UncoverScreen(.1f);
    }

    private void DestroyCurrentLevel()
    {
        Destroy(currentLevel.gameObject);
    }
}
