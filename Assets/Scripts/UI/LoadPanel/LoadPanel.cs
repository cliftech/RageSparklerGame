using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadPanel : MonoBehaviour
{
    private EventSystem eventSystem;
    private MainMenuManager mainMenuManager;

    public RectTransform canvasRect;
    public RectTransform editProfileButtons;
    public Button arrangeProfileButton, deleteProfileButton, agreeToDeleteSave, disagreeToDeleteSave;
    public Button backButton;
    public Text areYouSureDeleteText, selectProfileArrangeText;
    public LoadProfileSlot profileSlot;
    private RectTransform[] loadProfileSlots;
    private RectTransform rect;
    public int minProfilesToShow = 5;

    private Vector2 hiddenPos;
    private Vector2 centerPos;

    private bool isShowing;
    private int indexSelected;
    private bool isArrangeMode = false;
    private int indexSelected_arrangeMode;

    private void Awake()
    {
        eventSystem = FindObjectOfType<EventSystem>();
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
    }

    public void Show()
    {
        isArrangeMode = false;        
        this.gameObject.SetActive(true);
        int slotCount = SaveManager.profileCount;
        if (loadProfileSlots != null)
        {
            for (int j = 1; j < loadProfileSlots.Length; j++)
            {
                Destroy(loadProfileSlots[j].gameObject);
            }
        }
        loadProfileSlots = new RectTransform[slotCount];
        if (slotCount <= 0)
            return;
        RectTransform pRect = profileSlot.GetComponent<RectTransform>();
        loadProfileSlots[0] = pRect;
        float yOffset = pRect.sizeDelta.y + 5;


        profileSlot.Set(1, SaveManager.LoadProfile(0), this);
        int i = 1;
        for (; i < slotCount; i++)
        {
            LoadProfileSlot p = Instantiate(profileSlot.gameObject, profileSlot.transform.position, Quaternion.identity, profileSlot.transform.parent)
                .GetComponent<LoadProfileSlot>();
            p.Set(i + 1, SaveManager.LoadProfile(i), this);
            RectTransform r = p.GetComponent<RectTransform>();
            loadProfileSlots[i] = r;
            Vector2 pos = r.anchoredPosition;
            pos.y -= yOffset * i;
            r.anchoredPosition = pos;
        }
        //for(; i < minProfilesToShow; i++)
        //{
        //    LoadProfileSlot p = Instantiate(profileSlot.gameObject, profileSlot.transform.position, Quaternion.identity, profileSlot.transform.parent)
        //        .GetComponent<LoadProfileSlot>();
        //    p.SetNull(i + 1, this);
        //    RectTransform r = p.GetComponent<RectTransform>();
        //    loadProfileSlots[i] = r;
        //    Vector2 pos = r.anchoredPosition;
        //    pos.y -= yOffset * i;
        //    r.anchoredPosition = pos;
        //}

        rect.anchoredPosition = centerPos;
        isShowing = true;
        StartCoroutine(Wait());
        eventSystem.SetSelectedGameObject(loadProfileSlots[0].transform.Find("Button").gameObject);
    }

    IEnumerator Wait() {
        yield return new WaitForEndOfFrame();
        MoveProfileEditButtons(1);
    }

    public void MoveProfileEditButtons(int i)
    {
        if (isArrangeMode)
        {
            indexSelected_arrangeMode = i - 1;
            return;
        }
        indexSelected = i - 1;

        editProfileButtons.gameObject.SetActive(true);
        editProfileButtons.position = new Vector2(editProfileButtons.position.x, loadProfileSlots[indexSelected].position.y);

        Navigation nav = arrangeProfileButton.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = null;
        nav.selectOnDown = deleteProfileButton;
        nav.selectOnLeft = loadProfileSlots[indexSelected].transform.Find("Button").GetComponent<Button>();
        nav.selectOnRight = null;
        arrangeProfileButton.navigation = nav;

        nav = deleteProfileButton.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = arrangeProfileButton;
        nav.selectOnDown = null;
        nav.selectOnLeft = loadProfileSlots[indexSelected].transform.Find("Button").GetComponent<Button>();
        nav.selectOnRight = null;
        deleteProfileButton.navigation = nav;

        arrangeProfileButton.gameObject.SetActive(true);
        deleteProfileButton.gameObject.SetActive(true);
        arrangeProfileButton.interactable = true;
        deleteProfileButton.interactable = true;
        areYouSureDeleteText.gameObject.SetActive(false);
        selectProfileArrangeText.gameObject.SetActive(false);

        agreeToDeleteSave.gameObject.SetActive(false);
        disagreeToDeleteSave.gameObject.SetActive(false);

    }

    public void HideProfileEditButtons()
    {
        editProfileButtons.gameObject.SetActive(false);
    }

    public void Hide()
    {
        rect.position = hiddenPos;
        isShowing = false;
        this.gameObject.SetActive(false);
    }

    public void LoadSlot(SaveProfile profile)
    {
        if (isArrangeMode)
        {
            ExchangeProfilesPressed();
            return;
        }
        mainMenuManager.LoadGame(profile.id);
    }

    public void DeleteProfileCall()
    {
        areYouSureDeleteText.gameObject.SetActive(true);
        deleteProfileButton.gameObject.SetActive(false);
        selectProfileArrangeText.gameObject.SetActive(false);
        arrangeProfileButton.interactable = false;

        Navigation nav = agreeToDeleteSave.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = null;
        nav.selectOnDown = null;
        nav.selectOnLeft = loadProfileSlots[indexSelected].transform.Find("Button").GetComponent<Button>();
        nav.selectOnRight = disagreeToDeleteSave;
        agreeToDeleteSave.navigation = nav;

        nav = disagreeToDeleteSave.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = null;
        nav.selectOnDown = null;
        nav.selectOnLeft = agreeToDeleteSave;
        nav.selectOnRight = null;
        disagreeToDeleteSave.navigation = nav;

        agreeToDeleteSave.gameObject.SetActive(true);
        disagreeToDeleteSave.gameObject.SetActive(true);

        eventSystem.SetSelectedGameObject(disagreeToDeleteSave.gameObject);
    }

    public void AgreeToDeleteProfileCall()
    {
        SaveManager.DeleteProfile(indexSelected);
        if (SaveManager.profileCount > 0)
        {
            Show();
            MoveProfileEditButtons(loadProfileSlots.Length < (indexSelected + 1) ? loadProfileSlots.Length : (indexSelected + 1));
        }
        else
        {
            backButton.onClick.Invoke();
        }
    }

    public void DisagreeToDeleteProfileCall()
    {
        MoveProfileEditButtons(indexSelected + 1);
        eventSystem.SetSelectedGameObject(loadProfileSlots[indexSelected].transform.Find("Button").gameObject);
    }

    public void ArrangeProfileCall()
    {
        areYouSureDeleteText.gameObject.SetActive(false);
        selectProfileArrangeText.gameObject.SetActive(true);
        deleteProfileButton.interactable = false;

        Navigation nav = arrangeProfileButton.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = null;
        nav.selectOnDown = null;
        nav.selectOnLeft = loadProfileSlots[indexSelected].transform.Find("Button").GetComponent<Button>();
        nav.selectOnRight = null;
        arrangeProfileButton.navigation = nav;

        isArrangeMode = true;
        eventSystem.SetSelectedGameObject(loadProfileSlots[indexSelected].transform.Find("Button").gameObject);
    }

    private void ExchangeProfilesPressed()
    {
        isArrangeMode = false;
        SaveManager.ExhangeProfiles(indexSelected, indexSelected_arrangeMode);
        Show();
        MoveProfileEditButtons(indexSelected + 1);
        eventSystem.SetSelectedGameObject(loadProfileSlots[indexSelected].transform.Find("Button").gameObject);
    }
}
