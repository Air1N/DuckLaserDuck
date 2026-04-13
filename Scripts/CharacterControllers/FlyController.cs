using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class FlyScript : MonoBehaviour
{
    public float moveSpeed;
    public AIPath aiPath;

    void Start()
    {
        aiPath.maxSpeed = moveSpeed;
    }
}
