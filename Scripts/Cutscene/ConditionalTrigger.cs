using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Game.State;

public class ConditionalTrigger : MonoBehaviour
{
    /* ═══ Condition Types ═══════════════════════════════════ */

    public enum Condition
    {
        Manual,
        OnStart,
        OnEnable,
        OnDisable,
        OnDestroy,
        OnDeath,
        OnDestroyAnother,
        OnTriggerEnter2D,
        OnTriggerExit2D,
        OnCollisionEnter2D,
        OnRoomCleared,
        OnRoomClearedAndLooted,
        WhenAllFlagsSet,
        WhenAnyFlagSet,
        CustomEvent,
        OnUpgradeRequirementMet,
    }

    /* ═══ Action Types ══════════════════════════════════════ */

    public enum ActionType
    {
        Activate,
        Deactivate,
        Destroy,
        SetFlag,
        ClearFlag,
        PlayCutscene,
        RegisterForRoomCleanup,
        SpawnPrefab,
        InvokeUnityEvent,
    }

    [System.Serializable]
    public class ActionEntry
    {
        public ActionType type;

        [Tooltip("Target for Activate/Deactivate/Destroy/RegisterForRoomCleanup. Uses this GameObject if empty.")]
        public GameObject target;

        [Tooltip("Flag ID for SetFlag/ClearFlag.")]
        public string flagID;

        [Tooltip("Cutscene prefab for PlayCutscene.")]
        public Cutscene cutscenePrefab;
        public bool pauseDuringCutscene = true;

        [Tooltip("Prefab to instantiate for SpawnPrefab.")]
        public GameObject spawnPrefab;
        public Transform spawnPoint;
        public Transform spawnParent;
        public int spawnCount = 1;

        [Tooltip("Invoked when type is InvokeUnityEvent.")]
        public UnityEvent unityEvent;
    }

    /* ═══ Inspector ═════════════════════════════════════════ */

    [Header("Condition")]
    [SerializeField] private Condition condition;

    [Header("Prerequisites — ALL must pass to fire")]
    [SerializeField] private string[] prerequisiteFlags;
    [Tooltip("Uses PlayerUpgradesManager.CheckRequirements() syntax.")]
    [SerializeField] private string[] upgradePrerequisites;

    [Header("Prevention — blocks firing if ANY is met")]
    [SerializeField] private string[] preventionFlags;
    [Tooltip("If this upgrade requirement IS met, the trigger is BLOCKED.")]
    [SerializeField] private string[] preventionUpgradeRequirements;

    [Header("Timing")]
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private int requiredHits = 1;

    [Header("Condition — Death")]
    [SerializeField] private HitManager hitManager;

    [Header("Condition — DestroyAnother")]
    [SerializeField] private GameObject watchedObject;

    [Header("Condition — Physics")]
    [SerializeField] private string tagFilter = "";

    [Header("Condition — Flags")]
    [SerializeField] private string[] watchedFlags;

    [Header("Condition — Upgrade Requirement")]
    [SerializeField] private string[] watchedUpgradeRequirements;

    [Header("Actions")]
    [SerializeField] private ActionEntry[] actions;

    /* ═══ Runtime ═══════════════════════════════════════════ */

    private RoomManager _room;
    private PlayerUpgradesManager _pum;
    private DestroyNotifier _watchedNotifier;
    private int _hits;
    private bool _fired;
    private bool _wasCleared;
    private bool _wasDead;
    private bool _wasUpgradeReqMet;
    private bool _started;

    /* ═══ Unity Lifecycle ═══════════════════════════════════ */

    void Awake()
    {
        if (hitManager == null)
            hitManager = GetComponent<HitManager>();
    }

    void Start()
    {
        _started = true;
        _room = FindFirstObjectByType<RoomManager>();
        _pum = FindFirstObjectByType<PlayerUpgradesManager>();

        if (hitManager != null) _wasDead = hitManager.dead;
        if (_room != null) _wasCleared = _room.roomCleared;

        if (condition == Condition.OnDestroyAnother && watchedObject != null)
        {
            _watchedNotifier = DestroyNotifier.Require(watchedObject);
            _watchedNotifier.Destroyed += OnWatchedDestroyed;
        }

        if (condition == Condition.OnUpgradeRequirementMet && _pum != null
            && watchedUpgradeRequirements != null && watchedUpgradeRequirements.Length > 0)
            _wasUpgradeReqMet = _pum.CheckRequirements(watchedUpgradeRequirements);

        if (condition == Condition.OnStart)
            TryFire();

        if (condition == Condition.OnEnable && gameObject.activeInHierarchy)
            TryFire();
    }

    void OnEnable()
    {
        FlagStateManager.OnFlagSet += OnFlagEvent;
        FlagStateManager.OnFlagCleared += OnFlagEvent;

        if (condition == Condition.OnEnable && _started)
            TryFire();
    }

    void OnDisable()
    {
        FlagStateManager.OnFlagSet -= OnFlagEvent;
        FlagStateManager.OnFlagCleared -= OnFlagEvent;

        if (condition == Condition.OnDisable)
            TryFire();
    }

    void OnDestroy()
    {
        FlagStateManager.OnFlagSet -= OnFlagEvent;
        FlagStateManager.OnFlagCleared -= OnFlagEvent;

        if (_watchedNotifier != null)
            _watchedNotifier.Destroyed -= OnWatchedDestroyed;

        if (condition == Condition.OnDestroy)
            TryFire();
    }

    void Update()
    {
        if (_fired && oneShot) return;

        switch (condition)
        {
            case Condition.OnDeath:
                PollDeath();
                break;
            case Condition.OnRoomCleared:
                PollRoom(false);
                break;
            case Condition.OnRoomClearedAndLooted:
                PollRoom(true);
                break;
            case Condition.OnUpgradeRequirementMet:
                PollUpgradeRequirement();
                break;
        }
    }

