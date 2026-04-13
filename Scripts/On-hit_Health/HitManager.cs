using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HitManager : MonoBehaviour
{
    protected Callback<UserStatsReceived_t> UserStatsReceivedCallback;
    [SerializeField] private bool isPlayer = false;
    public Rigidbody2D rb;
    public int maxHealth;
    public float health;

    public bool destroyable;
    public bool dies;

    public float flashDuration;
    public int iframes;

    public float knockbackRatio;
    public int regenerationRate;

    public Animator anim;

    public bool dead;

    int regen = 0;
    int flash = 0;
    public int ifra = 0;

    public int healthScalingPerRoomAdd = 0;
    public float healthScalingPerRoomMult = 0f;

    public bool scalingHealth = true;

    [Header("Take hits from")]
    public bool enemyProjectileHits;
    public bool playerProjectileHits;
    public bool neutralProjectileHits;
    public bool enemyBodyHits;
    public bool playerBodyHits;

    [Header("Get vaporized by")]
    public bool enemyProjectileVaporizes;
    public bool playerProjectileVaporizes;
    public bool neutralProjectileVaporizes;
    public bool enemyBodyVaporizes;
    public bool playerBodyVaporizes;

    [Header("Flags")]
    public bool noStatusEffects;


    private RoomManager roomManager;

    private Vector2 kbForce;

    [SerializeField] private AIPath aiPath;

    [SerializeField] private float burnStacks = 0;
    [SerializeField] private float poisonStacks = 0;
    [SerializeField] private float frostStacks = 0;

    private int elementalTickRate = 15;

    private float startMaxSpeed = 0f;

    [SerializeField] private bool showDamageNumber = true;

    [SerializeField] private GameObject damageNumberPrefab;

    [SerializeField] private PlayerUpgradesManager upgradeManager;
    [SerializeField] private StatsAndAchievements statsAndAchievements;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip hitNoise;

    public List<GameObject> hitRecently;
    [SerializeField] private float minSecondsBetweenHitBySameObject = 0.3f; // 0.3s

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private PlayerController playerController;

    public bool isBoss = false;

    [Header("OverlaysAndEmitters")]
    [SerializeField] private GameObject gasEmitterObject;
    [SerializeField] private GameObject flameOverlayObject;
    [SerializeField] private GameObject frostOverlayObject;
    [SerializeField] private GameObject poisonOverlayObject;

    private int elementalTick = 0;
    private int consecutiveHits = 0;
    private int consecutiveCritHits = 0;

    private bool awakened = false;

    private bool shouldApplyBurn = false;
    private bool shouldApplyPoison = false;
    private bool shouldApplyFrost = false;

    void Awake()
    {
        GameObject player = GameObject.Find("char");
        upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
        statsAndAchievements = FindFirstObjectByType<StatsAndAchievements>();
        playerController = FindFirstObjectByType<PlayerController>();

        if (scalingHealth)
        {
            GameObject roomHolder = GameObject.Find("roomFloorType1");
            if (roomHolder) roomManager = roomHolder.GetComponent<RoomManager>();
            if (roomManager)
            {
                maxHealth = (int)(maxHealth * Mathf.Pow(healthScalingPerRoomMult, roomManager.currentRoomNum + roomManager.currentBiomeNum * 10));
                maxHealth = maxHealth + (roomManager.currentRoomNum + roomManager.currentBiomeNum * 10) * healthScalingPerRoomAdd;
            }
        }

        if (upgradeManager.bugsprayPercent > 0 && playerProjectileHits)
        {
            maxHealth = (int)(maxHealth * (1 - upgradeManager.bugsprayPercent));
        }

        health = maxHealth;
        if (aiPath) startMaxSpeed = aiPath.maxSpeed;

        awakened = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!awakened) return;
        if (!isActiveAndEnabled) return;

        TestProjectileHitsAndVaporize(col);
        TestBodyHitsAndVaporize(col);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!awakened) return;
        if (!isActiveAndEnabled) return;

        TestProjectileHitsAndVaporize(col);
        TestBodyHitsAndVaporize(col);
    }

    void TestProjectileHitsAndVaporize(Collider2D col)
    {
        // Hit
        if (col.gameObject == gasEmitterObject) return;

        if (enemyProjectileHits && (col.gameObject.CompareTag("EnemyProjectile") || col.gameObject.CompareTag("EnemyProjectileAndObstacle")))
        {
            TriggerHit(col);
        }

        if (playerProjectileHits && col.gameObject.CompareTag("AllyProjectile"))
        {
            TriggerHit(col);
        }

        if (neutralProjectileHits && col.gameObject.CompareTag("NeutralProjectile"))
        {
            TriggerHit(col);
        }


        // Vaporize

        if (enemyProjectileVaporizes && (col.gameObject.CompareTag("EnemyProjectile") || col.gameObject.CompareTag("EnemyProjectileAndObstacle")))
        {
            Destroy(gameObject);
        }

        if (playerProjectileVaporizes && col.gameObject.CompareTag("AllyProjectile"))
        {
            Destroy(gameObject);
        }

        if (neutralProjectileVaporizes && col.gameObject.CompareTag("NeutralProjectile"))
        {
            Destroy(gameObject);
        }
    }

    void TestBodyHitsAndVaporize(Collider2D col)
    {
        // Hit

        if (enemyBodyHits && col.gameObject.tag == "EnemyBody")
        {
            TriggerHit(col);
        }

        if (playerBodyHits && col.gameObject.tag == "Player")
        {
            TriggerHit(col);
        }


        // Vaporize

        if (enemyBodyVaporizes && col.gameObject.tag == "EnemyBody")
        {
            Destroy(gameObject);
        }

        if (playerBodyVaporizes && col.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }

    void SpawnDamageNumber(Vector2 pos, float takenDamage, Color setColor)
    {
        if (!showDamageNumber) return;

        GameObject damageNumberInstance = Instantiate(damageNumberPrefab, pos, Quaternion.identity);
        TextMeshProUGUI dmgNumberText = damageNumberInstance.GetComponentInChildren<TextMeshProUGUI>();

        dmgNumberText.text = Mathf.RoundToInt(takenDamage * 10f).ToString();
        dmgNumberText.fontSize = Mathf.Clamp(Mathf.Sqrt(takenDamage * 100f), 0f, 48f);
        dmgNumberText.color = setColor;

        statsAndAchievements.UpdateMaxSingleHitDmg(takenDamage);
    }

    void TriggerHit(Collider2D col)
    {
        if (ifra > 0 || dead)
            return;

        if (upgradeManager.resetConsecutiveHits) consecutiveHits = 0;

        if (col.attachedRigidbody)
        {
            if (hitRecently.Contains(col.attachedRigidbody.gameObject)) return;

            hitRecently.Add(col.attachedRigidbody.gameObject);
            StartCoroutine(RemoveOldestHitRecordDelayed(col.attachedRigidbody.gameObject));
        }

        float takenDamage = 0;
        col.TryGetComponent(out Damage damageComponent);
        col.TryGetComponent(out ProjectileController projectileController);

        if (damageComponent)
        {
            Color normalColor = new(0.9f, 1f, 1f);
            Color critColor = new(0.9f, 0.1f, 0.1f);

            Color dmgColor = normalColor;

            takenDamage = damageComponent.damage;

            if (isPlayer)
            {
                float moveSpeed = playerController.moveSpeed + upgradeManager.playerMoveSpeedModifier;
                if (upgradeManager.inertiaAcceleration > 0f) moveSpeed *= Mathf.Min(playerController.movingTimer / 50f, 2f);

                takenDamage *= Mathf.Max(0.1f, 1 - upgradeManager.avoidanceDefense * moveSpeed);

                upgradeManager.gotDamagedNextShotBoost = true;
            }

            if (playerProjectileHits || playerBodyHits)
            {
                if (Random.Range(0f, 1f) < upgradeManager.vaporizingShotChance && !isBoss)
                    takenDamage = health;

                upgradeManager.totalDamageDealt += takenDamage;
                upgradeManager.totalHitsDelivered += 1;

                upgradeManager.last20Hits.Add(takenDamage);
            }

            if (playerProjectileHits)
            {
                if (shouldApplyBurn && projectileController.isLightning)
                {
                    takenDamage += upgradeManager.conductiveFlamesDmgModifierFlat;
                    takenDamage *= upgradeManager.conductiveFlamesDmgModifierMult;
                }

                consecutiveHits++;
                takenDamage += upgradeManager.consecutiveHitDamageModifierFlat * consecutiveHits;
                takenDamage *= Mathf.Pow(upgradeManager.consecutiveHitDamageModifierMult, consecutiveHits);

                bool critted = false;
                if (projectileController && projectileController.canCrit)
                {
                    if (Random.Range(0f, 1f) < projectileController.critChance) critted = true;
                    else critted = false;

                    if (upgradeManager.apRound && consecutiveHits > 1) critted = true;

                    if (critted) consecutiveCritHits++;
                    else
                    {
                        upgradeManager.resetConsecutiveCritHits = true;
                        consecutiveCritHits = 0;
                    }
                }

                takenDamage += upgradeManager.consecutiveCriticalHitDamageModifierFlat * consecutiveCritHits;
                takenDamage *= Mathf.Pow(upgradeManager.consecutiveCriticalHitDamageModifierMult, consecutiveCritHits);

                if (upgradeManager.gotDamagedNextShotBoost)
                {
                    if (upgradeManager.bloodletting) takenDamage *= 2;
                    if (upgradeManager.pressurizedNeedle) critted = true;

                    upgradeManager.gotDamagedNextShotBoost = false;
                }

                if (upgradeManager.randomizeDmgPercent > 0f)
                {
                    takenDamage = takenDamage + Random.Range(takenDamage * -upgradeManager.randomizeDmgPercent * upgradeManager.gamblerBombMult,
                                                            takenDamage * upgradeManager.randomizeDmgPercent * upgradeManager.gamblerBombMult);
                }

                if (critted)
                {
                    takenDamage *= upgradeManager.criticalHitDamageMultiplier;
                    dmgColor = critColor;
                }
            }

            health -= takenDamage;

            if (playerProjectileHits)
            {
                if (health < 0)
                {
                    damageComponent.damage += upgradeManager.onKillDmgModifierFlat;
                    damageComponent.damage *= upgradeManager.onKillDmgModifierMult;

                    if (projectileController)
                    {
                        projectileController.attainedDamage += upgradeManager.onKillDmgModifierFlat;
                        projectileController.attainedDamage *= upgradeManager.onKillDmgModifierMult;
                    }

                    transform.localScale *= upgradeManager.onKillSizeModifierMult;
                }
            }

            SpawnDamageNumber(col.ClosestPoint(transform.position), takenDamage, dmgColor);
        }

        if (projectileController)
        {
            burnStacks += projectileController.burnStacksToApply;
            poisonStacks += projectileController.poisonStacksToApply;
            frostStacks += projectileController.frostStacksToApply;
        }

        if (health <= 0 && !dead)
        {
            PerformDeathOperations();
        }

        if (takenDamage > 0)
        {
            ifra = iframes;
            if (isPlayer) rb.excludeLayers |= 1 << LayerMask.NameToLayer("EnemyLayer");
            if (flashDuration > 0) HitAnimation();
        }

        if (knockbackRatio != 0)
            Knockback(col);
    }

    void HitAnimation()
    {
        flash = 0;
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in sprites)
        {
            Material material = sprite.material;
            material.SetFloat("_OnHit", 0.01f);
        }

        if (audioSource) audioSource.PlayOneShot(hitNoise);
    }

    void UnFlash()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in sprites)
        {
            Material material = sprite.material;
            material.SetFloat("_OnHit", 0f);
        }
    }

    void Knockback(Collider2D attackerCol)
    {
        if (attackerCol.GetComponent<KnockbackForce>())
        {
            Vector2 force = attackerCol.GetComponent<KnockbackForce>().knockbackForce * knockbackRatio * (transform.position - attackerCol.transform.position).normalized;
            if (aiPath != null) kbForce += force;
            else rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (ifra > 0)
        {
            ifra--;
        }
        else if (isPlayer)
        {
            rb.excludeLayers &= ~(1 << LayerMask.NameToLayer("EnemyLayer"));
        }

        if (flash <= 1000)
            flash++;

        elementalTick++;


        if (!noStatusEffects && !dead)
        {
            if (burnStacks > 50) shouldApplyBurn = true;
            else shouldApplyBurn = false;

            if (poisonStacks > 50) shouldApplyPoison = true;
            else shouldApplyPoison = false;

            if (frostStacks > 0) shouldApplyFrost = true;
            else shouldApplyFrost = false;

            if (upgradeManager.gaseousCloud > 0)
            {
                ParticleSystem[] gasParticleSystems = gasEmitterObject.GetComponentsInChildren<ParticleSystem>();
                Emitter emitterScript = gasEmitterObject.GetComponent<Emitter>();

                if (shouldApplyPoison)
                {
                    if (emitterScript.stopped) emitterScript.StartEmitting();
                }
                else
                {
                    if (!emitterScript.stopped) emitterScript.StopEmitting();
                }

                foreach (ParticleSystem gasParticleSystem in gasParticleSystems)
                {
                    ParticleSystem.MainModule gasParticleSystemMain = gasParticleSystem.main;

                    GameObject gasParticleSystemObject = gasParticleSystem.gameObject;
                    Light2D gasLight2D = gasParticleSystemObject.GetComponent<Light2D>();
                    CircleCollider2D gasCollider = gasParticleSystemObject.GetComponent<CircleCollider2D>();
                    ProjectileController gasProjController = gasParticleSystemObject.GetComponent<ProjectileController>();

                    if (shouldApplyPoison)
                    {
                        // Also this poison effect is triggered every 0.3 seconds (via hitRecently), adding gaseous cloud times 10% of poison stacks per second
                        // Also add a storage for default multipliers and stuff
                        gasParticleSystemObject.transform.localScale = new(1f, 0.8f, 1f);

                        gasParticleSystemMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 1f, 0f, 0.1f));
                        gasParticleSystemMain.startSpeed = 3f;

                        gasCollider.radius = 4f;

                        gasLight2D.intensity = 5f;
                        gasLight2D.color = new Color(0f, 1f, 0f, 1f);

                        gasProjController.poisonStacksToApply = (int)Mathf.Max(1f, poisonStacks * upgradeManager.gaseousCloud * 0.01f / 3.333f);
                        if (gasParticleSystem.isStopped) gasParticleSystem.Play();
                    }
                    else
                    {
                        gasLight2D.intensity = 0f;
                        gasProjController.poisonStacksToApply = 0;
                        if (!gasParticleSystem.isStopped) gasParticleSystem.Stop();
                    }

                    if (shouldApplyPoison && shouldApplyBurn)
                    {
                        gasParticleSystemMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.4f, 0f, 0.2f));
                        gasParticleSystemMain.startSpeed = 4.5f;

                        gasCollider.radius = 4f * 1.2f;

                        gasLight2D.intensity = 10f;
                        gasLight2D.color = new Color(1f, 0.2f, 0f, 1f);

                        gasProjController.burnStacksToApply = (int)Mathf.Max(1f, burnStacks * upgradeManager.gaseousCloud * 0.01f / 3.333f);
                    }
                    else
                    {
                        gasProjController.burnStacksToApply = 0;
                    }

                    gasParticleSystemObject.transform.SetParent(null);
                }
            }


            //////////
            // BURN //
            //////////
            if (shouldApplyBurn)
            {
                flameOverlayObject.SetActive(true);
                if (elementalTick % elementalTickRate == 0)
                {
                    float takenDamage = Mathf.Max(1f, burnStacks * 0.005f);
                    health -= takenDamage;

                    upgradeManager.totalDamageDealt += takenDamage;

                    HitAnimation();

                    SpawnDamageNumber(transform.position, takenDamage, new Color(0.9f, 0.5f, 0.1f));
                }
            }
            else flameOverlayObject.SetActive(false);


            ////////////
            // POISON //
            ////////////
            if (shouldApplyPoison)
            {
                poisonOverlayObject.SetActive(true); // WHY ARE YOU FINDING THESE EVERY FRAME?
                if (elementalTick % elementalTickRate == 0)
                {
                    float takenDamage = Mathf.Max(1f, Mathf.Sqrt(poisonStacks) / 10f);
                    health -= takenDamage;

                    upgradeManager.totalDamageDealt += takenDamage;

                    HitAnimation();

                    SpawnDamageNumber(transform.position, takenDamage, new Color(0.1f, 0.6f, 0.1f));
                }
            }
            else poisonOverlayObject.SetActive(false);


            ///////////
            // FROST //
            ///////////
            if (shouldApplyFrost)
            {
                frostOverlayObject.SetActive(true);
                if (aiPath) aiPath.maxSpeed = startMaxSpeed * 1000f / (frostStacks + 1000f);
            }
            else
            {
                frostOverlayObject.SetActive(false);
                if (aiPath) aiPath.maxSpeed = startMaxSpeed;
            }

            burnStacks -= burnStacks / 2 / 50; // Lose half the stacks every second
            poisonStacks -= poisonStacks / 2 / 50; // Lose half the stacks every second
            if (frostStacks > 0) frostStacks--;
        }

        if (health <= 0 && !dead)
        {
            PerformDeathOperations();
        }

        if (flash <= flashDuration && flash > 0)
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in sprites)
            {
                Material material = sprite.material;
                material.SetFloat("_OnHit", flash / flashDuration * 0.9f);
            }
        }

        if (flash > flashDuration)
        {
            UnFlash();
        }

        if (regenerationRate != 0)
        {
            regen++;
            if (regen > 50f / regenerationRate)
            {
                health++;
                regen = 0;
            }
        }

        if (kbForce.magnitude > 0 && aiPath != null)
        {
            aiPath.Move(kbForce * Time.fixedDeltaTime);
            kbForce *= 0.9f;
        }
    }

    void PerformDeathOperations()
    {
        if (destroyable)
            Destroy(gameObject);

        if (dies)
        {
            anim.SetBool("die", true);

            rb.excludeLayers = ~0; // Disable collision

            if (upgradeManager.carrionBlast)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                explosion.transform.localScale = 0.4f * upgradeManager.aoeSizeModifierMult * Vector3.one;
                explosion.GetComponent<Damage>().damage = 8;

                if (upgradeManager.explosionsAlwaysCrit)
                    explosion.GetComponent<Damage>().damage *= upgradeManager.criticalHitDamageMultiplier;
            }

            if (!noStatusEffects)
            {
                flameOverlayObject.SetActive(false);
                poisonOverlayObject.SetActive(false);
                frostOverlayObject.SetActive(false);
            }
        }

        dead = true;

        if (gameObject.TryGetComponent(out Worms worms))
            worms.dropWorms();

        if (gameObject.TryGetComponent(out DropGreenGrubs greenGrubs))
            greenGrubs.dropGreenGrubs();
    }

    IEnumerator RemoveOldestHitRecordDelayed(GameObject obj)
    {
        yield return new WaitForSeconds(minSecondsBetweenHitBySameObject);

        hitRecently.Remove(obj);
    }
}
