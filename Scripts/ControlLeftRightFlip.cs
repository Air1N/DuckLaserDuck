using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class ControlLeftRightFlip : MonoBehaviour
{
    [SerializeField] private bool facingRight = true;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Transform playerTracker;

    [SerializeField] private bool velocityBased = true;
    [SerializeField] private bool targetDirectionBased = false;

    public bool stopped = false;
    private bool justTurned = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (justTurned)
        {
            justTurned = false;
            return;
        }

        if (stopped) return;

        if (velocityBased)
        {
            if (aiPath.desiredVelocity.x > 0 && !facingRight) Flip(true);
            else if (aiPath.desiredVelocity.x < 0 && facingRight) Flip(false);
        }

        if ((targetDirectionBased && !velocityBased) || (targetDirectionBased && velocityBased && aiPath.desiredVelocity.x == 0))
        {
            if (playerTracker.position.x > transform.position.x && !facingRight) Flip(true);
            else if (playerTracker.position.x < transform.position.x && facingRight) Flip(false);
        }
    }

    void Flip(bool faceRight)
    {
        Vector2 newScale = transform.localScale;
        if (facingRight != faceRight) newScale.x *= -1;

        facingRight = faceRight;
        transform.localScale = newScale;

        justTurned = true;
    }
}
