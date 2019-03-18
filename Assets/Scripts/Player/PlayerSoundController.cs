using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    private Player player;

    public AudioSource audioSource;
    public AudioSource footstepSource;

    public AudioClip footstepSound, jumpGroundedSound, jumpInAirSound, landSound, getHitSound, dashGroundedSound, dashInAirSound;
    public AudioClip attack1Sound, attack2Sound, attack3Sound, airAttack1Sound, airAttack2Sound, downwardAttackStartSound, downwardAttackCommenceSound
        ;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    public void PlayFootstepSound()
    {
        footstepSource.PlayOneShot(footstepSound);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(player.playerMovement.isGrounded ? jumpGroundedSound : jumpInAirSound);
    }
    public void PlayLandSound()
    {
        audioSource.PlayOneShot(landSound);
    }
    public void PlayGetHitSound()
    {
        audioSource.PlayOneShot(getHitSound);
    }
    public void PlayDashSound()
    {
        audioSource.PlayOneShot(player.playerMovement.isGrounded ? dashGroundedSound : dashInAirSound);
    }
    public void PlayAttackSound(int attackNum)
    {
        audioSource.PlayOneShot(attackNum == 1 ? attack1Sound : attackNum == 2 ? attack2Sound : attack3Sound);
    }
    public void PlayAirAttackSound(int attackNum)
    {
        audioSource.PlayOneShot(attackNum == 1 ? airAttack1Sound : airAttack2Sound);
    }
    public void PlayDownwardAttackStart()
    {
        audioSource.PlayOneShot(downwardAttackStartSound);
    }
    public void PlayDownwardAttackCommence()
    {
        audioSource.PlayOneShot(downwardAttackCommenceSound);
    }
}
