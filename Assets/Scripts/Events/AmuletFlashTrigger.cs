using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmuletFlashTrigger : MonoBehaviour
{
    private TheFirstFlash Flash;
    private DialogBox dialogBox;
    [TextArea(1, 5)]public string dialogText = "Cats rule this world. Cats rule this world. Cats rule this world. Cats rule this world. Cats rule this world.";
    private bool triggered = false;

    private void Awake()
    {
        Flash = FindObjectOfType<TheFirstFlash>();
        dialogBox = Resources.FindObjectsOfTypeAll<DialogBox>()[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            Flash.StartFlashing();
            dialogBox.ShowText(dialogBox.playerName, dialogText);
            triggered = true;
        }   
    }
}
