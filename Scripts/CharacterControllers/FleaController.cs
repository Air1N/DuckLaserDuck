using Pathfinding;
using UnityEngine;

public class FleaController : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private int dashCooldown;
    [SerializeField] private int minRandomCooldownOffset;
    [SerializeField] private int maxRandomCooldownOffset;

    [Header("Timing")]
    [SerializeField] private int dashStartup;
    [SerializeField] private int dashDuration;
    [SerializeField] private int betweenDashPause;

    [Header("Dash 1")]
    [SerializeField] private float dash1Distance = 10f;

    [Header("Dash 2")]
    [SerializeField] private float dash2Distance = 5f;

    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerLocator;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private ControlLeftRightFlip controlLeftRightFlip;

    [Header("Gizmos")]
    [SerializeField] private Color dash1GizmoColor = new Color(1f, 0.5f, 0f, 0.4f);
    [SerializeField] private Color dash2GizmoColor = new Color(1f, 1f, 0f, 0.4f);

    private enum FleaState { Walking, Dash1Startup, Dash1Active, BetweenPause, Dash2Startup, Dash2Active }
    private FleaState state = FleaState.Walking;

    private int tick = 0;
    private Vector3 dashDirNormal;
    private float currentDashDistance;
    private float startMoveSpeed;
    private int randomCDOffset = 0;

    private float currentDashForce;

    private void Start()
    {
        startMoveSpeed = aiPath.maxSpeed;
        randomCDOffset = Random.Range(minRandomCooldownOffset, maxRandomCooldownOffset);

        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }

    private void FixedUpdate()
    {
        tick++;

        switch (state)
        {
            case FleaState.Walking: TickWalking(); break;
            case FleaState.Dash1Startup: TickDash1Startup(); break;
            case FleaState.Dash1Active: TickDash1Active(); break;
            case FleaState.BetweenPause: TickBetweenPause(); break;
            case FleaState.Dash2Startup: TickDash2Startup(); break;
            case FleaState.Dash2Active: TickDash2Active(); break;
        }
    }

    private void TickWalking()
    {
        if (tick > dashCooldown + randomCDOffset)
            EnterDash1Startup();
    }

    private void EnterDash1Startup()
    {
        state = FleaState.Dash1Startup;
        tick = 0;

        rb.velocity = Vector2.zero;
        aiPath.maxSpeed = 0f;
        controlLeftRightFlip.stopped = true;
        anim.SetBool("dash", true);

        AimAtPlayer();
        currentDashDistance = dash1Distance;
        currentDashForce = CalculateDashForce(dash1Distance);
        ShowLine();
    }

    private void TickDash1Startup()
    {
        UpdateLineWorld();

        if (tick > dashStartup)
        {
            state = FleaState.Dash1Active;
            tick = 0;
            HideLine();
        }
    }

    private void TickDash1Active()
    {
        rb.AddForce(dashDirNormal * currentDashForce, ForceMode2D.Force);

        if (tick > dashDuration)
        {
            rb.velocity = Vector2.zero;
            state = FleaState.BetweenPause;
            tick = 0;
            anim.SetBool("dash", false);
            controlLeftRightFlip.stopped = false;
        }
    }

    private void TickBetweenPause()
    {
        if (tick > betweenDashPause)
            EnterDash2Startup();
    }

    private void EnterDash2Startup()
    {
        state = FleaState.Dash2Startup;
        tick = 0;

        rb.velocity = Vector2.zero;
        controlLeftRightFlip.stopped = true;
        anim.SetBool("dash", true);

        AimAtPlayer();
        currentDashDistance = dash2Distance;
        currentDashForce = CalculateDashForce(dash2Distance);
        ShowLine();
    }

    private void TickDash2Startup()
    {
        UpdateLineWorld();

        if (tick > dashStartup)
        {
            state = FleaState.Dash2Active;
            tick = 0;
            HideLine();
        }
    }

    private void TickDash2Active()
    {
        rb.AddForce(dashDirNormal * currentDashForce, ForceMode2D.Force);

        if (tick > dashDuration)
        {
            rb.velocity = Vector2.zero;
            aiPath.maxSpeed = startMoveSpeed;
            controlLeftRightFlip.stopped = false;
            anim.SetBool("dash", false);

            state = FleaState.Walking;
            tick = 0;
            randomCDOffset = Random.Range(minRandomCooldownOffset, maxRandomCooldownOffset);
        }
    }

    private float CalculateDashForce(float distance)
    {
        float mass = rb.mass;
        float drag = rb.drag;
        float dt = Time.fixedDeltaTime;

        if (drag > 0f)
        {
            float decayPerTick = 1f - drag * dt;
            decayPerTick = Mathf.Clamp(decayPerTick, 0f, 1f);
            float geometricSum = decayPerTick * (1f - Mathf.Pow(decayPerTick, dashDuration)) / (1f - decayPerTick);
            return (distance * mass) / (dt * dt * geometricSum);
        }

        return (distance * mass) / (dt * dt * dashDuration);
    }

    private void AimAtPlayer()
    {
        Vector3 dir = playerLocator.position - transform.position;
        dashDirNormal = dir.normalized;
    }

    private void ShowLine()
    {
        lineRenderer.enabled = true;
        UpdateLineWorld();
    }

    private void UpdateLineWorld()
    {
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(Vector2)(dashDirNormal * currentDashDistance);
        lineRenderer.SetPositions(new Vector3[] { start, end });
    }

    private void HideLine()
    {
        lineRenderer.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = dash1GizmoColor;
        Gizmos.DrawWireSphere(transform.position, dash1Distance);

        Gizmos.color = dash2GizmoColor;
        Gizmos.DrawWireSphere(transform.position, dash2Distance);
    }
}