using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmuletFlashTrigger : MonoBehaviour
{
    private TheFirstFlash Flash;

    private void Awake()
    {
        Flash = FindObjectOfType<TheFirstFlash>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Flash.StartFlashing();
    }
}
