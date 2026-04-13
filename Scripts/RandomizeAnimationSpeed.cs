using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAnimationSpeed : MonoBehaviour
{
    [SerializeField] private float min;
    [SerializeField] private float max;
    [SerializeField] private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator.speed = Random.Range(min, max);
    }
}
