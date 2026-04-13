using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionRotation : MonoBehaviour
{
    [SerializeField] private Vector3 setPositionTo;
    [SerializeField] private Quaternion setRotationTo;

    [SerializeField] private bool worldSpace = false;

    void Start()
    {
        if (worldSpace)
        {
            transform.position = setPositionTo;
            transform.rotation = setRotationTo;
        }
        else
        {
            transform.localPosition = setPositionTo;
            transform.localRotation = setRotationTo;
        }
    }

    void OnEnable()
    {
        if (worldSpace)
        {
            transform.position = setPositionTo;
            transform.rotation = setRotationTo;
        }
        else
        {
            transform.localPosition = setPositionTo;
            transform.localRotation = setRotationTo;
        }
    }

    void FixedUpdate()
    {
        if (worldSpace)
        {
            transform.position = setPositionTo;
            transform.rotation = setRotationTo;
        }
        else
        {
            transform.localPosition = setPositionTo;
            transform.localRotation = setRotationTo;
        }
    }
}
