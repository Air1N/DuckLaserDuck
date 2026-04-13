using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public Transform player;
    public float openHeight;
    public float openRange;

    public Vector3 originalPosition;
    public RoomManager roomManager;
    public float lerpSpeed;
    public float fadeSpeed;

    public bool entered;

    public SpriteRenderer prophRend;

    public Collider2D col;

    bool locked = false;

    public bool activated = false;

    void Start()
    {
        player = GameObject.Find("playerFollower").transform;
        originalPosition = transform.position;
        col = transform.GetComponent<Collider2D>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (roomManager.roomCleared) locked = false;
        else
        {
            locked = true;

            transform.position = originalPosition;
            col.enabled = true;
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(1f, 1f, fadeSpeed * Time.fixedDeltaTime));
            transform.GetComponent<SpriteRenderer>().color = newColor;
        }

        if (locked) return;
        float alpha = transform.GetComponent<SpriteRenderer>().color.a;

        if (Vector3.Distance(player.position, originalPosition) < openRange)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition + new Vector3(0f, openHeight, 0f), lerpSpeed * Time.fixedDeltaTime);
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, 0f, fadeSpeed * Time.fixedDeltaTime));
            transform.GetComponent<SpriteRenderer>().color = newColor;
            col.enabled = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, lerpSpeed * Time.fixedDeltaTime);
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, 1f, fadeSpeed * Time.fixedDeltaTime));
            transform.GetComponent<SpriteRenderer>().color = newColor;
            if (Vector3.Distance(transform.position, originalPosition) < 0.5f && col.enabled == false) col.enabled = true;
        }
    }
}
