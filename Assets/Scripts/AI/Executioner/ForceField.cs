using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    private CameraController cameraController;
    private ParticleSystem ps1, ps2;
    new private Collider2D collider;
    private Animator animator;

    public AudioClip soundLoop;
    private AudioSource audioSource;

    private float timer;

    public void Set(Vector3 pos, float size, float time)
    {
        cameraController = FindObjectOfType<CameraController>();
        ps1 = GetComponent<ParticleSystem>();
        ps2 = transform.Find("Sparks").GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        transform.localScale = Vector3.one * size;
        ps2.transform.localScale = Vector3.one * size * 1.2f;
        transform.position = pos;

        timer = time;
        ps2.Play();
        ps1.Play();
        animator.SetTrigger("Expand");
        audioSource.clip = soundLoop;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        cameraController.Shake(10);
        if (timer <= 0)
        {
            ps2.Stop();
            ps1.Stop();
            audioSource.Stop();
            this.enabled = false;
            collider.enabled = false;
            Destroy(gameObject, 1f);
        }
    }
}
