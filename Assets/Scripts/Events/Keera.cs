using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keera : MonoBehaviour
{
    public string displayName = "Keera";
    [Header("which corresponds to the state Keera is in")]
    [Header("each element is a seperate dialogue")]
    [TextArea(1, 5)]
    public List<string> stateDialogueLines;
    [TextArea(1, 5)]
    public string levelUpLines;
    private string[] dialogueLines;
    public int lineIndex;
    private bool allLinesShown;

    public int state;

    private DialogBox dialogBox;
    private InteractableGUI interactableGUI;
    private Player player;
    private GameManager gameManager;
    private HubManager hubManager;

    private bool playerInRange;

    public void Awake()
    {
        dialogBox = Resources.FindObjectsOfTypeAll<DialogBox>()[0];
        player = FindObjectOfType<Player>();
        gameManager = FindObjectOfType<GameManager>();
        hubManager = FindObjectOfType<HubManager>();
        interactableGUI = Resources.FindObjectsOfTypeAll<InteractableGUI>()[0];
    }

    public void SetState(int state)
    {
        Awake();
        this.state = state;
        if (state != -1)
            dialogueLines = stateDialogueLines[state].Split('\n');
        else
            dialogueLines = levelUpLines.Split('\n');
        lineIndex = 0;
        allLinesShown = false;
        if (state == 0)
            ShowNextLine();
        else if(!playerInRange)
            dialogBox.HideText();
    }

    void Update()
    {

        if ((state == 0 || playerInRange) && Input.GetButtonDown("Interact"))
        {
            if (!allLinesShown)
                ShowNextLine();
            else
                AllLinesShown();
        }
    }

    private void AllLinesShown()
    {
        if (state == 0)
        {
            player.hubSaveState.UnlockHubPortal(0);
            player.hubSaveState.UnlockHubPortal(1);
            player.hubSaveState.SetKeeraState(1);
            gameManager.SaveGame();
            hubManager.LoadHub(player.hubSaveState);
            SetState(1);
        }
        else if (state == 1)
        {
            SetState(-1);
        }
        else if (state == -1)
        {
            PlayerLevelUp();
            AskToLevelUp();
        }
        dialogBox.HideText();
    }

    private void AskToLevelUp()
    {
        interactableGUI.Show("Level up\n", transform, new Vector2(0, 2), player.priceToLevelUp.ToString(), "essence");
    }
    private void PlayerLevelUp()
    {
        if (player.essence >= player.priceToLevelUp)
        {
            if (player.essence > 0)
                player.essence -= player.priceToLevelUp;
            player.LevelUp();
            player.statusGUI.UpdateEssenceText();
            player.SetItemStats();
            player.statusGUI.UpdateHealthbar();
            gameManager.SaveGame(true);
            AskToLevelUp();
        }
    }

    private void ShowNextLine()
    {
        dialogBox.ShowText(displayName, dialogueLines[lineIndex], -1, true);
        lineIndex++;
        if (lineIndex >= dialogueLines.Length)
            allLinesShown = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (state != 0)
            {
                interactableGUI.Show("Talk", transform, new Vector2(0, 2));
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (state != 0)
            {
                dialogBox.HideText();
                interactableGUI.Hide();
                SetState(state);
            }
        }
    }
}
