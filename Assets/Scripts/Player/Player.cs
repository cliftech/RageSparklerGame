using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player : MonoBehaviour
{
    private CameraController cameraController;
    private PlayerInteract plrInter;
    private GameManager gamemanager;
    [HideInInspector] public Inventory equipment;
    [HideInInspector] public Inventory hubChest;
    [HideInInspector] public PlayerSoundController soundController;
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public TheFirstFlash AmuletFlash;
    [HideInInspector] public StatusGUI statusGUI;

    [HideInInspector] public int level;
    [HideInInspector] public float activeMaxHealth;
    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public float Armor = 0;

    [HideInInspector] public List<int> checkpoints;

    public float attack1Dam = 5;
    public float attack2Dam = 7.5f;
    public float attack3Dam = 10;
    public float downwardAttackDam = 7.5f;
    public int essence;
    public int storedEssence;

    private float health;
    //private int level;
    private string enemyWeaponTag = "EnemyWeapon";
    private string trapTag = "Trap";
    private Action interactAction;
    private int respawnPortalID;

    [HideInInspector] public bool isDead;
    [HideInInspector] public int currentProfileID;
    [HideInInspector] public float timePlayed;
    [HideInInspector] private Vector3 lastPosInHub;
    [HideInInspector] public int numberOfDeaths;
    [HideInInspector] public bool hubUnloked;

    void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        plrInter = FindObjectOfType<PlayerInteract>();
        gamemanager = FindObjectOfType<GameManager>();
        soundController = GetComponentInChildren<PlayerSoundController>();
        playerMovement = GetComponent<PlayerMovement>();
        AmuletFlash = FindObjectOfType<TheFirstFlash>();
        statusGUI = FindObjectOfType<StatusGUI>();
        Inventory[] equipments = GetComponents<Inventory>();
        for (int i = 0; i < equipments.Length; i++)
        {
            if (equipments[i].inventoryUI.name == "EquipmentUI")
                equipment = equipments[i];
            else
                hubChest = equipments[i];
        }
    }
    void Start()
    {
        playerMovement.damageContainer.SetDamageCall(() => GetDamage());
        playerMovement.damageContainer.SetDoKnockbackCall(() => IsCurrnetAttackKnockingBack());
        essence = 0;
        level = 1;
        SetItemStats();
        health = activeMaxHealth;
        statusGUI.UpdateEssenceText();
        statusGUI.UpdateHealthbar();
        statusGUI.UpdateLevelText();
        statusGUI.UpdateInventoryStats();
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (interactAction != null)
                interactAction.Invoke();
        }
        timePlayed += Time.deltaTime;
    }

    /// <summary>
    /// Hits the player
    /// </summary>
    /// <param name="damage">the amount of damage to deal</param>
    /// <param name="knockBackDirection">set to 1 if attack came from right of PC, -1 if from left, leave 0 if no knockback</param>
    public void GetHit(float damage, int knockBackDirection = 0)
    {
        float damageReduction = 1 - (0.052f * Armor)/(0.9f+0.048f * Armor);    
        health -= damage * damageReduction;
        if (knockBackDirection == 2)
            playerMovement.KnockbackUp(damage);
        else if (knockBackDirection != 0)
            playerMovement.KnockBack(knockBackDirection == 1, damage);
        if (health <= 0)
            Die();
        statusGUI.UpdateHealthbar();
        soundController.PlayGetHitSound();
        cameraController.Shake(damage);
        ParticleEffectManager.PlayEffect(ParticleEffect.Type.blood, playerMovement.capsColl.bounds.center, knockBackDirection == 1 ? Vector3.left : Vector3.right);
    }

    public void SetItemStats()
    {
        Armor = 0;
        activeMaxHealth = base_maxhealth + level * health_perLevel;
        attack1Dam = 5;
        attack2Dam = 7.5f;
        attack3Dam = 10;
        downwardAttackDam = 7.5f;
        for (int i = 0; i < equipment.slot.Length; i++)
        {
            activeMaxHealth += equipment.slot[i].GetComponent<Slot>().health;
            Armor += equipment.slot[i].GetComponent<Slot>().armor;
            attack1Dam += equipment.slot[i].GetComponent<Slot>().damage;
            attack2Dam += equipment.slot[i].GetComponent<Slot>().damage * 1.5f;
            attack3Dam += equipment.slot[i].GetComponent<Slot>().damage * 2;
            downwardAttackDam += equipment.slot[i].GetComponent<Slot>().damage * 1.5f;
        }
        statusGUI.UpdateInventoryStats();
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
        numberOfDeaths++;
        playerMovement.animator.SetBool("Dead", true);
        StartCoroutine(ReviveAfterTime(2f));
        essence /= 2;
        statusGUI.UpdateEssenceText();
        gamemanager.SaveGame();
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
        gamemanager.ResetLevel(respawnPortalID);
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
        return attack1Dam;
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

    public SaveProfile GetCurrentProfile()
    {
        Vector3 playerHubPos = GetPosInHub();
        SaveProfile p = new SaveProfile(currentProfileID, level, essence, storedEssence, timePlayed, numberOfDeaths, equipment.GetItemIds(), hubChest.GetItemIds(), 
                                        checkpoints, playerHubPos.x, playerHubPos.y, hubUnloked,
                                                         playerMovement.dashUnlocked, playerMovement.midAirDashUnlocked,
                                                         playerMovement.downwardAttackUnlocked, playerMovement.wallJumpingUnlocked,
                                                         playerMovement.maxJumpCount, playerMovement.dashDistance,
                                                         playerMovement.minDelayBetweenDashes, playerMovement.maxMidairDashesCount,
                                                         playerMovement.invincibilityFrameTime);
        return p;
    }
    public void LoadFromProfile(SaveProfile profile, bool overideSavedHubPosition)
    {
        currentProfileID = profile.id;
        timePlayed = profile.id;
        level = profile.lvl;
        essence = profile.essence;
        storedEssence = profile.essenceStored;
        numberOfDeaths = profile.numberOfDeaths;
        lastPosInHub = new Vector3(profile.xPosInHub, profile.yPosInHub, 0);
        hubUnloked = profile.hubUnloked;
        checkpoints = profile.checkpoints;
        numberOfDeaths = profile.numberOfDeaths;
        if (!overideSavedHubPosition)
            transform.position = lastPosInHub;

        playerMovement.dashUnlocked = profile.dashUnlocked;
        playerMovement.midAirDashUnlocked = profile.midAirDashUnlocked;
        playerMovement.downwardAttackUnlocked = profile.downwardAttackUnlocked;
        playerMovement.wallJumpingUnlocked = profile.wallJumpingUnlocked;
        playerMovement.maxJumpCount = profile.maxJumpCount;
        playerMovement.dashDistance = profile.dashDistance;
        playerMovement.minDelayBetweenDashes = profile.minDelayBetweenDashes;
        playerMovement.maxMidairDashesCount = profile.maxMidairDashesCount;
        playerMovement.invincibilityFrameTime = profile.invincibilityFrameTime;
        statusGUI.UpdateEssenceText();
        statusGUI.UpdateHealthbar();
        statusGUI.UpdateInventoryStats();
        statusGUI.UpdateLevelText();
    }

    public Vector3 GetPosInHub()
    {
        if (gamemanager.isCurrLevelHub)
            return transform.position;
        return lastPosInHub;
    }
    public void SavePosAsLastInHub()
    {
        lastPosInHub = transform.position;
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
        cameraController.Shake(1);
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
