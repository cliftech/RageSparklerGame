using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashParticleController : MonoBehaviour
{
    private Player player;
    public ParticleSystem dodgeleftParticles;
    public ParticleSystem dodgerightParticles;

    public void Awake()
    {
        player = GetComponent<Player>();
    }
    
    public void Play(int direction)
    {
        if (direction == 1)
            dodgerightParticles.Play();
        else
            dodgeleftParticles.Play();

        if (!player.playerMovement.isGrounded)
        {
            dodgeleftParticles.transform.position = player.playerMovement.capsColl.bounds.center;
            dodgerightParticles.transform.position = player.playerMovement.capsColl.bounds.center;
        }
        else
        {
            Vector2 pos = player.playerMovement.capsColl.bounds.center;
            pos.y = player.playerMovement.capsColl.bounds.min.y + 0.1f;
            dodgeleftParticles.transform.position = pos;
            dodgerightParticles.transform.position = pos;
        }
    }
}
