using Pathfinding;
using UnityEngine;

public class CrabController : MonoBehaviour
{
    public enum CrabState { Strafing, ChargeWindup, Charging, Recovery }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RotateAimToward rotateAimToward;
    [SerializeField] private HitManager hitManager;

    [Header("Line Renderer (child, local space, points local-right)")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Strafe")]
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private int strafeMinTicks = 100;
    [SerializeField] private int strafeMaxTicks = 180;
    [SerializeField, Range(0f, 1f)] private float circleWeight = 0.75f;
    [SerializeField] private int dirChangeMinTicks = 40;
    [SerializeField] private int dirChangeMaxTicks = 80;

    [Header("Charge Windup")]
    [SerializeField] private int windupTicks = 50;

    [Header("Charge")]
    [SerializeField] private float chargeForce = 200f;
    [SerializeField] private float chargeDistance = 15f;
    [SerializeField] private float maxChargeSpeed = 18f;
    [SerializeField] private int maxChargeTicks = 300;

    [Header("Claw Attack")]
    [SerializeField] private float clawFireDistance = 4f;
    [SerializeField] private int clawMoveId = 0;
    [SerializeField] private int clawMoveIdHoldTicks = 4;

    [Header("Recovery")]
    [SerializeField] private int recoveryTicks = 40;
    [SerializeField] private float brakeDrag = 0.92f;

    [Header("Debug")]
    [SerializeField] private CrabState currentState;

    // internal
    private CrabState state;
    private int tick;
    private int strafeDuration;
    private int circleDir;
    private int nextDirChangeTick;
    private Vector3 chargeDir;
    private Vector3 chargeStartPos;
    private bool clawsFired;
    private int clawsFiredTick;

    private GameObject strafePointGO;
    private Transform strafePoint;

    void Start()
    {
        strafePointGO = new GameObject(name + "_StrafePt");
        strafePoint = strafePointGO.transform;

        if (lineRenderer)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.enabled = false;
        }

        EnterStrafe();
    }

    void OnDestroy()
    {
        if (strafePointGO) Destroy(strafePointGO);
    }

    void FixedUpdate()
    {
        if (hitManager && hitManager.dead) return;

        tick++;
        currentState = state;

        switch (state)
        {
            case CrabState.Strafing: TickStrafe(); break;
            case CrabState.ChargeWindup: TickWindup(); break;
            case CrabState.Charging: TickCharge(); break;
            case CrabState.Recovery: TickRecovery(); break;
        }
    }

    /* ── STRAFE ───────────────────────────────── */

    void EnterStrafe()
    {
        state = CrabState.Strafing;
        tick = 0;
        strafeDuration = Random.Range(strafeMinTicks, strafeMaxTicks);
        circleDir = Random.value > 0.5f ? 1 : -1;
        nextDirChangeTick = Random.Range(dirChangeMinTicks, dirChangeMaxTicks);

        aiPath.canMove = true;
        aiPath.maxSpeed = strafeSpeed;

        if (rotateAimToward) rotateAimToward.enabled = true;
        if (lineRenderer) lineRenderer.enabled = false;

        anim.SetInteger("move_id", -1);
        anim.SetBool("charge_windup", false);
    }

    void TickStrafe()
    {
        if (tick >= nextDirChangeTick)
        {
            circleDir *= -1;
            nextDirChangeTick = tick + Random.Range(dirChangeMinTicks, dirChangeMaxTicks);
        }

        Vector3 toPlayer = playerTransform.position - transform.position;
        float dist = toPlayer.magnitude;
        Vector3 toPlayerDir = dist > 0.001f ? toPlayer / dist : Vector2.right;
        Vector3 perpDir = new Vector3(-toPlayerDir.y, toPlayerDir.x, 0f) * circleDir;

        Vector3 desired;
        if (dist < preferredDistance * 0.6f)
            desired = Vector3.Lerp(-toPlayerDir, perpDir, 0.3f).normalized;
        else if (dist > preferredDistance * 1.4f)
            desired = Vector3.Lerp(toPlayerDir, perpDir, 0.4f).normalized;
        else
            desired = Vector3.Lerp(-toPlayerDir, perpDir, circleWeight).normalized;

        strafePoint.position = transform.position + desired * preferredDistance;
        destinationSetter.target = strafePoint;

        if (tick >= strafeDuration)
            EnterWindup();
    }

    /* ── WINDUP ───────────────────────────────── */

    void EnterWindup()
    {
        state = CrabState.ChargeWindup;
        tick = 0;

        aiPath.canMove = false;
        aiPath.maxSpeed = 0f;
        rb.velocity = Vector2.zero;

        if (rotateAimToward) rotateAimToward.enabled = true;

        UpdateLineEndpoint();
        if (lineRenderer) lineRenderer.enabled = true;

        anim.SetBool("charge_windup", true);
    }

    void TickWindup()
    {
        if (tick >= windupTicks)
            EnterCharge();
    }

    /* ── CHARGE ───────────────────────────────── */

    void EnterCharge()
    {
        state = CrabState.Charging;
        tick = 0;
        clawsFired = false;

        chargeDir = (playerTransform.position - transform.position).normalized;
        chargeStartPos = transform.position;

        if (rotateAimToward) rotateAimToward.enabled = true;
        if (lineRenderer) lineRenderer.enabled = false;

        anim.SetBool("charge_windup", false);
    }

    void TickCharge()
    {
        rb.AddForce(chargeDir * chargeForce, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxChargeSpeed)
            rb.velocity = rb.velocity.normalized * maxChargeSpeed;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (!clawsFired && distToPlayer <= clawFireDistance)
        {
            anim.SetInteger("move_id", clawMoveId);
            clawsFired = true;
            clawsFiredTick = tick;
        }

        if (clawsFired && tick >= clawsFiredTick + clawMoveIdHoldTicks)
            anim.SetInteger("move_id", -1);

        float traveled = Vector2.Distance(chargeStartPos, transform.position);
        if (traveled >= chargeDistance || tick >= maxChargeTicks)
            EnterRecovery();
    }

    /* ── RECOVERY ─────────────────────────────── */

    void EnterRecovery()
    {
        state = CrabState.Recovery;
        tick = 0;

        anim.SetInteger("move_id", -1);
        if (lineRenderer) lineRenderer.enabled = false;
    }

    void TickRecovery()
    {
        rb.velocity *= brakeDrag;

        if (tick >= recoveryTicks)
            EnterStrafe();
    }

    /* ── LINE HELPERS ─────────────────────────── */

    void UpdateLineEndpoint()
    {
        if (!lineRenderer) return;
        float scaleX = Mathf.Abs(lineRenderer.transform.lossyScale.x);
        if (scaleX < 0.001f) scaleX = 1f;
        float localLength = chargeDistance / scaleX;
        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.right * localLength });
    }

    /* ── GIZMOS ───────────────────────────────── */

    void OnDrawGizmosSelected()
    {
        if (playerTransform)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, preferredDistance);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(playerTransform.position, preferredDistance * 0.6f);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(playerTransform.position, preferredDistance * 1.4f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, clawFireDistance);

        if (Application.isPlaying && state == CrabState.Charging)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(chargeStartPos, chargeDistance);
            Gizmos.DrawLine(chargeStartPos, chargeStartPos + chargeDir * chargeDistance);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(chargeStartPos, transform.position);
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, chargeDistance);
        }
    }
}