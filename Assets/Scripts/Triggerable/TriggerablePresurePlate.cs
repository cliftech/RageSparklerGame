using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerablePresurePlate : MonoBehaviour
{
    public TriggerableTrap[] traps;
    public float resetTime = 5f;
    private Animator animator;

    public AudioClip activateSound;
    private AudioSource audioSource;
    public bool triggerOnCollision = false;

    private bool isReadyToTrigger = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        audioSource.clip = activateSound;
    }

    public void TriggerEvent()
    {
        if (!isReadyToTrigger)
            return;
        foreach(TriggerableTrap t in traps)
            if(t.gameObject.activeSelf)
                t.Trigger();
        isReadyToTrigger = false;
    }

    public void HasResetEvent()
    {

    }

    private IEnumerator Reset() {
        yield return new WaitForSecondsRealtime(resetTime);
        animator.SetTrigger("Reset");
        isReadyToTrigger = true;
    }

    public void TriggerAnimation(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (triggerOnCollision)
                TriggerEvent();
            animator.SetTrigger("Trigger");
            audioSource.Play();
            StartCoroutine(Reset());
        }
    }
}
