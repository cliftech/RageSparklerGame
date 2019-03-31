using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player : MonoBehaviour
{
    private LevelManager levelManager;
    [HideInInspector] public PlayerSoundController soundController;
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public TheFirstFlash AmuletFlash;
    [HideInInspector] public StatusGUI statusGUI;

    [HideInInspector] public int level;
    [HideInInspector] public float activeMaxHealth;
    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }

    public List<int> Checkpoints;

    public float attack1Dam = 5;
    public float attack2Dam = 7.5f;
    public float attack3Dam = 10;
    public float downwardAttackDam = 7.5f;
    public int essence;

    private float health;
    //private int level;
    private string enemyWeaponTag = "EnemyWeapon";
    private string trapTag = "Trap";
    private Action interactAction;
    private int respawnPortalID;

    [HideInInspector] public bool isDead;

    void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        soundController = GetComponentInChildren<PlayerSoundController>();
        playerMovement = GetComponent<PlayerMovement>();
        AmuletFlash = FindObjectOfType<TheFirstFlash>();
        statusGUI = FindObjectOfType<StatusGUI>();
    }
    void Start()
    {
        playerMovement.damageContainer.SetDamageCall(() => GetDamage());
        playerMovement.damageContainer.SetDoKnockbackCall(() => IsCurrnetAttackKnockingBack());
        essence = 0;
        level = 1;
        CalculateStats();
        statusGUI.UpdateEssenceText();
        statusGUI.UpdateHealthbar();
        statusGUI.UpdateLevelText();
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (interactAction != null)
                interactAction.Invoke();
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
        if (knockBackDirection == 2)
            playerMovement.KnockbackUp(damage);
        else if (knockBackDirection != 0)
            playerMovement.KnockBack(knockBackDirection == 1, damage);
        if (health <= 0)
            Die();
        statusGUI.UpdateHealthbar();

        soundController.PlayGetHitSound();
    }

    public void SetInteractAction(Action action)
    {
        interactAction = action;
    }
    public void ClearInteractAction(Action action)
    {
        if (interactAction == action)
            interactAction = null;
    }

    private void Die()
    {
        isDead = true;
        playerMovement.animator.SetBool("Dead", true);
        StartCoroutine(ReviveAfterTime(2f));
        essence = 0;
        statusGUI.UpdateEssenceText();
    }
    private IEnumerator ReviveAfterTime(float time)
    {
        yield return new WaitForSecondsRealtime(.5f);
        playerMovement.rb.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(time - .5f);
        Revive();
    }

    private void Revive()
    {
        isDead = false;
        health = activeMaxHealth;
        playerMovement.animator.SetBool("Dead", false);
        levelManager.ResetLevel(respawnPortalID);
        statusGUI.UpdateHealthbar();
        statusGUI.UpdateEssenceText();
    }
    public void SetRespawnPortal(int respawnPortalID)
    {
        this.respawnPortalID = respawnPortalID;
    }
    public void LevelUp()
    {
        level++;
        statusGUI.UpdateLevelText();
    }
    private void CalculateStats()
    {
        //activeMaxHealth = base_maxhealth + level.currentLevel * health_perLevel;
        //health = activeMaxHealth;
        SetHealthByLevel();
    }

    public void SetHealthByLevel()
    {
        activeMaxHealth = base_maxhealth + level * health_perLevel;
        health = activeMaxHealth;
    }
    public float GetDamage()
    {
        if (playerMovement.isDownwardAttacking)
            return downwardAttackDam;
        switch (playerMovement.currentAttackNum)
        {
            case 1:
                return attack1Dam;
            case 2:
                return attack2Dam;
            case 3:
                return attack3Dam;
        }
        Debug.LogError("current attack number is nor recognized: " + playerMovement.currentAttackNum);
        return -1;
    }

    public void AddHealth(float healthAmount)
    {
        healthAmount = activeMaxHealth * healthAmount / 100;
        health += healthAmount;
        if (health >= activeMaxHealth)
            health = activeMaxHealth;
        statusGUI.UpdateHealthbar();
    }

    public bool IsCurrnetAttackKnockingBack()
    {
        return playerMovement.currentAttackNum == 3;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyWeaponTag) && !isDead && !playerMovement.isInvulnerable)
            GetHit(other.transform.GetComponentInParent<DamageContainer>().GetDamage(),
                other.transform.position.x > transform.position.x ? 1 : -1);

        if (other.CompareTag("Projectile") && !isDead && !playerMovement.isInvulnerable)
        {
            Projectile p = other.transform.GetComponentInParent<Projectile>();
            GetHit(p.GetDamage(),
                other.transform.position.x > transform.position.x ? 1 : -1);
            //p.Explode();
        }

        if (other.gameObject.CompareTag("Pick Up"))
        {
            Destroy(other.gameObject);
            essence++;
            statusGUI.UpdateEssenceText();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(trapTag) && !playerMovement.isInvulnerable && !isDead)
        {
            GetHit(collision.collider.GetComponent<Trap>().damagePercent * activeMaxHealth / 100, 2);
        }
    }

    #region Upgrading skills
    public void UnlockDash()
    {
        playerMovement.dashUnlocked = true;
    }

    public void UnlockMidAirDash()
    {
        playerMovement.midAirDashUnlocked = true;
    }

    public void UnlockDownwardAttack()
    {
        playerMovement.downwardAttackUnlocked = true;
    }

    public void UpgradeAirJumpCount(int airJumpCount)
    {
        playerMovement.maxJumpCount = airJumpCount + 1;
    }

    public void UpgradeDashDistance(float distance)
    {
        playerMovement.dashDistance = distance;
    }

    public void UpgradeDelayBetweenDashed(float delay)
    {
        playerMovement.minDelayBetweenDashes = delay;
    }

    public void UpgradeMaxMidAirDashCount(int count)
    {
        playerMovement.maxMidairDashesCount = count;
    }

    public void UpgradeInvincibilityFrame(float frameTime)
    {
        playerMovement.invincibilityFrameTime = frameTime;
    }
    #endregion

    public void FootstepEffectEvent()
    {
        soundController.PlayFootstepSound();
    }
    public void AttackEffectEvent(int attacknum)
    {
        soundController.PlayAttackSound(attacknum);
    }
    public void AirAttackEffectEvent(int attacknum)
    {
        soundController.PlayAirAttackSound(attacknum);
    }
    public void DownwardAttackSlamEffectEvent()
    {
        soundController.PlayDownwardAttackCommence();
    }
}
