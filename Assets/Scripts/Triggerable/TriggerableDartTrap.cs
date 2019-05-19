using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerableDartTrap : TriggerableTrap
{
    public enum Direction { Right, Left, Up, Down }

    public Transform dartSpawnPoint;
    public GameObject dartPrefab;
    public Direction direction;

    public float dartSpeed = 7f;
    public float dartDamage = 10f;

    [Range(1, 50)]
    public int dartShootCount = 1;
    [Range(0.1f, 1f)]
    public float dartShootDelay = 0.1f;

    private Animator animator;
    private AudioSource audioSource;
    public AudioClip shootDartClip;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Trigger()
    {
        StartCoroutine(ShootDarts());
    }

    private IEnumerator ShootDarts()
    {
        for (int i = 0; i < dartShootCount; i++)
        {

            if (dartShootCount == 1 || dartShootDelay == 1)
                animator.SetTrigger("Shoot");
            else
                animator.SetTrigger("ShootFast");
            yield return new WaitForSecondsRealtime(dartShootDelay);
        }
    }

    private void ShootEvent()
    {
        ProjectileDart dart = Instantiate(dartPrefab, dartSpawnPoint.position, Quaternion.identity, dartSpawnPoint.parent).GetComponent<ProjectileDart>();
        dart.Set(TriggerableDartTrap.GetVectorFromDirection(direction), dartSpeed, dartDamage);
        audioSource.clip = shootDartClip;
        audioSource.Play();
    }

    private static Vector2 GetVectorFromDirection(Direction d)
    {
        Vector2 dir;
        switch (d)
        {
            case Direction.Right:
                dir = Vector2.right;
                break;
            case Direction.Left:
                dir = Vector2.left;
                break;
            case Direction.Up:
                dir = Vector2.up;
                break;
            default: //Direction.Down:
                dir = Vector2.down;
                break;
        }
        return dir;
    }


}