    /* ═══ Physics ═══════════════════════════════════════════ */

    void OnTriggerEnter2D(Collider2D other)
    {
        if (condition != Condition.OnTriggerEnter2D) return;
        if (!PassesTagFilter(other.gameObject)) return;
        TryFire();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (condition != Condition.OnTriggerExit2D) return;
        if (!PassesTagFilter(other.gameObject)) return;
        TryFire();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (condition != Condition.OnCollisionEnter2D) return;
        if (!PassesTagFilter(col.gameObject)) return;
        TryFire();
    }

    /* ═══ Callbacks ═════════════════════════════════════════ */

    private void OnFlagEvent(string flagID)
    {
        if (_fired && oneShot) return;
        switch (condition)
        {
            case Condition.WhenAllFlagsSet:
                if (FlagStateManager.AllSet(watchedFlags)) TryFire();
                break;
            case Condition.WhenAnyFlagSet:
                if (FlagStateManager.AnySet(watchedFlags)) TryFire();
                break;
        }
    }

    private void OnWatchedDestroyed(GameObject obj)
    {
        if (condition != Condition.OnDestroyAnother) return;
        _watchedNotifier = null;
        TryFire();
    }

    /* ═══ Polling ═══════════════════════════════════════════ */

    private void PollDeath()
    {
        if (hitManager == null) return;
        bool dead = hitManager.dead;
        if (dead && !_wasDead) TryFire();
        _wasDead = dead;
    }

    private void PollRoom(bool requireLooted)
    {
        if (_room == null) return;
        bool cleared = requireLooted
            ? _room.roomCleared && _room.fullyLooted
            : _room.roomCleared;
        if (cleared && !_wasCleared) TryFire();
        _wasCleared = cleared;
    }

    private void PollUpgradeRequirement()
    {
        if (_pum == null) return;
        if (watchedUpgradeRequirements == null || watchedUpgradeRequirements.Length == 0) return;
        bool met = _pum.CheckRequirements(watchedUpgradeRequirements);
        if (met && !_wasUpgradeReqMet) TryFire();
        _wasUpgradeReqMet = met;
    }

    /* ═══ Public ════════════════════════════════════════════ */

    public void Trigger()
    {
        if (condition != Condition.Manual && condition != Condition.CustomEvent) return;
        TryFire();
    }

    /* ═══ Firing ════════════════════════════════════════════ */

    private void TryFire()
    {
        if (_fired && oneShot) return;
        if (!PassesAllChecks()) return;

        _hits++;
        if (_hits < requiredHits) return;

        _fired = true;
        _hits = 0;

        if (delay > 0f && gameObject.activeInHierarchy)
            StartCoroutine(DelayedExecute());
        else
            Execute();
    }

    private IEnumerator DelayedExecute()
    {
        yield return new WaitForSecondsRealtime(delay);
        Execute();
    }

    private void Execute()
    {
        if (actions == null) return;

        foreach (var a in actions)
        {
            GameObject t = a.target != null ? a.target : gameObject;

            switch (a.type)
            {
                case ActionType.Activate:
                    t.SetActive(true);
                    break;

                case ActionType.Deactivate:
                    t.SetActive(false);
                    break;

                case ActionType.Destroy:
                    Destroy(t);
                    break;

                case ActionType.SetFlag:
                    if (!string.IsNullOrEmpty(a.flagID))
                        FlagStateManager.Set(a.flagID);
                    break;

                case ActionType.ClearFlag:
                    if (!string.IsNullOrEmpty(a.flagID))
                        FlagStateManager.Clear(a.flagID);
                    break;

                case ActionType.PlayCutscene:
                    if (a.cutscenePrefab != null && CutsceneManager.Instance != null)
                        CutsceneManager.Instance.Play(a.cutscenePrefab, a.pauseDuringCutscene);
                    break;

                case ActionType.RegisterForRoomCleanup:
                    var room = FindFirstObjectByType<RoomManager>();
                    if (room != null)
                        room.trashedObjectsAtRoomEnd.Add(t);
                    break;

                case ActionType.SpawnPrefab:
                    if (a.spawnPrefab != null)
                    {
                        Transform point = a.spawnPoint != null ? a.spawnPoint : transform;
                        for (int i = 0; i < Mathf.Max(1, a.spawnCount); i++)
                            Instantiate(a.spawnPrefab, point.position, point.rotation, a.spawnParent);
                    }
                    break;

                case ActionType.InvokeUnityEvent:
                    a.unityEvent?.Invoke();
                    break;
            }
        }
    }

    /* ═══ Checks ════════════════════════════════════════════ */

    private bool PassesAllChecks()
    {
        if (!FlagStateManager.AllSet(prerequisiteFlags))
            return false;

        if (preventionFlags != null && preventionFlags.Length > 0)
            if (FlagStateManager.AnySet(preventionFlags))
                return false;

        if (upgradePrerequisites != null && upgradePrerequisites.Length > 0)
        {
            if (_pum == null) _pum = FindFirstObjectByType<PlayerUpgradesManager>();
            if (_pum == null || !_pum.CheckRequirements(upgradePrerequisites))
                return false;
        }

        if (preventionUpgradeRequirements != null && preventionUpgradeRequirements.Length > 0)
        {
            if (_pum == null) _pum = FindFirstObjectByType<PlayerUpgradesManager>();
            if (_pum != null && _pum.CheckRequirements(preventionUpgradeRequirements))
                return false;
        }

        return true;
    }

    private bool PassesTagFilter(GameObject other)
        => string.IsNullOrEmpty(tagFilter) || other.CompareTag(tagFilter);
}