using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Animations;

public class AnimateWhenWalking : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private AIPath aiPath;
    public bool stopped = false;

    void FixedUpdate()
    {
        if (stopped) anim.SetFloat("velocity", 0f);
        else anim.SetFloat("velocity", Mathf.Abs(aiPath.desiredVelocity.x) + Mathf.Abs(aiPath.desiredVelocity.y));
    }
}
