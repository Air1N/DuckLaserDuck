using UnityEngine;

public class GrubController : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private int popupDuration;
    [SerializeField] private int maxRandomPopupDurationOffset;

    [Header("Hide")]
    [SerializeField] private int hideDuration;
    [SerializeField] private int maxRandomHideDurationOffset;

    [Header("Teleport")]
    [SerializeField] private Transform teleportNear;
    [SerializeField] private float minTPDist;
    [SerializeField] private float maxTPDist;

    [Header("References")]
    [SerializeField] private ShooterDefault shooterDefault;
    [SerializeField] private Animator anim;
    [SerializeField] private HitManager hitManager;
    [SerializeField] private RoomManager roomManager;

    [Header("Gizmos")]
    [SerializeField] private Color minTPGizmoColor = new Color(0f, 1f, 0.5f, 0.4f);
    [SerializeField] private Color maxTPGizmoColor = new Color(0f, 0.5f, 1f, 0.4f);

    private enum GrubState { PoppedUp, HideStartup, Hidden }
    private GrubState state = GrubState.PoppedUp;

    private int tick = 0;
    private float originalFirerate;
    private int randomPopupDurationOffset;
    private int randomHideDurationOffset;

    private void Start()
    {
        roomManager = GameObject.Find("roomFloorType1").GetComponent<RoomManager>();
        originalFirerate = shooterDefault.firerate;

        randomPopupDurationOffset = RandomOffset(maxRandomPopupDurationOffset);
        randomHideDurationOffset = RandomOffset(maxRandomHideDurationOffset);
    }

    private void FixedUpdate()
    {
        if (hitManager.dead) return;

        tick++;

        switch (state)
        {
            case GrubState.PoppedUp: TickPoppedUp(); break;
            case GrubState.HideStartup: TickHideStartup(); break;
            case GrubState.Hidden: TickHidden(); break;
        }
    }

    private void TickPoppedUp()
    {
        if (tick > popupDuration + randomPopupDurationOffset)
            EnterHideStartup();
    }

    private void EnterHideStartup()
    {
        state = GrubState.HideStartup;
        tick = 0;

        shooterDefault.firerate = 0.0001f;
        shooterDefault.counter = 0;
        anim.SetBool("hide", true);
    }

    // future use: add a frame-counted delay here if the burrow animation needs time before the grub is fully hidden
    private void TickHideStartup()
    {
        state = GrubState.Hidden;
        tick = 0;
        randomHideDurationOffset = RandomOffset(maxRandomHideDurationOffset);
    }

    private void TickHidden()
    {
        if (tick > hideDuration + randomHideDurationOffset)
            EnterPoppedUp();
    }

    private void EnterPoppedUp()
    {
        state = GrubState.PoppedUp;
        tick = 0;

        roomManager.TeleportWithinRoom(gameObject, teleportNear, minTPDist, maxTPDist);

        shooterDefault.firerate = originalFirerate;
        shooterDefault.counter = 0;
        anim.SetBool("hide", false);

        randomPopupDurationOffset = RandomOffset(maxRandomPopupDurationOffset);
    }

    private int RandomOffset(int max) => Random.Range(-max, max);

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = teleportNear != null ? teleportNear.position : transform.position;

        Gizmos.color = minTPGizmoColor;
        Gizmos.DrawWireSphere(origin, minTPDist);

        Gizmos.color = maxTPGizmoColor;
        Gizmos.DrawWireSphere(origin, maxTPDist);
    }
}