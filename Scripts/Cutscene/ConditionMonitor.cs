using System.Collections;
using UnityEngine;
using Game.State;

/// <summary>
/// Detects a chosen condition and calls <see cref="Execute"/> when met.
/// Subclass for specific actions (cutscenes, game-object manipulation, etc.).
/// </summary>
public abstract class ConditionMonitor : MonoBehaviour
{
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
    }

    [Header("Condition")]
    [SerializeField] protected Condition condition;

    [Header("Prerequisites")]
    [Tooltip("ALL of these flags must be set before this monitor can fire.")]
    [SerializeField] private string[] prerequisiteFlags;

    [Header("Settings")]
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool oneShot = true;
    [Tooltip("Times the condition must trigger before executing.")]
    [SerializeField] private int requiredHits = 1;

    [Header("Condition — Death")]
    [Tooltip("HitManager to watch. Auto-found on this GameObject if empty.")]
    [SerializeField] private HitManager hitManager;

    [Header("Condition — DestroyAnother")]
    [Tooltip("Watch this object for destruction.")]
    [SerializeField] private GameObject watchedObject;

    [Header("Condition — Physics")]
    [Tooltip("Only react to objects with this tag. Empty = any.")]
    [SerializeField] private string tagFilter = "";

    [Header("Condition — Flags")]
    [Tooltip("Flag IDs for WhenAllFlagsSet / WhenAnyFlagSet.")]
    [SerializeField] private string[] watchedFlags;

    [Header("Condition — CustomEvent")]
    [Tooltip("Event name to listen for via GameEvents.Raise().")]
    [SerializeField] private string customEventName;

    /* ── Runtime ─────────────────────────────────────────── */

    private RoomManager _room;
    private int _hits;
    private bool _fired;
    private bool _wasCleared;
    private bool _wasDead;
    private DestroyNotifier _watchedNotifier;
    private bool _started;

    protected abstract void Execute();

    /* ── Unity Lifecycle ─────────────────────────────────── */

    protected virtual void Awake()
    {
        if (hitManager == null)
            hitManager = GetComponent<HitManager>();
    }

    protected virtual void Start()
    {
        _started = true;
        _room = FindFirstObjectByType<RoomManager>();

        if (hitManager != null) _wasDead = hitManager.dead;
        if (_room != null) _wasCleared = _room.roomCleared;

        // OnDestroyAnother — hook into watched object
        if (condition == Condition.OnDestroyAnother && watchedObject != null)
        {
            _watchedNotifier = DestroyNotifier.Require(watchedObject);
            _watchedNotifier.Destroyed += OnWatchedDestroyed;
        }

        if (condition == Condition.OnStart) TryFire();
    }

    protected virtual void OnEnable()
    {
        FlagStateManager.OnFlagSet += OnFlagEvent;
        FlagStateManager.OnFlagCleared += OnFlagEvent;

        if (!string.IsNullOrEmpty(customEventName) && condition == Condition.CustomEvent)
            GameEvents.Subscribe(customEventName, OnCustomEvent);

        if (condition == Condition.OnEnable && _started)
            TryFire();
    }

    protected virtual void OnDisable()
    {
        FlagStateManager.OnFlagSet -= OnFlagEvent;
        FlagStateManager.OnFlagCleared -= OnFlagEvent;

        if (!string.IsNullOrEmpty(customEventName))
            GameEvents.Unsubscribe(customEventName, OnCustomEvent);

        if (condition == Condition.OnDisable) TryFire();
    }

    protected virtual void OnDestroy()
    {
        FlagStateManager.OnFlagSet -= OnFlagEvent;
        FlagStateManager.OnFlagCleared -= OnFlagEvent;

        if (!string.IsNullOrEmpty(customEventName))
            GameEvents.Unsubscribe(customEventName, OnCustomEvent);

        if (_watchedNotifier != null)
            _watchedNotifier.Destroyed -= OnWatchedDestroyed;

        if (condition == Condition.OnDestroy) TryFire();
    }

    void Update()
    {
        if (_fired && oneShot) return;
        if (!PrerequisitesMet()) return;

        switch (condition)
        {
            case Condition.OnDeath:
                PollDeath();
                break;
            case Condition.OnRoomCleared:
                PollRoom(requireLooted: false);
                break;
            case Condition.OnRoomClearedAndLooted:
                PollRoom(requireLooted: true);
                break;
        }
    }

    /* ── Physics ─────────────────────────────────────────── */

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

    /* ── Callbacks ───────────────────────────────────────── */

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

    private void OnCustomEvent()
    {
        if (condition != Condition.CustomEvent) return;
        TryFire();
    }

    private void OnWatchedDestroyed(GameObject obj)
    {
        if (condition != Condition.OnDestroyAnother) return;
        TryFire();
    }

    /* ── Poll Helpers ────────────────────────────────────── */

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

    /* ── Firing ──────────────────────────────────────────── */

    public void FireManual()
    {
        if (condition != Condition.Manual) return;
        TryFire();
    }

    protected void TryFire()
    {
        if (_fired && oneShot) return;
        if (!PrerequisitesMet()) return;

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

    private bool PrerequisitesMet()
        => FlagStateManager.AllSet(prerequisiteFlags);

    private bool PassesTagFilter(GameObject other)
        => string.IsNullOrEmpty(tagFilter) || other.CompareTag(tagFilter);
}