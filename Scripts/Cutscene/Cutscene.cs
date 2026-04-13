using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base component for cutscene prefab roots. Attach to the top-level GameObject
/// of a cutscene prefab. Override <see cref="OnBegin"/> and <see cref="OnEnd"/>
/// for custom setup/teardown. Call <see cref="Complete"/> when the cutscene
/// finishes naturally (animation event, timeline signal, etc.).
/// If <see cref="duration"/> > 0 the cutscene auto-completes after that time.
/// </summary>
public class Cutscene : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID. Used for completion flags and lookup.")]
    public string cutsceneID;

    [Header("Behaviour")]
    public bool skippable = true;

    [Tooltip("If > 0 the cutscene auto-completes after this many seconds (unscaled).")]
    public float duration = 0f;

    [Header("Completion")]
    [Tooltip("Flag set in FlagStateManager when this cutscene completes or is skipped. " +
             "Leave empty for no flag.")]
    public string completionFlag;

    /// <summary>Raised once when the cutscene finishes (naturally or skipped).</summary>
    public event Action Completed;

    private bool _running;

    /* ── Called by CutsceneManager ────────────────────────── */

    public void Begin()
    {
        _running = true;
        gameObject.SetActive(true);
        OnBegin();
        if (duration > 0f) StartCoroutine(AutoComplete());
    }

    public void End()
    {
        _running = false;
        StopAllCoroutines();
        OnEnd();
    }

    /* ── Call from animation events / timeline signals ───── */

    /// <summary>Signal that the cutscene reached its natural end.</summary>
    public void Complete()
    {
        if (!_running) return;
        _running = false;
        Completed?.Invoke();
    }

    /* ── Override points ─────────────────────────────────── */

    protected virtual void OnBegin() { }
    protected virtual void OnEnd() { }

    /* ── Internal ────────────────────────────────────────── */

    private IEnumerator AutoComplete()
    {
        yield return new WaitForSecondsRealtime(duration);
        if (_running) Complete();
    }
}