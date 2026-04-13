using Pathfinding;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    public HitManager hitManager;
    public Transform playerLocator;
    public Transform player;
    public ShooterDefault shooterScript;
    public AIPath aiPath;

    public int shootStopDuration;
    public int shootStopOffset;
    public bool stopped;
    public int stopCounter;

    private bool canHitPlayer;

    [SerializeField] private AnimateWhenWalking animateWhenWalking;
    [SerializeField] private ControlLeftRightFlip controlLeftRightFlip;

    void Start()
    {
        aiPath.maxSpeed = moveSpeed;
        player = GameObject.Find("playerFollower").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, dirToPlayer, 20f, LayerMask.GetMask("ObstacleLayer"));

        if (hitInfo.collider != null) canHitPlayer = false;
        else canHitPlayer = true;

        if (!stopped && shooterScript.targetInFireRange && canHitPlayer && shooterScript.counter > 50 / shooterScript.firerate - shootStopOffset)
            stopped = true;

        if (!stopped && (!shooterScript.targetInFireRange || !canHitPlayer) && shooterScript.counter > 50 / shooterScript.firerate - shootStopOffset)
        {
            shooterScript.counter = (int)(50 / shooterScript.firerate) - shootStopOffset;
        }

        if (stopped)
        {
            stopCounter++;
            aiPath.maxSpeed = 0f;
            shooterScript.autoAim = false;
            shooterScript.overrideAimedCorrectly = true;

            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            stopCounter = 0;

            aiPath.maxSpeed = moveSpeed;
            shooterScript.autoAim = true;
            shooterScript.overrideAimedCorrectly = false;

            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (aiPath.remainingDistance <= aiPath.endReachedDistance)
            {
                Vector3 verticalOffset = Vector3.down * Random.Range(0f, 1f) * 10f; // Move the frog down slowly, to avoid standing still when the player is out of shooting angles
                Vector3 horizontalOffset = Vector3.right * Random.Range(-1f, 1f);

                transform.position = Vector2.Lerp(transform.position, transform.position - playerLocator.position + verticalOffset + horizontalOffset, 0.001f);
            }
        }

        if (stopCounter > shootStopDuration)
        {
            stopCounter = 0;

            stopped = false;
        }

        controlLeftRightFlip.stopped = stopped;
        animateWhenWalking.stopped = stopped;
    }
}
