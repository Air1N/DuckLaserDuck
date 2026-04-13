using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Linq;
using System.Reflection;
using Game.State;

public class PlayerUpgradesManager : MonoBehaviour
{
    public Dictionary<string, object> originalValues = new();
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private PlayerInventoryManager inventoryManager;

    public Transform projectileHeadSpawnPoint;
    public Transform projectileFeetSpawnPoint;


    public List<string> upgrades = new List<string>();
    public List<string> items = new List<string>();
    public List<GameObject> itemObjects;


    public float shotsExplodeChance = 0f;
    public float explosionSize = 0.2f;

    public float aoeSizeModifierMult = 1f;

    public float projectilesTrack = 0f;

    public int piercingBoost = 0;

    public float chainLightningDamage;

    public int chainLightningQty;

    public float chainLightningOdds;

    public float randomizeDmgPercent = 0f;

    public float dmgModifierFlat = 0;
    public float dmgModifierMult = 1f;

    public float spreadModifierFlat = 0f;
    public float spreadModifierMult = 1f;

    public float bulletSpeedModifierFlat = 0f;
    public float bulletSpeedModifierMult = 1f;

    public float bulletLifetimeModifierFlat = 0f;
    public float bulletLifetimeModifierMult = 1f;

    public float bulletSizeModifierMult = 1f;

    public float playerMoveSpeedModifier = 0f;

    public float knockbackModifierFlat = 0f;
    public float knockbackModifierMult = 1f;
    public float wiggly = 0f;

    public bool shotsOrbit = false;

    public float orbitRadius = 0f;

    public float dashCooldownModifierFlat = 0f;
    public float dashCooldownModifierMult = 1f;

    public int projectileCountModifierFlat = 0;
    public float additionalProjectileChanceModifierFlat = 0f;

    public int bouncinessCount = 0;

    public float wormDMGCoeff = 0f;
    public float speedDMGCoeff = 0f;

    public int burnStacksPerShot = 0;
    public float burnChance = 0f;
    public int poisonStacksPerShot = 0;
    public float poisonChance = 0f;

    public int frostStacksPerShot = 0;
    public float frostChance = 0f;

    public float firerateModifierFlat = 0f;
    public float firerateModifierMult = 1f;
    public float criticalHitChance = 0.01f;
    public float criticalHitDamageMultiplier = 2f;
    public bool explosionsAlwaysCrit = false;
    public bool critsAlwaysExplode = false;
    public bool elempyre = false;
    public float bugsprayPercent = 0;
    public bool thorHammer = false;
    public bool carrionBlast = false;
    public bool phantomWell = false;

    public float splinterShotChance = 0f;
    public float vaporizingShotChance = 0f;

    public float standingStillFirerateBoost = 0f;
    public float notShootingFirerateBoostMult = 0f;
    public float chainGunMaxFirerateMult = 0f;
    public float chainGunMaxSpreadMult = 0f;
    public float acceleratingShotStartSpeed = 0f;
    public float acceleratingShotAccelRate = 0f;
    public float avoidanceDefense = 0f;
    public float inertiaAcceleration = 0f;
    public int gaseousCloud = 0;
    public int pulsingShot = 0;
    public float bulletGrowthPerBounce = 0;
    public int inflation = 0;
    public float gamblerBombMult = 1f;
    public float rewardsCardDiscount = 0;
    public int creditCardDebtLimit = 0;

    public int highVoltage = 0;
    public bool kineticEnergy = false;

    public float droneBulletSizeModifierMult = 1f;
    public float droneBulletDamageModifierFlat = 0f;

    public float conductiveFlamesDmgModifierFlat = 0f;
    public float conductiveFlamesDmgModifierMult = 1f;

    public float haltDamageMult = 1f;
    public float haltSpreadMult = 0f;

    public float franticFirerateModMult = 0f;

    public bool bloodletting = false;
    public bool pressurizedNeedle = false;

    public bool gotDamagedNextShotBoost = false;
    public bool ghostKnife = false;

    public float weakReplicationChance = 0f;
    public int shrapnel = 0;

    public float onKillDmgModifierFlat = 0f;
    public float onKillDmgModifierMult = 1f;

    public float onKillSizeModifierMult = 1f;

    public bool splinterSting = false;

    public bool apRound = false;

