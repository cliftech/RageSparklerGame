using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public int Level { get { return level; } }
    public string enemyWeaponTag;
    public Text coinText;
    public Text healhtText;

    private float activeMaxHealth;
    private float health;
    private int level;
    private int coins;

    [HideInInspector] public bool isDead;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        playerMovement.damageContainer.SetDamageCall(() => GetDamage());
        Initialize();
        coins = 0;
        SetCoinText();
        SetHealthText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            print("DON'T CHEAT!!!!!!!!!!!!!!!");
            Revive();
        }
    }

    /// <summary>
    /// Hits the player
    /// </summary>
    /// <param name="damage">the amount of damage to deal</param>
    /// <param name="knockBackDirection">set to 1 if attack came from right of PC, -1 if from left, leave 0 if no knockback</param>
    public void GetHit(float damage, int knockBackDirection = 0)
    {
        health -= damage;
        print("PC Health - " + health);
        if(knockBackDirection != 0)
            playerMovement.KnockBack(knockBackDirection == 1, damage);
        if (health <= 0)
            Die();
        SetHealthText();
    }

    private void Die()
    {
        isDead = true;
        playerMovement.animator.SetBool("Dead", true);
    }

    private void Revive()
    {
        isDead = false;
        health = activeMaxHealth;
        playerMovement.animator.SetBool("Dead", false);
    }

    private void Initialize()
    {
        activeMaxHealth = base_maxhealth + level * health_perLevel;
        health = activeMaxHealth;
    }

    public float GetDamage()
    {
        return (playerMovement.attackComboCount + 1) * 5;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyWeaponTag) && !isDead && !playerMovement.isInvalnurable)
            GetHit(other.transform.GetComponentInParent<DamageContainer>().GetDamage(),
                other.transform.position.x > transform.position.x ? 1 : -1);

        if (other.gameObject.CompareTag("Pick Up"))
        {
            Destroy(other.gameObject);
            coins++;
            SetCoinText();
        }
    }
    void SetCoinText()
    {
        coinText.text = "Coins: " + coins.ToString();
    }

    void SetHealthText()
    {
        healhtText.text = "Health: " + health.ToString("0") + activeMaxHealth.ToString("0");
    }
}
