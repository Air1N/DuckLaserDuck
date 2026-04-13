using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UpgradeToPlayer : MonoBehaviour
{

    public float addShotsExplodeChance = 0f;
    public float addExplosionSize = 0f;

    public float addAOESizeModifierMult = 0f;

    public float addProjectilesTrack = 0f;

    public int addPiercingBoost = 0;

    public float addChainLightningDamage = 0f;

    public int addChainLightningQty = 0;

    public float addChainLightningOdds = 0;

    public float addRandomizeDmgPercent = 0f;

    public float addDmgModifierFlat = 0f;
    public float addDmgModifierMult = 0f;

    public float addSpreadModifierFlat = 0f;
    public float addSpreadModifierMult = 0f;

    public float addBulletSpeedModifierFlat = 0f;
    public float addBulletSpeedModifierMult = 0f;

    public float addBulletLifetimeModifierFlat = 0f;
    public float addBulletLifetimeModifierMult = 0f;

    public float addBulletSizeModifierMult = 0f;

    public float addPlayerMoveSpeedModifier = 0f;

    public float addKnockbackModifierFlat = 0f;
    public float addKnockbackModifierMult = 0f;
    public float addWiggly = 0f;

    public bool addShotsOrbit = false;

    public float addOrbitRadius = 0f;

    public float addDashCooldownModifierFlat = 0f;
    public float addDashCooldownModifierMult = 0f;

    public int addProjectileCountModifierFlat = 0;
    public float addAdditionalProjectileChanceModifierFlat = 0f;

    public int addBouncinessCount = 0;

    public float addDashCooldownModifier = 0f;

    public float addWormDMGCoeff = 0f;
    public float addSpeedDMGCoeff = 0f;

    public int addBurnStacksPerShot = 0;
    public float addBurnChance = 0f;
    public int addPoisonStacksPerShot = 0;
    public float addPoisonChance = 0f;

    public int addFrostStacksPerShot = 0;
    public float addFrostChance = 0f;

    public float addFirerateModifierFlat = 0f;
    public float addFirerateModifierMult = 0f;
    public float addCriticalHitChance = 0f;
    public float addCriticalHitDamageMultiplier = 0f;
    public bool addExplosionsAlwaysCrit = false;
    public bool addCritsAlwaysExplode = false;
    public bool addElempyre = false;
    public float addBugsprayPercent = 0f;
    public bool addThorHammer = false;
    public bool addCarrionBlast = false;
    public bool addPhantomWell = false;

    public float addSplinterShotChance = 0f;
    public float addVaporizingShotChance = 0f;

    public float addStandingStillFirerateBoost = 0f;
    public float addNotShootingFirerateBoostMult = 0f;
    public float addChainGunMaxFirerateMult = 0f;
    public float addChainGunMaxSpreadMult = 0f;
    public float addAcceleratingShotStartSpeed = 0f;
    public float addAcceleratingShotAccelRate = 0f;
    public float addAvoidanceDefense = 0f;
    public float addInertiaAcceleration = 0f;
    public int addGaseousCloud = 0;
    public int addPulsingShot = 0;
    public float addBulletGrowthPerBounce = 0;
    public int addInflation = 0;
    public int addGamblerBombMult = 0;
    public float addRewardsCardDiscount = 0f;
    public int addCreditCardDebtLimit = 0;

    public int addHighVoltage = 0;
    public bool addKineticEnergy = false;

    public float addDroneBulletSizeModifierMult = 0f;
    public float addDroneBulletDamageModifierFlat = 0f;

    public float addConductiveFlamesDmgModifierFlat = 0f;
    public float addConductiveFlamesDmgModifierMult = 0f;

    public float addHaltDamageMult = 0f;
    public float addHaltSpreadMult = 0f;

    public float addFranticFirerateModMult = 0f;

    public bool addBloodletting = false;
    public bool addPressurizedNeedle = false;

    public bool addGotDamagedNextShotBoost = false;
    public bool addGhostKnife = false;

    public float addWeakReplicationChance = 0f;
    public int addShrapnel = 0;

    public float addOnKillDmgModifierFlat = 0f;
    public float addOnKillDmgModifierMult = 0f;

    public float addOnKillSizeModifierMult = 0f;

    public bool addSplinterSting = false;
    public bool addAPRound = false;

    public float addConsecutiveHitDamageModifierFlat = 0f;
    public float addConsecutiveHitDamageModifierMult = 0f;

    public float addConsecutiveCriticalHitDamageModifierFlat = 0f;
    public float addConsecutiveCriticalHitDamageModifierMult = 0f;

    /* ═══ Reflection Apply ══════════════════════════════════
 *  Strips "add" prefix, matches to PUM field by name
 *  (case-insensitive), then:
 *    float/int → +=  (skipped if 0)
 *    bool      → = true (skipped if false)
 * ═══════════════════════════════════════════════════════ */

    private static readonly BindingFlags FIELD_FLAGS =
        BindingFlags.Public | BindingFlags.Instance;

    private static Dictionary<string, FieldInfo> _pumCache;
    private static FieldInfo[] _myFields;

    void Start()
    {
        var pum = FindFirstObjectByType<PlayerUpgradesManager>();
        if (pum != null) Apply(pum);
    }

    public void Apply(PlayerUpgradesManager pum)
    {
        if (pum == null) return;
        EnsureCache();

        foreach (var field in _myFields)
        {
            string key = field.Name;
            if (key.StartsWith("add") && key.Length > 3)
                key = key.Substring(3);

            if (!_pumCache.TryGetValue(key.ToLowerInvariant(), out var pumField)) continue;
            if (pumField.FieldType != field.FieldType) continue;

            if (field.FieldType == typeof(bool))
            {
                if ((bool)field.GetValue(this))
                    pumField.SetValue(pum, true);
            }
            else if (field.FieldType == typeof(float))
            {
                float val = (float)field.GetValue(this);
                if (val != 0f)
                    pumField.SetValue(pum, (float)pumField.GetValue(pum) + val);
            }
            else if (field.FieldType == typeof(int))
            {
                int val = (int)field.GetValue(this);
                if (val != 0)
                    pumField.SetValue(pum, (int)pumField.GetValue(pum) + val);
            }
        }
    }

    private static void EnsureCache()
    {
        if (_myFields != null) return;

        var all = typeof(UpgradeToPlayer).GetFields(FIELD_FLAGS);
        var filtered = new List<FieldInfo>();
        foreach (var f in all)
        {
            if (f.DeclaringType != typeof(UpgradeToPlayer)) continue;
            filtered.Add(f);
        }
        _myFields = filtered.ToArray();

        _pumCache = new Dictionary<string, FieldInfo>();
        foreach (var f in typeof(PlayerUpgradesManager).GetFields(FIELD_FLAGS))
            _pumCache[f.Name.ToLowerInvariant()] = f;
    }
}
