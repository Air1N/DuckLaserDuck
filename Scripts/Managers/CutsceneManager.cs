using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.State;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton that manages cutscene playback. Pauses the game while a cutscene
/// is active and handles skip input. Place on a persistent GameObject in
/// your Game scene.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    /* ── Singleton ───────────────────────────────────────── */

    public static CutsceneManager Instance { get; private set; }

    /* ── Inspector ───────────────────────────────────────── */

    [Header("Skip Input")]
    public InputActionReference skipAction;

    [Tooltip("Seconds the player must hold the skip key. 0 = instant skip on press.")]
    [SerializeField] private float holdToSkipTime = 1f;

    [Header("Skip UI (optional)")]
    [SerializeField] private GameObject skipPromptRoot;
    [SerializeField] private Image skipFillImage;

    /* ── Runtime State ───────────────────────────────────── */

    public bool IsPlaying { get; private set; }
    public Cutscene CurrentCutscene { get; private set; }
    public float SkipProgress { get; private set; }

    private Cutscene _instance;
    private float _savedTimeScale;
    private bool _paused;

    /* ── Events ──────────────────────────────────────────── */

    public event Action<Cutscene> CutsceneStarted;
    public event Action<Cutscene> CutsceneEnded;
    public event Action<Cutscene> CutsceneSkipped;


    /* ── Unity ───────────────────────────────────────────── */

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (!IsPlaying || _instance == null) return;
        HandleSkip();
    }

    /* ── Public API ──────────────────────────────────────── */

    /// <summary>Instantiate and play a cutscene prefab.</summary>
    /// <param name="prefab">Cutscene prefab to instantiate.</param>
    /// <param name="pauseGame">Freeze Time.timeScale while playing.</param>
    public void Play(Cutscene prefab, bool pauseGame = true)
    {
        if (prefab == null || IsPlaying) return;

        // Pause
        if (pauseGame)
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _paused = true;
        }

        // Instantiate under this transform so it shares our lifetime
        _instance = Instantiate(prefab, transform);
        CurrentCutscene = _instance;
        IsPlaying = true;
        SkipProgress = 0f;

        _instance.Completed += OnCutsceneCompleted;
        _instance.Begin();

        if (skipPromptRoot) skipPromptRoot.SetActive(_instance.skippable);
        if (skipFillImage) skipFillImage.fillAmount = 0f;

        CutsceneStarted?.Invoke(_instance);
    }

    /// <summary>Force-end the current cutscene as if skipped.</summary>
    public void Skip()
    {
        if (!IsPlaying) return;
        Finish(skipped: true);
    }

    /// <summary>Force-end without setting the completion flag (e.g. scene unloading).</summary>
    public void Cancel()
    {
        if (!IsPlaying) return;
        Cleanup();
    }

    /* ── Internals ───────────────────────────────────────── */

    private void HandleSkip()
    {
        if (!_instance.skippable) return;

        if (holdToSkipTime <= 0f)
        {
            if (skipAction.action.IsPressed()) Finish(skipped: true);
            return;
        }

        if (skipAction.action.IsPressed())
        {
            SkipProgress += Time.unscaledDeltaTime / holdToSkipTime;
            if (skipFillImage) skipFillImage.fillAmount = SkipProgress;
            if (SkipProgress >= 1f) Finish(skipped: true);
        }
        else if (SkipProgress > 0f)
        {
            SkipProgress = Mathf.Max(0f, SkipProgress - Time.unscaledDeltaTime * 2f);
            if (skipFillImage) skipFillImage.fillAmount = SkipProgress;
        }
    }

    private void OnCutsceneCompleted()
    {
        Finish(skipped: false);
    }

    private void Finish(bool skipped)
    {
        if (!IsPlaying) return;

        var finished = _instance;

        // Set completion flag
        if (!string.IsNullOrEmpty(finished.completionFlag))
            FlagStateManager.Set(finished.completionFlag);

        Cleanup();

        if (skipped) CutsceneSkipped?.Invoke(finished);
        CutsceneEnded?.Invoke(finished);
    }

    private void Cleanup()
    {
        IsPlaying = false;
        SkipProgress = 0f;

        if (_instance != null)
        {
            _instance.Completed -= OnCutsceneCompleted;
            _instance.End();
            Destroy(_instance.gameObject);
        }

        _instance = null;
        CurrentCutscene = null;

        if (_paused)
        {
            Time.timeScale = _savedTimeScale;
            _paused = false;
        }

        if (skipPromptRoot) skipPromptRoot.SetActive(false);
        if (skipFillImage) skipFillImage.fillAmount = 0f;
    }
}