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

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        audioSource.clip = activateSound;
    }

    public void TriggerEvent()
    {
        foreach(TriggerableTrap t in traps)
            if(t.gameObject.activeSelf)
                t.Trigger();
    }

    public void HasResetEvent()
    {

    }

    private IEnumerator Reset() {
        yield return new WaitForSecondsRealtime(resetTime);
        animator.SetTrigger("Reset");
    }

    public void TriggerAnimation(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            animator.SetTrigger("Trigger");
            audioSource.Play();
            StartCoroutine(Reset());
        }
    }
}
