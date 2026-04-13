using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentUnparent : MonoBehaviour
{
    [SerializeField] private Transform originalParent;
    [SerializeField] private bool attachToParent;
    [SerializeField] private bool removeFromParent;

    void Start()
    {
        if (attachToParent) transform.parent = originalParent;
        if (removeFromParent) transform.parent = null;
    }

    void OnEnable()
    {
        if (attachToParent) transform.parent = originalParent;
        if (removeFromParent) transform.parent = null;
    }
}
