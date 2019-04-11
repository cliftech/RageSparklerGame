using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private EssenceCollectorGUI essenceCollectorGUI;
    private AreaNotificationText areaNotificationText;
    private CameraController cameraController;
    private Player player;
    private ScreenCover screenCover;
    private SoundManager soundManager;

    public GameObject zerothArea;
    public GameObject hubArea;
    public enum OnStartLoadType { loadArea0, loadHub }
    public OnStartLoadType onStartLoadType = OnStartLoadType.loadHub;
    public bool overideSavedHubPosition = false;
    [Header("Set for build versions of the game")]
    public bool isBuildVersion;

    private GameObject currentLevelPrefab;
    private Level currentLevel;
    public bool isCurrLevelHub { get { return currentLevel != null && currentLevel.isHub; } }

    private Coroutine gameSavingAfterIntervalRoutine;

    void Awake()
    {
        essenceCollectorGUI = FindObjectOfType<EssenceCollectorGUI>();
        areaNotificationText = Resources.FindObjectsOfTypeAll<AreaNotificationText>()[0];
        cameraController = FindObjectOfType<CameraController>();
        player = FindObjectOfType<Player>();
        screenCover = FindObjectOfType<ScreenCover>();
        soundManager = FindObjectOfType<SoundManager>();
    }

    void Start()
    {
        int profileToLoad = PlayerPrefs.GetInt("SaveProfileToLoad", 0);
        print("Loading profile: " + profileToLoad);
        if (SaveManager.profileCount == 0 || profileToLoad >= SaveManager.profileCount)
        {
            // creating a new game save profile
            Vector3 playerHubPos = (onStartLoadType == OnStartLoadType.loadArea0 ? zerothArea : hubArea).GetComponent<Level>().spawnPoint.position;
            player.LoadFromProfile(new SaveProfile(profileToLoad, 1, 0, 0, 0, 0, player.equipment.GetItemIds(), player.hubChest.GetItemIds(),
                                                         player.checkpoints, playerHubPos.x, playerHubPos.y, false,
                                                         player.playerMovement.dashUnlocked, player.playerMovement.midAirDashUnlocked,
                                                         player.playerMovement.downwardAttackUnlocked, player.playerMovement.wallJumpingUnlocked,
                                                         player.playerMovement.maxJumpCount, player.playerMovement.dashDistance,
                                                         player.playerMovement.minDelayBetweenDashes, player.playerMovement.maxMidairDashesCount,
                                                         player.playerMovement.invincibilityFrameTime), false);
            SaveGame(true);
        }

        LoadGame(profileToLoad);

        if (!isBuildVersion)
        {
            if (onStartLoadType == OnStartLoadType.loadArea0)
                LoadLevel(zerothArea);
            else if (onStartLoadType == OnStartLoadType.loadHub)
                LoadLevel(hubArea);
        }
        else
        {
            if(!player.hubUnloked)
                LoadLevel(zerothArea);
            else
                LoadLevel(hubArea);
        }
    }

    private IEnumerator SaveGameAfterInterval(float interval)
    {
        yield return new WaitForSecondsRealtime(interval);
        SaveGame(true);
        gameSavingAfterIntervalRoutine = null;
    }

    public void SaveGame(bool force = false)
    {
        if (!force)
        {
            if (gameSavingAfterIntervalRoutine == null)
                gameSavingAfterIntervalRoutine = StartCoroutine(SaveGameAfterInterval(0.5f));
        }
        else
            SaveManager.SaveProfile(player.GetCurrentProfile());
    }

    public void LoadGame(int profileID = -1)
    {
        if (profileID == -1)
            profileID = 0;
        SaveProfile profile = SaveManager.LoadProfile(profileID);

        //print(profile);
        player.LoadFromProfile(profile, overideSavedHubPosition);
        player.equipment.LoadByIds(profile.itemsInInventory);
        player.hubChest.LoadByIds(profile.itemsInHubChest);
        player.SetItemStats();
        player.statusGUI.UpdateInventoryStats();
    }


    #region loading levels
    // loading levels in 3 steps --- cover screen -> load level -> uncover screen

    public void LoadLevel(GameObject level, int potalId = -1)
    {
        currentLevelPrefab = level;
        player.playerMovement.StopDashing();
        if (currentLevel != null)
            screenCover.CoverScreen(.1f, () => LoadLevel(potalId));
        else
            LoadLevel(potalId);
    }

    private void LoadLevel(int portalID)
    {
        if (currentLevel != null)
            DestroyCurrentLevel();
        currentLevel = Instantiate(currentLevelPrefab).GetComponent<Level>();

        Vector2 pos;
        if (portalID == -1)
        {
            pos = currentLevel.spawnPoint.position;
        }
        else
        {
            pos = currentLevel.checkPoints.First(p => p.portalId == portalID).transform.position;
        }
        player.transform.position = pos;
        if (currentLevel.isHub)
        {
            if (!player.hubUnloked)
                player.hubUnloked = true;

            EssenceCollector essenceCollector = currentLevel.GetComponentInChildren<EssenceCollector>();
            essenceCollector.Initialize();
            essenceCollector.UpdateEssenceCollector();
        }
        
        player.SetRespawnPortal(portalID);
        cameraController.SetBounds(currentLevel.LeftBound, currentLevel.TopBound, currentLevel.RightBound, currentLevel.BottomBound);
        areaNotificationText.ShowNotification(currentLevel.title);
        screenCover.UncoverScreen(.1f);
        if (currentLevel.backgroundMusic != null)
        {
            soundManager.PlayMusic(currentLevel.backgroundMusic);
        }
        player.AmuletFlash.SetAmuletFlash(currentLevel.DoesAmuletFlash, currentLevel.LevelEnd);
        SaveGame(true);
    }

    public void ResetLevel(int portalID)
    {
        LoadLevel(currentLevelPrefab, portalID);
    }

    private void DestroyCurrentLevel()
    {
        Destroy(currentLevel.gameObject);
    }
    #endregion
}
