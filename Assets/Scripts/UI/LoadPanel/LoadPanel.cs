using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : MonoBehaviour
{
    private MainMenuManager mainMenuManager;

    public RectTransform canvasRect;
    public LoadProfileSlot profileSlot;
    private RectTransform rect;

    private Vector2 hiddenPos;
    private Vector2 centerPos;

    private bool isShowing;

    private void Awake()
    {
        mainMenuManager = GameObject.FindObjectOfType<MainMenuManager>();
        rect = GetComponent<RectTransform>();
    }
    private void Start()
    {
        hiddenPos = new Vector2(-10000, -10000);
        // Calculate *screen* position (note, not a canvas/recttransform position)
        Vector2 centerScreenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, centerScreenPoint, null, out centerPos);

        Hide();

        //for (int id = 0; id < 5; id++)
        //{
        //    SaveProfile p = new SaveProfile();
        //    p.id = id;
        //    p.lvl = id + 3;
        //    p.timePlayed = id * 6.21f + 0.235f;
        //    p.essence = id * 50;
        //    SaveManager.SaveProfile(p);
        //}        

        //var p = SaveManager.LoadProfile(0);
        //print(p);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        int slotCount = SaveManager.profileCount;
        if (slotCount <= 0)
            return;
        RectTransform pRect = profileSlot.GetComponent<RectTransform>();
        float yOffset = pRect.sizeDelta.y + 5;


        profileSlot.Set(1, SaveManager.LoadProfile(0), this);
        for (int i = 1; i < slotCount; i++)
        {
            LoadProfileSlot p = Instantiate(profileSlot.gameObject, profileSlot.transform.position, Quaternion.identity, profileSlot.transform.parent)
                .GetComponent<LoadProfileSlot>();
            p.Set(i + 1, SaveManager.LoadProfile(i), this);
            RectTransform r = p.GetComponent<RectTransform>();
            Vector2 pos = r.anchoredPosition;
            pos.y -= yOffset * i;
            r.anchoredPosition = pos;
        }

        rect.anchoredPosition = centerPos;
        isShowing = true;
    }

    public void Hide()
    {
        rect.position = hiddenPos;
        isShowing = false;
        this.gameObject.SetActive(false);
    }

    public void LoadSlot(SaveProfile profile)
    {
        mainMenuManager.LoadGame(profile.id);
    }
}
