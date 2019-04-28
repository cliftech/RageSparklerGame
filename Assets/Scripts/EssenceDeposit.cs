using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceDeposit : MonoBehaviour
{
    new private SpriteRenderer renderer;
    private CameraController cameraController;
    private ParticleSystem hitEffect;

    public GameObject essencePrefab;
    public Sprite fullSprite;
    public Sprite halfEmptySprite;
    public int essenceStored = 20;
    public int maxHitsTaken = 6;

    private int essenceLeft;

    private bool hasCollidedInFrame = false;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        cameraController = FindObjectOfType<CameraController>();
        hitEffect = GetComponentInChildren<ParticleSystem>();
        renderer.sprite = fullSprite;
        essenceLeft = essenceStored;
    }

    private void DropEssence()
    {

        int essenceDropped;
        if (essenceLeft > essenceStored / maxHitsTaken)
            essenceDropped = essenceStored / maxHitsTaken;
        else
            essenceDropped = essenceLeft;

        for (int i = 0; i < essenceDropped; i++)
        {
            int isDirRight = Random.value > 0.5f ? 1 : -1;
            Vector2 position = transform.position;
            position.x += isDirRight * 0.2f;

            Rigidbody2D rb = Instantiate(essencePrefab, position, Quaternion.identity, transform.parent).GetComponent<Rigidbody2D>();
            Vector2 force = new Vector2(Random.Range(0, 0.5f) * isDirRight, Random.Range(0.2f, 0.5f)).normalized * 5;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        essenceLeft -= essenceDropped;
        if (essenceLeft <= 0)
            Destroy(this.gameObject);
        else if (essenceLeft * 2 < essenceStored)
            renderer.sprite = halfEmptySprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerWeapon") && !hasCollidedInFrame)
        {
            DropEssence();
            cameraController.Shake(5);
            hasCollidedInFrame = true;
            hitEffect.Play();
            StartCoroutine(ResetCollision());
        }
    }

    private IEnumerator ResetCollision()
    {
        yield return new WaitForEndOfFrame();
        hasCollidedInFrame = false;
    }
}
