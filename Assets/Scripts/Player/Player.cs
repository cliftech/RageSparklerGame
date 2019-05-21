using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private CameraController cameraController;
    private GameManager gamemanager;
    [HideInInspector] public Inventory equipment;
    [HideInInspector] public Inventory hubChest;
    [HideInInspector] public PlayerSoundController soundController;
    [HideInInspector] public PlayerDashParticleController playerDashParticleController;
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public PlayerInteract plrInter;
    [HideInInspector] public TheFirstFlash AmuletFlash;
    [HideInInspector] public StatusGUI statusGUI;
    [HideInInspector] public ParticleSystem footPrintParticles;
    [HideInInspector] public ParticleSystem landParticles;
    [HideInInspector] public ParticleSystem jumpParticles, wallJumpParticles_left, wallJumpParticles_right;

    [HideInInspector] public int level;
    [HideInInspector] public float activeMaxHealth;
    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public float Armor = 0;

    public CraftingWindow craftWindow;

    [HideInInspector] public List<int> checkpoints;
    [HideInInspector] public HubSaveState hubSaveState;
    [HideInInspector] public Dictionary<string, int> enemyKillCount;

    public float attack1Dam = 5;
    public float attack2Dam = 7.5f;
    public float attack3Dam = 10;
    public float downwardAttackDam = 7.5f;
    public int essence;
    public int storedEssence;
    public int priceToLevelUp;

    private float health;
    //private int level;
    private string enemyWeaponTag = "EnemyWeapon";
    private string trapTag = "Trap";
    private Action interactAction;
    private int respawnPortalID;

    [HideInInspector] public bool isDead;
    [HideInInspector] public int currentProfileID;
    [HideInInspector] public float timePlayed;
    [HideInInspector] private int lastHubPortalID;
    [HideInInspector] public int numberOfDeaths;
    [HideInInspector] public bool hubUnloked;

    private bool isDrinkingPotion = false;

    void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        plrInter = FindObjectOfType<PlayerInteract>();
        gamemanager = FindObjectOfType<GameManager>();
        soundController = GetComponentInChildren<PlayerSoundController>();
        playerDashParticleController = GetComponentInChildren<PlayerDashParticleController>();
        playerMovement = GetComponent<PlayerMovement>();
        AmuletFlash = FindObjectOfType<TheFirstFlash>();
        statusGUI = FindObjectOfType<StatusGUI>();
        footPrintParticles = transform.Find("FootprintParticles").GetComponent<ParticleSystem>();
        landParticles = transform.Find("LandParticles").GetComponent<ParticleSystem>();
        jumpParticles = transform.Find("JumpParticles").GetComponent<ParticleSystem>();
        wallJumpParticles_left = transform.Find("WallJumpParticlesLeft").GetComponent<ParticleSystem>();
        wallJumpParticles_right = transform.Find("WallJumpParticlesRight").GetComponent<ParticleSystem>();
        enemyKillCount = new Dictionary<string, int>();

        Inventory[] equipments = GetComponents<Inventory>();
        for (int i = 0; i < equipments.Length; i++)
        {
            if (equipments[i].inventoryUI.name == "EquipmentUI")
                equipment = equipments[i];
            else
                hubChest = equipments[i];
        }
        craftWindow.equipment = equipment;
        craftWindow.hubChest = hubChest;
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
        statusGUI.UpdatePotionCharges();
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact") && !InGameMenu.isMenuShowing())
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
        float damageReduction = 1 - (0.052f * Armor) / (0.9f + 0.048f * Armor);
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
        for (int i = 0; i < equipment.slots.Length; i++)
        {
            if (equipment.slots[i].itemas != null)
            {
                activeMaxHealth += equipment.slots[i].itemas.health;
                Armor += equipment.slots[i].itemas.armor;
                attack1Dam += equipment.slots[i].itemas.damage;
                attack2Dam += equipment.slots[i].itemas.damage * 1.5f;
                attack3Dam += equipment.slots[i].itemas.damage * 2;
                downwardAttackDam += equipment.slots[i].itemas.damage * 1.5f;
            }
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
        StartCoroutine(StopMovingAfterDelay(0.5f));
        essence /= 2;
        statusGUI.UpdateEssenceText();
        gamemanager.SaveGame();
        gamemanager.StartCoroutine(gamemanager.ResetLevel(respawnPortalID, 2f));
    }
    private IEnumerator StopMovingAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        playerMovement.rb.velocity = Vector2.zero;
    }

    public void Revive()
    {
        isDead = false;
        health = activeMaxHealth;
        playerMovement.animator.SetBool("Dead", false);
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
        CalculateLevelUpPrice();
        statusGUI.UpdateLevelText();
    }
    private void CalculateLevelUpPrice()
    {
        priceToLevelUp = (int)((level * level * 1f) / 10f + 20f + 5 * level);
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
        Debug.LogError("Current attack number is not recognized: " + playerMovement.currentAttackNum);
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
        if ((other.CompareTag(enemyWeaponTag) || other.CompareTag("Projectile")) && !isDead && !playerMovement.isInvulnerable)
        {
            GetHit(other.transform.GetComponentInParent<DamageContainer>().GetDamage(),
                other.transform.position.x > transform.position.x ? 1 : -1);
            if (isDrinkingPotion && playerMovement.isDisabled)
            {
                playerMovement.SetEnabled(true);
                isDrinkingPotion = false;
            }
        }

        //if (other.CompareTag("Projectile") && !isDead && !playerMovement.isInvulnerable)
        //{
        //    Projectile p = other.transform.GetComponentInParent<Projectile>();
        //    GetHit(p.GetDamage(),
        //        other.transform.position.x > transform.position.x ? 1 : -1);
        //    if (isDrinkingPotion && playerMovement.isDisabled)
        //    {
        //        playerMovement.SetEnabled(true);
        //        isDrinkingPotion = false;
        //    }
        //}

        if (other.gameObject.CompareTag("Pick Up"))
        {
            Destroy(other.gameObject);
            essence++;
            statusGUI.UpdateEssenceText();
            gamemanager.SaveGame(false);
        }

        if (other.gameObject.CompareTag("Rune"))
        {
            if (other.gameObject.name.StartsWith("DuobleJumpRune") || other.gameObject.name.StartsWith("TripleJumpRune"))
            {
                UpgradeAirJumpCount(playerMovement.maxJumpCount);
            }

            if (other.gameObject.name.StartsWith("DashRune"))
            {
                UnlockDash();
            }

            if (other.gameObject.name.StartsWith("WallClimbingRune"))
            {
                UnlockWallClimbing();
            }

            if (other.gameObject.name.StartsWith("AirDashRune"))
            {
                UnlockMidAirDash();
            }

            if (other.gameObject.name.StartsWith("DownwardAttackRune"))
            {
                UnlockDownwardAttack();
            }

            if (other.gameObject.name.StartsWith("DoubleAirDashRune") || other.gameObject.name.StartsWith("TripleAirDashRune"))
            {
                UpgradeMaxMidAirDashCount(++playerMovement.maxMidairDashesCount);
            }

            if (other.gameObject.name.StartsWith("DashDistanceRune"))
            {
                UpgradeDashDistance(++playerMovement.dashDistance);
            }

            if (other.gameObject.name.StartsWith("NoDashDelayRune"))
            {
                UpgradeDelayBetweenDashed(0);
            }

            if (other.gameObject.name.StartsWith("InvincibilityRune"))
            {
                UpgradeInvincibilityFrame(playerMovement.invincibilityFrameTime + 0.2f);
            }

            Destroy(other.gameObject);
            gamemanager.SaveGame(false);
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
        List<int> inventoryAmounts;
        List<string> inventoryItems = equipment.GetItemIds(out inventoryAmounts);
        List<int> hubChestAmounts;
        List<string> hubChestItems = hubChest.GetItemIds(out hubChestAmounts);
        SaveProfile p = new SaveProfile(currentProfileID, level, essence, storedEssence, timePlayed, numberOfDeaths, inventoryItems, inventoryAmounts, hubChestItems, hubChestAmounts,
                                        enemyKillCount, checkpoints, lastHubPortalID, hubUnloked,
                                                         playerMovement.dashUnlocked, playerMovement.midAirDashUnlocked,
                                                         playerMovement.downwardAttackUnlocked, playerMovement.wallJumpingUnlocked,
                                                         playerMovement.maxJumpCount, playerMovement.dashDistance,
                                                         playerMovement.minDelayBetweenDashes, playerMovement.maxMidairDashesCount,
                                                         playerMovement.invincibilityFrameTime, hubSaveState);
        return p;
    }
    public void LoadFromProfile(SaveProfile profile)
    {
        currentProfileID = profile.id;
        timePlayed = profile.timePlayed;
        level = profile.lvl;
        essence = profile.essence;
        storedEssence = profile.essenceStored;
        numberOfDeaths = profile.numberOfDeaths;
        lastHubPortalID = profile.lastHubPortalID;
        hubUnloked = profile.hubUnloked;
        enemyKillCount = profile.enemyKillCount != null ? profile.enemyKillCount : new Dictionary<string, int>();
        checkpoints = profile.checkpoints;
        numberOfDeaths = profile.numberOfDeaths;

        playerMovement.dashUnlocked = profile.dashUnlocked;
        playerMovement.midAirDashUnlocked = profile.midAirDashUnlocked;
        playerMovement.downwardAttackUnlocked = profile.downwardAttackUnlocked;
        playerMovement.wallJumpingUnlocked = profile.wallJumpingUnlocked;
        playerMovement.maxJumpCount = profile.maxJumpCount;
        playerMovement.dashDistance = profile.dashDistance;
        playerMovement.minDelayBetweenDashes = profile.minDelayBetweenDashes;
        playerMovement.maxMidairDashesCount = profile.maxMidairDashesCount;
        playerMovement.invincibilityFrameTime = profile.invincibilityFrameTime;

        hubSaveState = profile.hubSaveState;
        SetItemStats();
        CalculateLevelUpPrice();
        statusGUI.UpdateEssenceText();
        statusGUI.UpdateHealthbar();
        statusGUI.UpdateInventoryStats();
        statusGUI.UpdateLevelText();

        // cia tik pavyzdys kaip gauti kill count, gali istrint ar uzkomentuoti
        //print("EnemyKillCounts: ");
        //print("Executioner: " + GetEnemyKillCount(typeof(AI_Executioner)));
        //print("male naga: " + GetEnemyKillCount(typeof(AI_MaleNaga)));
        //print("male naga enraged: " + GetEnemyKillCount(typeof(AI_MaleNagaEnraged)));
        //print("female naga: " + GetEnemyKillCount(typeof(AI_FemaleNaga)));
        //print("female naga enraged: " + GetEnemyKillCount(typeof(AI_FemaleNagaEnraged)));
        //print("fire golem: " + GetEnemyKillCount(typeof(AI_FireGolem)));
        //print("ghoul: " + GetEnemyKillCount(typeof(AI_Ghoul)));
        //print("imp: " + GetEnemyKillCount(typeof(AI_Imp)));
        //print("necromancer: " + GetEnemyKillCount(typeof(AI_Necromancer)));
        //print("slug: " + GetEnemyKillCount(typeof(AI_Slug)));
    }

    public int GetlastHubPortalID()
    {
        return lastHubPortalID;
    }
    public void SaveLastHubPortalID(int portalID)
    {
        lastHubPortalID = portalID;
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

    public void UnlockWallClimbing()
    {
        playerMovement.wallJumpingUnlocked = true;
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

    public int GetEnemyKillCount(Type enemyType)
    {
        int count;
        if (enemyKillCount.TryGetValue(enemyType.Name, out count))
            return count;
        return 0;
    }
    public void AddEnemyKilldedToCount(Type type)
    {
        if (enemyKillCount.ContainsKey(type.Name))
        {
            enemyKillCount[type.Name]++;
        }
        else
        {
            enemyKillCount.Add(type.Name, 1);
        }
        gamemanager.SaveGame();
    }

    public void FootstepEffectEvent()
    {
        soundController.PlayFootstepSound();
        footPrintParticles.Play();
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
    public void PlayJumpEffect()
    {
        soundController.PlayJumpSound();
        if (playerMovement.isGrounded)
        {
            jumpParticles.Play();
            landParticles.Play();
        }
        else
        {
            if (playerMovement.isStuckToWall_L)
                wallJumpParticles_right.Play();
            else if (playerMovement.isStuckToWall_R)
                wallJumpParticles_left.Play();
            else
                jumpParticles.Play();

        }
    }

    public void UsePotionStart()
    {
        if (!playerMovement.isKnockedBack && !playerMovement.isDisabled && playerMovement.isGrounded && !playerMovement.isStuckToWall_L && 
            !playerMovement.isStuckToWall_R && !playerMovement.isDownwardAttacking && !playerMovement.isAttacking && !playerMovement.isDashing &&
            Health < activeMaxHealth)
        {
            isDrinkingPotion = true;
            playerMovement.SetEnabled(false);
            playerMovement.animator.SetTrigger("UseItem");
        }
    }

    public void UsePotionActivation()
    {
        Item potion = equipment.GetPotion();
        if (potion != null && potion.currentUses > 0)
        {
            potion.currentUses--;
            float heal = potion.healPercent;
            AddHealth(heal);
            soundController.PlayPotionUseSound();
            UpdatePotionSprite(potion);
            statusGUI.UpdatePotionCharges();
            gamemanager.SaveGame(true);
        }
        playerMovement.SetEnabled(true);
        isDrinkingPotion = false;
    }

    public void UpdatePotionSprite(Item potion)
    {
        if (potion.currentUses == 0)
        {
            equipment.slots[8].slotIcon.GetComponent<Image>().sprite = potion.Empty;
            potion.icon = potion.Empty;
        }
        if (potion.currentUses > potion.maxUses * 2 / 3)
        {
            equipment.slots[8].slotIcon.GetComponent<Image>().sprite = potion.Full;
            potion.icon = potion.Full;
        }
        if (potion.currentUses > 0 && potion.currentUses <= potion.maxUses * 1 / 3)
        {
            equipment.slots[8].slotIcon.GetComponent<Image>().sprite = potion.OneThird;
            potion.icon = potion.OneThird;
        }
        if (potion.currentUses > potion.maxUses * 1 / 3 && potion.currentUses <= potion.maxUses * 2 / 3)
        {
            equipment.slots[8].slotIcon.GetComponent<Image>().sprite = potion.TwoThirds;
            potion.icon = potion.TwoThirds;
        }
    }
}
