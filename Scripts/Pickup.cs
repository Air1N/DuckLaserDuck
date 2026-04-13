using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Pickup : MonoBehaviour
{
    [SerializeField] public int maxHealth;
    [SerializeField] public int health;
    [SerializeField] public int worms;
    [SerializeField] public int greenGrubs;
    [SerializeField] private HitManager playerHitManager;
    [SerializeField] private PlayerBankManager playerBankManager;
    [SerializeField] public float destroyDelay;

    [SerializeField] private TrackTransform trackTransform;

    [SerializeField] private BottomFloor bottomFloor;
    [SerializeField] private Rigidbody2D rb2D;

    [SerializeField] private bool stopAnimation = true;

    [SerializeField] private Animator animator;
    [SerializeField] private List<AudioClip> audioClips;

    private bool pickedUp = false;

    [SerializeField] private float soundOffsetMin = 0.1f;
    [SerializeField] private float soundOffsetMax = 0.4f;


    private void Start()
    {
        playerBankManager = GameObject.Find("char").GetComponent<PlayerBankManager>();
        playerHitManager = GameObject.Find("char").GetComponent<HitManager>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || pickedUp) return;
        StartPickingUp();
    }

    public void StartPickingUp()
    {
        if (pickedUp) return;

        transform.SetParent(null);
        StartTrackingPlayer();
        RemoveBottomFloor();
        DisableGravity();

        if (stopAnimation) StopAnimation();

        pickedUp = true;

        if (destroyDelay >= 0) StartCoroutine(DestroyAfterSeconds());
    }

    private void StartTrackingPlayer()
    {
        trackTransform.enabled = true;
    }

    private void RemoveBottomFloor()
    {
        bottomFloor.enabled = false;
    }

    private void DisableGravity()
    {
        rb2D.gravityScale = 0f;
    }

    private void StopAnimation()
    {
        animator.transform.localPosition = new Vector3(0f, 0f, 0f);
        animator.enabled = false;
    }

    private IEnumerator DestroyAfterSeconds()
    {
        float soundOffset = Random.Range(soundOffsetMin, soundOffsetMax);
        yield return new WaitForSecondsRealtime(destroyDelay - soundOffset);

        int choice = Random.Range(0, audioClips.Count);
        GetComponent<AudioSource>().PlayOneShot(audioClips[choice]);

        yield return new WaitForSecondsRealtime(soundOffset);

        playerBankManager.worms += worms;
        playerBankManager.greenGrubs += greenGrubs;

        playerHitManager.maxHealth += maxHealth;
        playerHitManager.health += health;

        Destroy(gameObject);
    }
}
