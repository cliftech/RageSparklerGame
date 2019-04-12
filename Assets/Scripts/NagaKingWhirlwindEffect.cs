using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NagaKingWhirlwindEffect : MonoBehaviour
{
    private CameraController cameraController;
    public ParticleSystem loop, burst;
    public AudioClip soundLoop;
    private AudioSource audioSource;
    private Player player;

    public float suctionDelay = 1f;
    public float maxSuctionDistance = 5f;
    public float suctionStrength = 1;

    private float timer;
    private bool isSucking;

    public void Set(Vector3 pos, float time)
    {
        player = FindObjectOfType<Player>();
        cameraController = FindObjectOfType<CameraController>();
        audioSource = GetComponent<AudioSource>();
        transform.position = pos;

        timer = time;
        burst.Play();
        loop.Play();
        audioSource.clip = soundLoop;
        audioSource.loop = true;
        audioSource.Play();
        StartCoroutine(DelayedSuctionEffect(suctionDelay));
    }

    IEnumerator DelayedSuctionEffect(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        burst.Stop();
        isSucking = true;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        cameraController.Shake(10);
        if (isSucking)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < maxSuctionDistance)
            {
                Vector2 dir = transform.position - player.transform.position;
                dir.y /= 10f;
                player.playerMovement.SetExternalVelocity(dir * suctionStrength);
            }
            else
                player.playerMovement.SetExternalVelocity(Vector2.zero);

        }
        if (timer <= 0)
        {
            player.playerMovement.SetExternalVelocity(Vector2.zero);
            loop.Stop();
            burst.Stop();
            audioSource.Stop();
            StopAllCoroutines();
            this.enabled = false;
            Destroy(gameObject, 3f);
        }
    }
}
