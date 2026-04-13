using UnityEngine;

public class CameraFollow : MonoBehaviour
{
public Transform player;
public Vector3 offset;

public float lerpSpeed;

void FixedUpdate ()
{
        transform.position = Vector3.Lerp(transform.position, new Vector3 (player.position.x + offset.x, player.position.y + offset.y, offset.z), lerpSpeed * Time.fixedDeltaTime);
}
}
