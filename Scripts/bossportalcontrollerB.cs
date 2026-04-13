using UnityEngine;

public class BossPortalControllerB : MonoBehaviour // B
{
    public enum PortalState
    {
        Inactive,
        SlowPulse,
        Accelerating,
        RapidPulse,
        Climax,
        Finished
    }

    [System.Serializable]
    public struct PhaseSettings
    {
        public int duration;
        public int beatInterval;
        public float pulseIntensity;
        public int beatsPerSpurt;
        public int bloodMin;
        public int bloodMax;
        public float speedMultiplier;
    }

    [Header("References")]
    [SerializeField] private Transform heartTransform;
    [SerializeField] private ParticleSystem bloodParticles;
    [SerializeField] private FadeSpriteTransparencyForSeconds fadeSprite;
    [SerializeField] private FadeLightTransparencyForSeconds fadeLight;
    [SerializeField] private DestroyAfterSeconds destroyAfterSeconds;
    [SerializeField] private SpawnPrefab spawnPrefab;

    [Header("Beat Animation")]
    [SerializeField] private AnimationCurve beatCurve;
    [SerializeField] private int beatAnimLength = 20;
    [SerializeField][Range(0f, 1f)] private float beatTimingRandomness = 0.15f;
    [SerializeField][Range(0f, 1f)] private float bloodAmountRandomness = 0.3f;

    [Header("Phases")]
    [SerializeField]
    private PhaseSettings slow = new PhaseSettings
    {
        duration = 300,
        beatInterval = 55,
        pulseIntensity = 0.12f,
        beatsPerSpurt = 5,
        bloodMin = 4,
        bloodMax = 8,
        speedMultiplier = 1f
    };
    [SerializeField] private int acceleratingDuration = 350;
    [SerializeField]
    private PhaseSettings fast = new PhaseSettings
    {
        duration = 150,
        beatInterval = 12,
        pulseIntensity = 0.25f,
        beatsPerSpurt = 1,
        bloodMin = 8,
        bloodMax = 16,
        speedMultiplier = 2f
    };
    [SerializeField]
    private PhaseSettings climax = new PhaseSettings
    {
        duration = 120,
        beatInterval = 4,
        pulseIntensity = 0.45f,
        beatsPerSpurt = 1,
        bloodMin = 18,
        bloodMax = 35,
        speedMultiplier = 6f
    };

    [Header("Climax Stream")]
    [SerializeField] private int streamMinInterval = 2;
    [SerializeField] private int streamMaxInterval = 5;
    [SerializeField] private int streamMinAmount = 6;
    [SerializeField] private int streamMaxAmount = 14;

    [Header("Finish Burst")]
    [SerializeField] private int finishBurstMin = 50;
    [SerializeField] private int finishBurstMax = 80;

    [Header("Options")]
    [SerializeField] private bool activateOnStart;

    private PortalState state = PortalState.Inactive;
    private int tick;
    private int beatTimer;
    private int currentBeatInterval;
    private int beatAnimTick;
    private int beatsUntilBlood;
    private int streamTimer;
    private int nextStreamInterval;
    private bool beating;
    private bool beatTriggered;
    private float currentIntensity;
    private Vector3 baseScale;
    private ParticleSystem.MainModule bloodMain;

    public PortalState State => state;

    private void Reset()
    {
        beatCurve = new AnimationCurve(
            new Keyframe(0.00f, 0.00f, 0f, 8f),
            new Keyframe(0.10f, 1.00f, 0f, -6f),
            new Keyframe(0.22f, -0.20f, 0f, 4f),
            new Keyframe(0.38f, 0.45f, 0f, -3f),
            new Keyframe(0.58f, 0.00f, -1f, 0f),
            new Keyframe(1.00f, 0.00f, 0f, 0f)
        );
    }

    private void Start()
    {
        bloodMain = bloodParticles.main;
        baseScale = heartTransform != null ? heartTransform.localScale : Vector3.one;

        if (activateOnStart)
            Activate();
    }

    public void Activate()
    {
        state = PortalState.SlowPulse;
        tick = 0;
        beatTimer = 0;
        beatAnimTick = 0;
        beating = false;
        currentBeatInterval = slow.beatInterval;
        beatsUntilBlood = slow.beatsPerSpurt;
        baseScale = heartTransform != null ? heartTransform.localScale : Vector3.one;
    }

    private void FixedUpdate()
    {
        if (state == PortalState.Inactive || state == PortalState.Finished)
            return;

        tick++;
        beatTriggered = false;

        switch (state)
        {
            case PortalState.SlowPulse:
                TickPhase(slow, PortalState.Accelerating);
                break;
            case PortalState.Accelerating:
                TickLerped(slow, fast, acceleratingDuration, PortalState.RapidPulse);
                break;
            case PortalState.RapidPulse:
                TickPhase(fast, PortalState.Climax);
                break;
            case PortalState.Climax:
                TickClimax();
                break;
        }

        UpdateHeartScale();
    }

