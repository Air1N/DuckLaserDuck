using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootOnInterval : MonoBehaviour
{
    [SerializeField]
    private int cooldown;
    
    [SerializeField]
    private int startOffset;

    private int currentCooldownTick;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string animationValueName;


    void Start() {
        currentCooldownTick = startOffset;
    }
    
    void FixedUpdate()
    {
        currentCooldownTick -= 1;
        if (currentCooldownTick <= 0) {
            animator.SetBool(animationValueName, true);
            currentCooldownTick = cooldown;
        } else {
            animator.SetBool(animationValueName, false);
        }
    }
}
