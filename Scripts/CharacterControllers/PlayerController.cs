using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;
    public float dashSpeed;

    public int dashCooldown;
    int dashCounter;
    public bool canDash;

    public Rigidbody2D rb;
    public Animator anim;

    public Transform projectileSpawnPoint;

    public PlayerUpgradesManager upgradeManager;

    private Vector2 movement;
    private Vector2 aimDir;
    private bool facingRight = false;
    public bool shooting = false;

    Vector3 relativeTarget;

    GameObject headRotation;

    public HitManager hitManager;
    public GameObject enemyPrefab;

    public int summontime;
    int summon = 0;

    public GameObject retryMenu;
    public GameObject pauseMenu;

    public Transform recoilSubject;
    public float recoilResetSlerpSpeed;

    public Vector3 aimPoint;

    Vector3 originalRecoilPosition;

    bool paused;

    public bool dashing = false;
    public bool dashButtonPressed = false;

    int dashAnimTimer = 0;
    float deadTick = 0;

    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private UpgradeMenuController upgradeMenuController;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameObject inventoryScreen;

    public bool inMenu = false;
    public bool lastDeviceGamepad = false;

    public int standingStillTimer = 0;
    public int movingTimer = 0;
    public int notShootingTimer = 0;
    public int isShootingTimer = 0;

    public Vector3 currentMoveAmount;

    public int haltActive = 0;
    public int stallBurstActive = 0;

    void Start()
    {
        Physics2D.IgnoreLayerCollision(9, 10);
        headRotation = GameObject.Find("HeadRotation");

        originalRecoilPosition = recoilSubject.localPosition;
        mainMenuController = FindObjectOfType<MainMenuController>();
    }

    void Update()
    {
        if (hitManager.dead && !retryMenu.activeSelf)
        {
            deadTick += Time.unscaledDeltaTime;

            if (deadTick >= 3)
            {
                Time.timeScale = 2f;
                retryMenu.SetActive(true);
                deadTick = 0;
            }
            else if (deadTick > 0.1f)
            {
                Time.timeScale = 0f;
            }
        }

        if (lastDeviceGamepad && mapManager.gameObject.activeSelf)
        {
            mapManager.contentTransform.position += new Vector3(movement.x * 0.1f, -movement.y, 0f) * Time.unscaledDeltaTime * 210f;
            mapManager.contentTransform.position += new Vector3(aimDir.x * 0.1f, -aimDir.y, 0f) * Time.unscaledDeltaTime * 210f;
        }

        if (inMenu && lastDeviceGamepad) Cursor.visible = false;
        else Cursor.visible = true;
    }

    void FixedUpdate()
    {
        standingStillTimer++;
        movingTimer++;

        notShootingTimer++;
        isShootingTimer++;


        if (movement == Vector2.zero) movingTimer = 0;
        else standingStillTimer = 0;

        if (shooting) notShootingTimer = 0;
        else isShootingTimer = 0;

        if (dashAnimTimer > 0) dashAnimTimer--;
        // Check if the player is able to dash
        if (!canDash)
        {
            // If the player can't dash, increment the dash counter
            dashCounter++;
        }


        if (notShootingTimer > 3 * 50)
            stallBurstActive = 0;
        else stallBurstActive++;

        if (standingStillTimer > 3 * 50)
            haltActive = 0;
        else haltActive++;

        // If the dash counter is greater than the dash cooldown, reset the counter and allow the player to dash
        float realDashCooldown = (dashCooldown + upgradeManager.dashCooldownModifierFlat) * upgradeManager.dashCooldownModifierMult;
        if (dashCounter > realDashCooldown)
        {
            canDash = true;
            dashCounter = 0;
        }

        if (dashAnimTimer == 0) dashing = false;

        anim.SetBool("dash", dashing);

        // Increment the summon counter
        summon++;

        // If the summon counter is greater than the summon time, reset the counter and summon an enemy
        if (summon > summontime)
        {
            summon = 0;
            Instantiate(enemyPrefab, new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f), Quaternion.identity);
        }

        // Check if the cursor is to the right of the player
        bool isCursorRight = (aimPoint.x - transform.position.x) > 0;

        // If the cursor is to the right and the player is not facing right, flip the player
        if (isCursorRight && !facingRight)
        {
            Flip();
        }
        // If the cursor is not to the right and the player is facing right, flip the player
        else if (!isCursorRight && facingRight)
        {
            Flip();
        }

        float realMoveSpeed = moveSpeed + upgradeManager.playerMoveSpeedModifier;
        if (upgradeManager.inertiaAcceleration > 0f) realMoveSpeed *= Mathf.Min(movingTimer / 50f, 2f);

        realMoveSpeed = Mathf.Max(5f, realMoveSpeed);

        currentMoveAmount = movement * realMoveSpeed;
        // Add a force to the player's rigidbody in the direction of movement
        rb.AddForce(movement * realMoveSpeed, ForceMode2D.Force);

        // Slerp the recoil subject's position towards its original position
        Vector3 slerpPos = Vector3.Slerp(recoilSubject.localPosition, originalRecoilPosition, recoilResetSlerpSpeed);
        recoilSubject.localPosition = slerpPos;
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector2 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;

        movement = context.ReadValue<Vector2>().normalized;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        anim.SetFloat("velocity", Mathf.Abs(movement.x) + Mathf.Abs(movement.y));
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        if (context.ReadValueAsButton())
        {
            if (canDash) dashing = true;
            dashButtonPressed = true;
        }
        else
        {
            dashButtonPressed = false;
            return;
        }

        if (canDash)
        {
            rb.AddForce(movement * dashSpeed, ForceMode2D.Impulse);
            canDash = false;
            dashAnimTimer = 8;
        }
    }

    public void OnAimKey(InputAction.CallbackContext context)
    {
        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        Vector2 mousePos = context.ReadValue<Vector2>();
        aimPoint = Camera.main.ScreenToWorldPoint(mousePos);
        aimPoint.z = 0f;
        relativeTarget = aimPoint - headRotation.transform.position;

        float rotZ = Mathf.Atan2(relativeTarget.y, relativeTarget.x) * Mathf.Rad2Deg;

        Quaternion toQuaternion = Quaternion.Euler(0, 0, rotZ + 180 * (facingRight ? -1 : 0));
        headRotation.transform.rotation = Quaternion.Slerp(headRotation.transform.rotation, toQuaternion * Quaternion.Euler(0f, 0f, 25f * (System.Convert.ToInt32(facingRight) * 2 - 1)), 500f * Time.deltaTime);
    }

    public void OnAimStick(InputAction.CallbackContext context)
    {

        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        aimDir = context.ReadValue<Vector2>().normalized;
        if (aimDir.x == 0 && aimDir.y == 0) return;

        if (inMenu) return;


        Vector2 screenspaceTarget = Camera.main.WorldToScreenPoint(headRotation.transform.position + (Vector3)aimDir * 5f);
        Mouse.current.WarpCursorPosition(screenspaceTarget);

        //Vector3.right if you have a sprite rotated in the right direction
        Quaternion toQuaternion = Quaternion.FromToRotation(Vector3.right * (facingRight ? -1 : 1), relativeTarget);
        headRotation.transform.rotation = Quaternion.Slerp(headRotation.transform.rotation, toQuaternion * Quaternion.Euler(0f, 0f, 25f * (System.Convert.ToInt32(facingRight) * 2 - 1)), 500f * Time.deltaTime);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        if (context.ReadValueAsButton()) shooting = true;
        else shooting = false;

        upgradeMenuController.shooting = shooting;
        anim.SetBool("shooting", shooting);
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        if (hitManager.dead) return;
        if (!context.performed && !context.canceled) return;
        if (inMenu) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        if (context.ReadValueAsButton())
        {
            paused = !paused;
            inMenu = paused;

            Time.timeScale = System.Convert.ToInt32(!paused);
            pauseMenu.SetActive(paused);
            mainMenuController.OpenPauseMenu();
        }
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (!context.performed && !context.canceled) return;

        if (context.control.device is Gamepad) lastDeviceGamepad = true;
        else lastDeviceGamepad = false;

        if (context.ReadValueAsButton())
        {
            inventoryScreen.SetActive(true);
        }
        else
        {
            inventoryScreen.SetActive(false);
        }
    }

    public void OnDeviceLost(PlayerInput playerInput)
    {
        paused = true;
        inMenu = paused;

        Time.timeScale = System.Convert.ToInt32(!paused);
        pauseMenu.SetActive(paused);
        mainMenuController.OpenPauseMenu();
    }
}
