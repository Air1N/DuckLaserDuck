using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterDefault : MonoBehaviour
{
    public GameObject prefab;
    public float firerate;
    public float firerateOffset;
    public float randomShootingOffset = 0f;
    public int delayBeforeFirstShot = 0;
    public bool shooting = false;
    public Animator anim;
    [SerializeField] private int animOffset;
    public HitManager hitManager;
    public float fireAngle;
    public float spread;

    public bool isEnemy;

    public bool autoFire;
    public bool noMaxRange = false;
    public float autoFireRadius;

    public bool autoAim;
    [SerializeField] private bool onlyShootIfAimedCorrectly;
    [SerializeField] private bool animateAutoAiming;
    [SerializeField] private bool animateAutoAimingOnlyIfCanShoot;
    [SerializeField] private int animateAimIfCanShootOffset = 0;
    public float aimAngle;
    public Transform aimPoint;

    public bool aimable = true;
    public bool shotFollowsAimer = true;

    public bool fireOnShootButton = true;
    public bool fireOnDashButton = false;

    private PlayerController playerController;

    public Transform inheritRotationFrom;
    public Transform inheritDirectionFrom;
    public float autoAimRotationSpeed;

    public float autoAimAngleLimitMin;
    public float autoAimAngleLimitMax;

    public float autoAimRadius;
    public bool isPlayer;

    public bool animateShooting = true;

    public Transform projectileAttachedTo;

    public float projectileTrackingSpeed;

    public float recoilDistance;
    public float recoilSlerpSpeed;

    public Transform recoilTowards;

    public Transform recoilSubject;
    Vector3 originalRecoilPosition;

    public bool animateRecoil = false;

    public Transform spawnPoint;

    public float counter = 0;
    int direction = 0;

    public int projectileCount = 1;

    private PlayerUpgradesManager upgradeManager;
    private HitManager playerHitManager;

    public Transform attachPoint;

    [SerializeField] private Vector3 rescaleBullet = new Vector3(1f, 1f, 1f);

    [SerializeField] private List<AudioClip> soundEffects;
    [SerializeField] private AudioSource audioSource;

    public bool targetInFireRange = false;
    public bool targetInAimRadius = false;
    public bool aimedCorrectly = false;
    public bool overrideAimedCorrectly = false;

    [SerializeField] private GameObject weakReplicationProjectile;

    private RoomManager roomManager;

    void Start()
    {
        upgradeManager = GameObject.Find("char").GetComponent<PlayerUpgradesManager>();
        playerController = GameObject.Find("char").GetComponent<PlayerController>();
        playerHitManager = GameObject.Find("char").GetComponent<HitManager>();

        roomManager = FindFirstObjectByType<RoomManager>();

        if (isPlayer)
        {
            if (aimable)
            {
                if (!inheritRotationFrom) inheritRotationFrom = GameObject.Find("HeadRotation").transform;
                if (!inheritDirectionFrom) inheritDirectionFrom = GameObject.Find("char").transform;
            }

            if (animateShooting && !anim)
            {
                anim = GameObject.Find("char").GetComponent<Animator>();
            }

            recoilTowards = GameObject.Find("recoilTowards").transform;
            recoilSubject = GameObject.Find("HeadPosition").transform;

            originalRecoilPosition = recoilSubject.localPosition;

            hitManager = GetComponentInParent<HitManager>();
        }
        else
        {
            if ((animateShooting || animateAutoAiming) && anim == null)
            {
                anim = GetComponentInParent<Animator>();
            }
        }

        counter = (int)(50 / firerate) - delayBeforeFirstShot;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hitManager != null && hitManager.dead)
            return;

        if (playerController.inMenu || (roomManager && roomManager.isShopRoom))
            return;

        float realFirerate = firerate;
        int realProjectileCount = projectileCount;
        float realSpread = Mathf.Max(0f, spread);

        if (isPlayer)
        {
            float bonusFirerateMult = 0f;
            float bonusSpreadMult = 0f;

            if (playerController.stallBurstActive < 5 * 50)
                bonusFirerateMult += upgradeManager.notShootingFirerateBoostMult;

            if (playerHitManager.health / playerHitManager.maxHealth < 0.4f)
                bonusFirerateMult += upgradeManager.franticFirerateModMult;

            if (playerController.haltActive < 6 * 50)
                bonusSpreadMult += upgradeManager.haltSpreadMult;

            bonusFirerateMult += Mathf.Min(playerController.isShootingTimer / 50f * 0.1f, upgradeManager.chainGunMaxFirerateMult); // 10 seconds to max
            bonusSpreadMult += Mathf.Min(playerController.isShootingTimer / 50f * 0.1f, upgradeManager.chainGunMaxSpreadMult);

            realFirerate = (firerate + upgradeManager.firerateModifierFlat) * (upgradeManager.firerateModifierMult + bonusFirerateMult);
            realProjectileCount = projectileCount + upgradeManager.projectileCountModifierFlat;
            realSpread = Mathf.Max(0f, (spread + upgradeManager.spreadModifierFlat) * (upgradeManager.spreadModifierMult + bonusSpreadMult));
        }

        counter++;

        if (aimable)
        {
            direction = inheritDirectionFrom.localScale.x < 0 ? 1 : -1;
            aimedCorrectly = false;

            if (autoAim && Vector3.Distance(aimPoint.position, transform.position) < autoAimRadius)
            {
                if (animateAutoAiming) anim.SetBool("aiming", true);
                if (animateAutoAimingOnlyIfCanShoot)
                {
                    if (counter > 50 / realFirerate - animateAimIfCanShootOffset) anim.SetBool("aiming", true);
                    else anim.SetBool("aiming", false);
                }
                aimedCorrectly = RotateToward(aimPoint, inheritRotationFrom);

                targetInAimRadius = true;
            }
            else
            {
                if (animateAutoAiming) anim.SetBool("aiming", false);
                if (animateAutoAimingOnlyIfCanShoot) anim.SetBool("aiming", false);
            }

            if (overrideAimedCorrectly) aimedCorrectly = true;
        }

        if (autoFire)
        {
            if (noMaxRange)
            {
                if (onlyShootIfAimedCorrectly)
                {
                    if (aimedCorrectly) shooting = true;
                    else shooting = false;
                }
                else shooting = true;
                targetInFireRange = true;
            }
            else if (Vector3.Distance(aimPoint.position, transform.position) < autoFireRadius)
            {
                if (onlyShootIfAimedCorrectly)
                {
                    if (aimedCorrectly) shooting = true;
                    else shooting = false;
                }
                else shooting = true;
                targetInFireRange = true;
            }
            else
            {
                shooting = false;
                targetInFireRange = false;
            }
        }


        if (counter > 50 / realFirerate)
        {
            if (shooting)
            {
                if (fireOnDashButton) shooting = false;

                if (animateShooting) anim.SetBool("shooting", true);

                if (counter < 50 / realFirerate + animOffset) return;

                for (int i = 0; i < realProjectileCount; i++)
                {
                    float chanceRoll = Random.Range(0f, 1f);
                    if (i < projectileCount || (isPlayer && chanceRoll <= upgradeManager.additionalProjectileChanceModifierFlat))
                    {
                        float rotationAngle = 0f;
                        if (shotFollowsAimer && inheritRotationFrom)
                        {
                            rotationAngle = inheritRotationFrom.rotation.eulerAngles.z
                                        + 180f * (direction + 1) / 2
                                        + direction * (fireAngle + Random.Range(-realSpread, realSpread));
                        }
                        else if (shotFollowsAimer)
                        {
                            rotationAngle = transform.eulerAngles.z
                                        + 180f * (direction + 1) / 2
                                        + direction * (fireAngle + Random.Range(-realSpread, realSpread));
                        }
                        else
                        {
                            rotationAngle = 0f;
                        }

                        GameObject newProjectile =
                            Instantiate(
                                prefab,
                                spawnPoint.position,
                                Quaternion.Euler(
                                    0f,
                                    0f,
                                    rotationAngle
                                )
                            );

                        newProjectile.transform.localScale = Vector3.Scale(newProjectile.transform.localScale, rescaleBullet);

                        if (projectileAttachedTo)
                            newProjectile.transform.parent = projectileAttachedTo;

                        if (Mathf.Abs(projectileTrackingSpeed) > 0)
                        {
                            if (!newProjectile.GetComponent<TrackingProjectile>()) newProjectile.AddComponent(typeof(TrackingProjectile));
                            newProjectile.GetComponent<TrackingProjectile>().RotationSpeed = projectileTrackingSpeed;
                        }

                        counter = -(int)(firerateOffset + Random.Range(-randomShootingOffset, randomShootingOffset));

                        if (isPlayer)
                        {
                            if (Mathf.Abs(upgradeManager.projectilesTrack + projectileTrackingSpeed) > 0)
                            {
                                if (!newProjectile.GetComponent<TrackingProjectile>()) newProjectile.AddComponent(typeof(TrackingProjectile));
                                newProjectile.GetComponent<TrackingProjectile>().RotationSpeed = upgradeManager.projectilesTrack + projectileTrackingSpeed;
                            }

                            newProjectile
                                .GetComponentInChildren<ProjectileController>()
                                .destroyAfterHits += upgradeManager.piercingBoost;

                            ChainEffect chainEffect = newProjectile.GetComponent<ChainEffect>();
                            chainEffect.maxChainTimes += upgradeManager.chainLightningQty;

                            if (Random.Range(0f, 1f) < upgradeManager.weakReplicationChance)
                            {
                                StartCoroutine(TrySpawnReplicate(rotationAngle));
                            }
                        }

                        if (attachPoint)
                        {
                            newProjectile.GetComponentInChildren<AttachTo>().attachTo = attachPoint;
                        }

                        if (Mathf.Abs(recoilDistance) > 0)
                            animateRecoil = true;

                        if (soundEffects.Count > 0)
                        {
                            int soundIdx = Random.Range(0, soundEffects.Count);
                            audioSource.clip = soundEffects[soundIdx];
                            audioSource.Play();
                        }
                    }
                }
            }
            else
            {
                if (animateShooting) anim.SetBool("shooting", false);
                counter = (int)(50 / realFirerate - firerateOffset);
            }
        }

        if (animateRecoil)
        {
            AnimateRecoil();
        }
    }

    void LateUpdate()
    {
        if (anim != null) anim.SetBool("shooting", false);
    }

    void OnShoot(InputValue value)
    {
        if (isPlayer && fireOnShootButton)
        {
            if (hitManager != null && hitManager.dead)
                return;

            shooting = value.isPressed;
        }
    }

    void OnDash(InputValue value)
    {
        if (isPlayer && fireOnDashButton)
        {
            if (hitManager != null && hitManager.dead)
                return;

            if (playerController.canDash || playerController.dashing) shooting = value.isPressed;
        }
    }

    private bool RotateToward(Transform target, Transform ye)
    {
        Vector3 dir = target.position - ye.position;

        float unlimitedTargetAngle =
            (
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
                + 1440f
                + 180f * (direction + 1) / 2
                + direction * aimAngle
            ) % 360;

        float targetAngle = unlimitedTargetAngle;

        float adjustedTargetAngle = Mathf.Abs((360f * (direction + 1) / 2 - targetAngle) % 360); // not sure why I do this?

        if (adjustedTargetAngle < autoAimAngleLimitMin)
        {
            targetAngle = Mathf.Abs(360f * (direction + 1) / 2 - autoAimAngleLimitMin);
        }

        if (adjustedTargetAngle > autoAimAngleLimitMax)
        {
            targetAngle = Mathf.Abs(360f * (direction + 1) / 2 - autoAimAngleLimitMax);
        }

        float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, targetAngle);
        float newAngle =
            Mathf.Min(Mathf.Abs(deltaAngle), autoAimRotationSpeed) * Mathf.Sign(deltaAngle);

        float unlimitedDeltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, unlimitedTargetAngle);

        ye.Rotate(transform.forward, newAngle);

        if (Mathf.Abs(unlimitedDeltaAngle) <= 5f) return true;
        return false;
    }

    void AnimateRecoil()
    {
        if (
            Vector3.Distance(
                recoilSubject.position,
                recoilSubject.TransformPoint(originalRecoilPosition)
            ) < recoilDistance
        )
        {
            Vector3 slerpPos = Vector3.Slerp(
                recoilSubject.position,
                recoilTowards.position,
                recoilSlerpSpeed
            );
            recoilSubject.position = slerpPos;
        }
        else
        {
            animateRecoil = false;
        }
    }

    private IEnumerator TrySpawnReplicate(float rotationAngle)
    {
        yield return new WaitForSeconds(0.1f);

        GameObject weakReplicateObj = Instantiate(prefab,
                                spawnPoint.position,
                                Quaternion.Euler(
                                    0f,
                                    0f,
                                    rotationAngle
                                ));

        weakReplicateObj.transform.localScale = Vector3.Scale(weakReplicateObj.transform.localScale, rescaleBullet);

        weakReplicateObj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.2f);

        if (Mathf.Abs(projectileTrackingSpeed) > 0 && weakReplicateObj.GetComponent<TrackingProjectile>())
            weakReplicateObj.GetComponent<TrackingProjectile>().RotationSpeed = projectileTrackingSpeed;
    }
}
