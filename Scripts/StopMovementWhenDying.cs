using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class StopMovementWhenDying : MonoBehaviour
{
    [SerializeField] private HitManager hitManager;
    [SerializeField] private AIPath aIPath;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hitManager.health <= 0) {
            aIPath.maxSpeed = 0f;
        }
    }
}