    public float consecutiveHitDamageModifierFlat = 0f;
    public float consecutiveHitDamageModifierMult = 1f;

    public float consecutiveCriticalHitDamageModifierFlat = 0f;
    public float consecutiveCriticalHitDamageModifierMult = 1f;

    public bool resetConsecutiveHits = false;
    public bool resetConsecutiveCritHits = false;

    public List<float> last20Hits = new() { 0f };
    public float totalDamageDealt = 0f;
    private float lastTotalDamageDealt = 0f;
    public float totalHitsDelivered = 0f;

    public HitManager lastHitObject = null;

    public SaveGameManager saveGameManager;
    private StatsAndAchievements statsAndAchievements;

    public bool loveRNGRandomized = false;

    private readonly List<string> projectileNames = new()
    {
        "machinegun",
        "seeker",
        "piercer",
        "shotty",
        "drone"
    };

    private readonly List<string> aoeNames = new()
    {
        "explosion",
        "dash aura",
        "gaseous cloud"
    };

    private void Awake()
    {
        foreach (FieldInfo pumField in typeof(PlayerUpgradesManager).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            originalValues[pumField.Name] = pumField.GetValue(this);
        }
    }

    private void Start()
    {
        statsAndAchievements = FindObjectOfType<StatsAndAchievements>();
    }

    private void FixedUpdate()
    {
        if (last20Hits.Count > 20)
            last20Hits.RemoveAt(0);

        if (totalDamageDealt != lastTotalDamageDealt)
        {
            saveGameManager.saveData.m_totalLifetimeDamageDealt += totalDamageDealt - lastTotalDamageDealt;
            saveGameManager.SaveJsonData();

            statsAndAchievements.m_flTotalDamageDealt = saveGameManager.saveData.m_totalLifetimeDamageDealt;
        }
        lastTotalDamageDealt = totalDamageDealt;
    }

    public bool CheckRequirements(string[] requirements)
    {
        // and | or/or | -not | *only (bypasses all other requirements if met)

        bool endResult = true;

        foreach (string requirement in requirements)
        {
            if (requirement.StartsWith("*") && EvaluateCondition(requirement.TrimStart('*'))) return true;
            else if (requirement.Contains("/")) endResult &= requirement.Split('/').Any(part => EvaluateCondition(part));
            else if (requirement.StartsWith("-")) endResult &= !EvaluateCondition(requirement.TrimStart('-'));
            else endResult &= EvaluateCondition(requirement);
        }

        return endResult;
    }

    private int GetCurrentRoom() => roomManager.currentRoomNum + roomManager.currentBiomeNum * 10;

    private bool EvaluateCondition(string condition)
    {
        if (condition.StartsWith("flag:"))
            return FlagStateManager.IsSet(condition.Substring(5));

        if (condition.StartsWith("!flag:"))
            return !FlagStateManager.IsSet(condition.Substring(6));

        if (condition.StartsWith("Room>"))
            return GetCurrentRoom() > int.Parse(condition.Substring(5));
        if (condition.StartsWith("Room<"))
            return GetCurrentRoom() < int.Parse(condition.Substring(5));
        if (condition.StartsWith("Room="))
            return GetCurrentRoom() == int.Parse(condition.Substring(5));

        return condition switch
        {
            "AnyProjectile" => upgrades.Intersect(projectileNames).Any(),
            "AnyAOE" => upgrades.Intersect(aoeNames).Any(),
            _ => upgrades.Contains(condition)
        };
    }

    public bool CheckOverLevel(UpgradeData upgradeSelected)
    {
        List<string> selectedUpgradeOccurences = upgrades.FindAll(
            x => x.Equals(upgradeSelected.title)
        );

        List<string> selectedItemOccurences = items.FindAll(
            x => x.Equals(upgradeSelected.title)
        );

        int selectedUpgradeLevel = selectedUpgradeOccurences.Count + selectedItemOccurences.Count;

        return selectedUpgradeLevel >= upgradeSelected.maxLevel;
    }

    public bool RollRarity(int upgradeRarity)
    {
        return Random.Range(0f, 100f) > upgradeRarity * 19;
    }

    public void AddItemToInventory(GameObject upgradeObject)
    {
        inventoryManager.AddItem(upgradeObject);
    }
}
