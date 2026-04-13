using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class ProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject thisBulletPrefab;
    public Rigidbody2D rb;
    public float speed;
    public float duration;
    public int destroyAfterHits;
    public PlayerUpgradesManager upgradeManager;
    public GameObject explosionPrefab;
    public PlayerController playerController;

    [SerializeField] private float gravity;
    [SerializeField] private ChainEffect chainEffect;

    int tick = 0;
    public int hits = 0;

    [SerializeField] private List<AudioClip> soundEffects;
    [SerializeField] private float wiggleAngle = 0f;
    private float wiggleFrequency = 0.04f;
    private float orbitRotationSpeed = 10f;
    private GameObject player;
    private bool startOrbit = false;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private int remainingBounces = 0;

    public int burnStacksToApply = 0;
    public int poisonStacksToApply = 0;
    public int frostStacksToApply = 0;

    public bool canRotate = true;

    public bool areaOfEffect = false;

    public bool obstacleDestroys = true;

    public List<GameObject> hitRecently;

    public bool critted = false;

    [SerializeField] public bool canCrit = true;
    [SerializeField] private bool canSplinter = true;
    [SerializeField] private bool canExplode = true;
    [SerializeField] private bool canApplyStatus = true;
    [SerializeField] private bool getsStatusWithElempyre = false;

    [SerializeField] public bool canPierce = true;
    [SerializeField] private bool doesShowElementalOverlay = true;
    [SerializeField] private bool canTriggerChainLightning = true;
    [SerializeField] private bool canTriggerWithEHammer = false;

    private int pulseTick = 0;
    private float minPulseScale;
    private float maxPulseScale;

    public float startDamage;
    public Vector3 startSize;

    public float critChance = 0f;

    public bool isDrone = false;

    public bool awakened = false;
    public float attainedDamage = 0f;

    public bool isLightning = false;



    void Awake()
    {
        InitializeComponents();

        if (CompareTag("AllyProjectile"))
        {
            InitializeAllyProjectile();
        }

        PlayStartSound();
        awakened = true;
    }

    private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.Find("char");
        upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void InitializeAllyProjectile()
    {
        InitializePulseScaling();
        ApplyDamageModifiers();
        ApplyCriticalHitChance();
        ApplyKnockbackModifiers();
        ApplyScaleModifiers();
        InitializeBounces();
        InitializeStatusEffects();
    }

    private void InitializePulseScaling()
    {
        minPulseScale = 1f / (1.04f * upgradeManager.pulsingShot);
        maxPulseScale = 1f * (1.06f * upgradeManager.pulsingShot);
    }

    private void ApplyDamageModifiers()
    {
        float realMoveSpeed = playerController.moveSpeed + upgradeManager.playerMoveSpeedModifier;
        float speedDMGbonus = realMoveSpeed * upgradeManager.speedDMGCoeff;
        float wormsDMGbonus = player.GetComponent<PlayerBankManager>().worms * upgradeManager.wormDMGCoeff;

        TryGetComponent(out Damage dmgComponent);
        if (dmgComponent)
        {
            float upgradedDamageBase = (dmgComponent.damage + upgradeManager.dmgModifierFlat + speedDMGbonus + wormsDMGbonus) * upgradeManager.dmgModifierMult;
            float damageJiggleRange = Mathf.Sqrt(upgradedDamageBase) / 5f;
            float damageJiggle = Random.Range(-damageJiggleRange, damageJiggleRange);

            dmgComponent.damage = upgradedDamageBase + damageJiggle;
            if (playerController.haltActive < 6 * 50) dmgComponent.damage *= upgradeManager.haltDamageMult;

            startDamage = dmgComponent.damage;
        }
    }

    private void ApplyCriticalHitChance()
    {
        critChance = upgradeManager.criticalHitChance;
    }

    private void ApplyKnockbackModifiers()
    {
        TryGetComponent(out KnockbackForce kbForceComp);
        if (kbForceComp)
        {
            float kbfV = kbForceComp.knockbackForce;
            kbForceComp.knockbackForce = (kbfV + upgradeManager.knockbackModifierFlat) * upgradeManager.knockbackModifierMult;
        }
    }

    private void ApplyScaleModifiers()
    {
        if (!areaOfEffect)
        {
            transform.localScale = transform.localScale * upgradeManager.bulletSizeModifierMult;
            if (isDrone) transform.localScale = transform.localScale * upgradeManager.droneBulletSizeModifierMult;
        }
        else
        {
            transform.localScale = transform.localScale * upgradeManager.aoeSizeModifierMult;
        }
    }

    private void InitializeBounces()
    {
        remainingBounces = upgradeManager.bouncinessCount;
    }

    private void InitializeStatusEffects()
    {
        if (getsStatusWithElempyre && upgradeManager.elempyre)
        {
            canApplyStatus = true;
        }

        if (canApplyStatus && doesShowElementalOverlay)
        {
            if (upgradeManager.elempyre)
            {
                ApplyElempyreStatusEffects();
            }
            else
            {
                ApplyRandomStatusEffects();
            }
        }
    }

    private void ApplyElempyreStatusEffects()
    {
        if (upgradeManager.burnStacksPerShot > 0)
        {
            transform.Find("FlameOverlay").gameObject.SetActive(true);
            burnStacksToApply = (int)(upgradeManager.burnStacksPerShot * 0.5f);
        }

        if (upgradeManager.poisonStacksPerShot > 0)
        {
            transform.Find("PoisonOverlay").gameObject.SetActive(true);
            poisonStacksToApply = (int)(upgradeManager.poisonStacksPerShot * 0.5f);
        }

        if (upgradeManager.frostStacksPerShot > 0)
        {
            transform.Find("FrostOverlay").gameObject.SetActive(true);
            frostStacksToApply = (int)(upgradeManager.frostStacksPerShot * 0.5f);
        }
    }

    private void ApplyRandomStatusEffects()
    {
        if (Random.Range(0f, 1f) < upgradeManager.burnChance && upgradeManager.burnStacksPerShot > 0)
        {
            transform.Find("FlameOverlay").gameObject.SetActive(true);
            burnStacksToApply = upgradeManager.burnStacksPerShot;
        }

        if (Random.Range(0f, 1f) < upgradeManager.poisonChance && upgradeManager.poisonStacksPerShot > 0)
        {
            transform.Find("PoisonOverlay").gameObject.SetActive(true);
            poisonStacksToApply = upgradeManager.poisonStacksPerShot;
        }

        if (Random.Range(0f, 1f) < upgradeManager.frostChance && upgradeManager.frostStacksPerShot > 0)
        {
            transform.Find("FrostOverlay").gameObject.SetActive(true);
            frostStacksToApply = upgradeManager.frostStacksPerShot;
        }
    }

    private void PlayStartSound()
    {
        int randomSoundEffectIdx = Random.Range(0, soundEffects.Count);
        if (soundEffects.Count > 0) audioSource.PlayOneShot(soundEffects[randomSoundEffectIdx]);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!awakened) return;

        tick++;

        if (CompareTag("AllyProjectile"))
        {
            UpdateAllyProjectile();
        }

        UpdateProjectileMovement();
    }

    private void UpdateAllyProjectile()
    {
        Damage dmgComponent = gameObject.GetComponentInChildren<Damage>();

        ApplyInflation();
        ApplyOrbitBehavior();
        ApplyWiggleBehavior();
        ApplyPulseBehavior(dmgComponent);
    }

    private void ApplyInflation()
    {
        if (upgradeManager.inflation > 0)
            transform.localScale *= 1 + 0.01f * upgradeManager.inflation;
    }

    private void ApplyOrbitBehavior()
    {
        if (upgradeManager.shotsOrbit && canRotate)
        {
            float directionFromOrbit = upgradeManager.orbitRadius - (player.transform.position - transform.position).magnitude;
            float absDistFromOrbit = Mathf.Abs(directionFromOrbit);

            Vector3 directionToPlayer = player.transform.position - transform.position;
            float rightFacingPlayer = Vector3.Dot(transform.right, directionToPlayer.normalized);

            if (directionFromOrbit < 0f)
            {
                if (rightFacingPlayer < 0f) transform.Rotate(0f, 0f, orbitRotationSpeed * Mathf.Max(1f, Mathf.Min(absDistFromOrbit, 3f)));
                startOrbit = true;
            }

            if (startOrbit && directionFromOrbit > 0f)
            {
                if (rightFacingPlayer > 0f) transform.Rotate(0f, 0f, -orbitRotationSpeed * Mathf.Max(1f, Mathf.Min(absDistFromOrbit, 3f)));
            }
        }
    }

    private void ApplyWiggleBehavior()
    {
        if (upgradeManager.wiggly > 0f && canRotate)
        {
            transform.Rotate(0f, 0f, upgradeManager.wiggly * Mathf.Cos(wiggleAngle));
            wiggleAngle += wiggleFrequency * GetUpgradedSpeed();
        }
    }

    private void ApplyPulseBehavior(Damage dmgComponent)
    {
        if (upgradeManager.pulsingShot > 0 && dmgComponent)
        {
            pulseTick += 1;
            if (pulseTick < 5)
            {
                dmgComponent.damage = startDamage + attainedDamage + (pulseTick / 5f);
                float newLocalScale = 1f + 0.1f * upgradeManager.pulsingShot;
                transform.localScale *= newLocalScale;
            }
            else
            {
                dmgComponent.damage = startDamage + attainedDamage + (1 - pulseTick / 5f);
                float newLocalScale = 1f - 0.1f * upgradeManager.pulsingShot;
                transform.localScale *= newLocalScale;
            }
            pulseTick %= 10;
        }
    }

    private void UpdateProjectileMovement()
    {
        if (rb)
        {
            float upgradedSpeed = GetUpgradedSpeed();
            float upgradedAcceleration = GetUpgradedAcceleration();
            float upgradedDuration = GetUpgradedDuration();

            Vector2 movement = transform.right;
            rb.velocity = movement * (upgradedSpeed + upgradedAcceleration * tick);

            CheckGhostKnifeSplinter(upgradedDuration);
            CheckProjectileLifetime(upgradedDuration);
        }
    }

    private float GetUpgradedSpeed()
    {
        float upgradedSpeed = speed;

        if (CompareTag("AllyProjectile"))
        {
            if (upgradeManager.acceleratingShotStartSpeed > 0)
                upgradedSpeed = upgradeManager.acceleratingShotStartSpeed;

            upgradedSpeed = (upgradedSpeed + upgradeManager.bulletSpeedModifierFlat) * upgradeManager.bulletSpeedModifierMult;
            upgradedSpeed = Mathf.Max(0f, upgradedSpeed); // Bullets don't go backwards
        }

        return upgradedSpeed;
    }

    private float GetUpgradedAcceleration()
    {
        float upgradedAcceleration = 0f;

        if (CompareTag("AllyProjectile"))
        {
            if (upgradeManager.acceleratingShotAccelRate > 0)
                upgradedAcceleration = upgradeManager.acceleratingShotAccelRate;
        }

        return upgradedAcceleration;
    }

    private float GetUpgradedDuration()
    {
        float upgradedDuration = duration;

        if (CompareTag("AllyProjectile"))
        {
            upgradedDuration = (upgradedDuration + upgradeManager.bulletLifetimeModifierFlat) * upgradeManager.bulletLifetimeModifierMult;
            upgradedDuration = Mathf.Max(0f, upgradedDuration); // No negative duration
        }

        return upgradedDuration;
    }

    private void CheckGhostKnifeSplinter(float upgradedDuration)
    {
        if (upgradeManager.ghostKnife && canSplinter && upgradedDuration > 0f)
        {
            if (CompareTag("AllyProjectile") && tick > upgradedDuration * 0.2f)
            {
                DoSplinter();
            }
        }
    }

    private void CheckProjectileLifetime(float upgradedDuration)
    {
        if (tick > upgradedDuration && upgradedDuration > 0f)
            Destroy(rb.gameObject, 0.01f);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!awakened) return;

        if (destroyAfterHits > 0 && col.GetComponentInParent<HitManager>())
        {
            HitManager hitManager = col.GetComponentInParent<HitManager>();
            if (hitManager.ifra > 0) return;

            GameObject hitObject;
            if (col.attachedRigidbody) hitObject = col.attachedRigidbody.gameObject;
            else hitObject = col.gameObject;

            if (hitManager.playerProjectileHits && gameObject.CompareTag("AllyProjectile"))
            {
                Hit(obj: hitObject);
            }

            if (hitManager.enemyProjectileHits && (gameObject.CompareTag("EnemyProjectile") || gameObject.CompareTag("EnemyProjectileAndObstacle")))
            {
                Hit(obj: hitObject);
            }

            if (hitManager.neutralProjectileHits && gameObject.CompareTag("NeutralProjectile"))
            {
                Hit(obj: hitObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (!awakened) return;

        if (collision2D.gameObject.CompareTag("Obstacle") || collision2D.gameObject.CompareTag("EnemyProjectileAndObstacle")) HitObstacle(collision2D);
    }

    void OnCollisionStay2D(Collision2D collision2D)
    {
        if (!awakened) return;

        if (collision2D.gameObject.CompareTag("Obstacle") || collision2D.gameObject.CompareTag("EnemyProjectileAndObstacle")) HitObstacle(collision2D);
    }

    void HitObstacle(Collision2D collision2D)
    {
        GameObject hitObject = collision2D.gameObject;
        if (hitRecently.Contains(hitObject)) return;

        Hit(isObstacle: true);
        StartCoroutine(StoreHitTemporarily(hitObject, seconds: 0.1f));

        if (CompareTag("AllyProjectile") && rb && upgradeManager.phantomWell && remainingBounces <= 0)
        {
            rb.excludeLayers |= 1 << LayerMask.NameToLayer("ObstacleLayer");
            return;
        }

        remainingBounces--;

        ContactPoint2D point = collision2D.contacts[0];
        Vector2 curDire = transform.TransformDirection(Vector2.right);

        Vector2 newDir = Vector2.Reflect(curDire, point.normal);
        transform.rotation = Quaternion.FromToRotation(Vector2.right, newDir);

        if (upgradeManager.bulletGrowthPerBounce > 0)
        {
            transform.localScale *= 1 + upgradeManager.bulletGrowthPerBounce;
        }
    }

    void Hit(bool isObstacle = false, GameObject obj = null)
    {
        if (hitRecently.Contains(obj)) return;
        if (!isObstacle) hits++;

        if (obj) StartCoroutine(StoreHitTemporarily(obj));

        if (CompareTag("AllyProjectile"))
        {
            CheckConsecutiveHits(isObstacle, obj);
            DoExplosion();
            DoChainLightning(isObstacle);
            if (canSplinter && Random.Range(0f, 1f) < upgradeManager.splinterShotChance) DoSplinter();
        }

        if (hits >= destroyAfterHits || !canPierce)
        {
            Destroy(gameObject.GetComponentInParent<Rigidbody2D>().gameObject, 0.01f);
        }
        else if (isObstacle && obstacleDestroys && remainingBounces <= 0)
        {
            if (CompareTag("AllyProjectile"))
            {
                if (!upgradeManager.phantomWell) Destroy(gameObject.GetComponentInParent<Rigidbody2D>().gameObject, 0.01f);
            }
            else Destroy(gameObject.GetComponentInParent<Rigidbody2D>().gameObject, 0.01f);
        }
    }

    private void CheckConsecutiveHits(bool isObstacle, GameObject obj)
    {
        if (!isObstacle)
        {
            if (obj.GetComponent<HitManager>() == upgradeManager.lastHitObject)
            {
                upgradeManager.resetConsecutiveHits = false;
            }
            else
            {
                upgradeManager.resetConsecutiveHits = true;
                upgradeManager.lastHitObject = obj.GetComponent<HitManager>();
            }
        }
    }

    private void DoExplosion()
    {
        if (canExplode && Random.Range(0f, 1f) < upgradeManager.shotsExplodeChance || (upgradeManager.critsAlwaysExplode && critted))
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = upgradeManager.explosionSize * upgradeManager.aoeSizeModifierMult * Vector3.one;

            if (upgradeManager.explosionsAlwaysCrit)
            {
                explosion.GetComponent<Damage>().damage *= upgradeManager.criticalHitDamageMultiplier;
            }
        }
    }

    private void DoChainLightning(bool isObstacle)
    {
        if (!chainEffect) return;

        if (canTriggerChainLightning || (canTriggerWithEHammer && upgradeManager.thorHammer))
        {
            if (isObstacle)
            {
                if (upgradeManager.kineticEnergy) chainEffect.StartChainEffect();
            }
            else chainEffect.StartChainEffect();
        }
    }

    private void DoSplinter()
    {
        canSplinter = false;
        for (int i = 0; i < 2; i++)
        {
            GameObject copy = Instantiate(thisBulletPrefab, transform.position, Quaternion.Euler(0f, 0f, transform.eulerAngles.z + Random.Range(-25f, 25f)));
            ProjectileController copyProjController = copy.GetComponent<ProjectileController>();
            copyProjController.canSplinter = false;
            Damage copyDmg = copy.GetComponentInChildren<Damage>();
            copyDmg.damage = GetComponentInChildren<Damage>().damage * 0.2f;

            if (upgradeManager.splinterSting) copyProjController.critChance = 1f;

            copy.transform.localScale /= 2f;
        }

        Destroy(gameObject, 0.01f);
    }

    IEnumerator StoreHitTemporarily(GameObject obj, float seconds = 0.3f)
    {
        hitRecently.Add(obj);
        yield return new WaitForSeconds(seconds);
        hitRecently.Remove(obj);
    }

    private void OnDestroy()
    {
        if (!awakened) return;

        if (hits == 0)
        {
            upgradeManager.resetConsecutiveHits = true;
            upgradeManager.resetConsecutiveCritHits = true;
        }
    }
}
