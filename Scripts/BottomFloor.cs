using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomFloor : MonoBehaviour
{
    [SerializeField] private float yOffset;
    private Vector3 spawnPosition;
    [SerializeField] private float reboundCoeff;

    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private bool drawGizmos = false;

    [SerializeField] private Transform floorPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (floorPosition)
            spawnPosition = floorPosition.position;
        else spawnPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (transform.position.y < spawnPosition.y + yOffset)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x * 0.94f, rb2d.velocity.y * -reboundCoeff);

            if (rb2d.velocity.y < 1f)
            {
                rb2d.position = new Vector2(transform.position.x, spawnPosition.y + yOffset);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Vector3 drawGizPos = Vector3.zero;

        if (spawnPosition == Vector3.zero)
        {
            if (floorPosition)
                drawGizPos = floorPosition.position;
            else drawGizPos = transform.position;
        }

        Vector3 bottomLine = new Vector3(drawGizPos.x, drawGizPos.y + yOffset, drawGizPos.z);
        Gizmos.DrawLine(bottomLine + Vector3.left, bottomLine + Vector3.right);
    }
}