    // ─── Phase Tickers ────────────────────────────────────────────────────

    private void TickPhase(PhaseSettings settings, PortalState next)
    {
        ApplySettings(settings);
        if (tick >= settings.duration) EnterPhase(next);
    }

    private void TickLerped(PhaseSettings from, PhaseSettings to, int duration, PortalState next)
    {
        float t = Mathf.Clamp01((float)tick / duration);
        ApplySettings(LerpSettings(from, to, t));
        if (tick >= duration) EnterPhase(next);
    }

    private void TickClimax()
    {
        float t = Mathf.Clamp01((float)tick / climax.duration);
        ApplySettings(LerpSettings(fast, climax, t));

        streamTimer++;
        if (streamTimer >= nextStreamInterval)
        {
            streamTimer = 0;
            nextStreamInterval = Random.Range(streamMinInterval, streamMaxInterval + 1);
            EmitBlood(streamMinAmount, streamMaxAmount);
        }

        if (tick >= climax.duration) Finish();
    }

    // ─── Core Mechanics ───────────────────────────────────────────────────

    private void ApplySettings(PhaseSettings s)
    {
        StepBeat(s.beatInterval, s.pulseIntensity);
        StepSporadicBlood(s.beatsPerSpurt, s.bloodMin, s.bloodMax);
        bloodMain.startSpeedMultiplier = s.speedMultiplier;
    }

    private void StepBeat(int baseInterval, float intensity)
    {
        beatTimer++;
        if (beatTimer < currentBeatInterval) return;

        int jitter = Mathf.RoundToInt(baseInterval * beatTimingRandomness);
        currentBeatInterval = Mathf.Max(2, baseInterval + Random.Range(-jitter, jitter + 1));
        beatTimer = 0;
        beatAnimTick = 0;
        beating = true;
        beatTriggered = true;
        currentIntensity = intensity;
    }

    private void StepSporadicBlood(int beatsPerSpurt, int minAmount, int maxAmount)
    {
        if (!beatTriggered) return;

        beatsUntilBlood--;
        if (beatsUntilBlood > 0) return;

        EmitBlood(minAmount, maxAmount);
        int jitter = Mathf.RoundToInt(beatsPerSpurt * beatTimingRandomness);
        beatsUntilBlood = Mathf.Max(1, beatsPerSpurt + Random.Range(-jitter, jitter + 1));
    }

    private void UpdateHeartScale()
    {
        if (heartTransform == null) return;

        if (!beating)
        {
            heartTransform.localScale = baseScale;
            return;
        }

        float progress = Mathf.Clamp01((float)beatAnimTick / beatAnimLength);
        heartTransform.localScale = baseScale * (1f + beatCurve.Evaluate(progress) * currentIntensity);
        beatAnimTick++;

        if (beatAnimTick > beatAnimLength)
            beating = false;
    }

    private void EmitBlood(int minAmount, int maxAmount)
    {
        if (bloodParticles == null) return;

        int baseCount = Random.Range(minAmount, maxAmount + 1);
        int variance = Mathf.RoundToInt(baseCount * bloodAmountRandomness);
        bloodParticles.Emit(Mathf.Max(1, baseCount + Random.Range(-variance, variance + 1)));
    }

    // ─── Utility ──────────────────────────────────────────────────────────

    private static PhaseSettings LerpSettings(PhaseSettings a, PhaseSettings b, float t)
    {
        return new PhaseSettings
        {
            beatInterval = Mathf.RoundToInt(Mathf.Lerp(a.beatInterval, b.beatInterval, t)),
            pulseIntensity = Mathf.Lerp(a.pulseIntensity, b.pulseIntensity, t),
            beatsPerSpurt = Mathf.RoundToInt(Mathf.Lerp(a.beatsPerSpurt, b.beatsPerSpurt, t)),
            bloodMin = Mathf.RoundToInt(Mathf.Lerp(a.bloodMin, b.bloodMin, t)),
            bloodMax = Mathf.RoundToInt(Mathf.Lerp(a.bloodMax, b.bloodMax, t)),
            speedMultiplier = Mathf.Lerp(a.speedMultiplier, b.speedMultiplier, t)
        };
    }

    private void EnterPhase(PortalState phase)
    {
        state = phase;
        tick = 0;

        if (phase == PortalState.Climax)
        {
            streamTimer = 0;
            nextStreamInterval = Random.Range(streamMinInterval, streamMaxInterval + 1);
        }
    }

    private void Finish()
    {
        EmitBlood(finishBurstMin, finishBurstMax);

        if (spawnPrefab != null) spawnPrefab.enabled = true;
        if (fadeSprite != null) fadeSprite.enabled = true;
        if (fadeLight != null) fadeLight.enabled = true;
        if (destroyAfterSeconds != null) destroyAfterSeconds.enabled = true;

        state = PortalState.Finished;
        heartTransform.localScale = baseScale;
    }
}