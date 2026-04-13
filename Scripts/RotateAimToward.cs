using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAimToward : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 0f;

    [SerializeField] private Transform inheritDirectionFrom;
    [SerializeField] private float angleOffset;

    [SerializeField] private float angleLimitMax;
    [SerializeField] private float angleLimitMin;

    [SerializeField] private bool limited = false;

    void FixedUpdate()
    {
        Vector3 targ = target.position;
        targ.z = 0f;

        Vector3 objectPos = transform.position;
        targ.x -= objectPos.x;
        targ.y -= objectPos.y;

        float targetAngle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;

        float realTargetAngle = targetAngle + angleOffset;

        // if (realTargetAngle > angleLimitMax) realTargetAngle = angleLimitMax;
        // if (realTargetAngle < angleLimitMin) realTargetAngle = angleLimitMin;

        float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, realTargetAngle);
        float clampedDeltaAngle = Mathf.Clamp(deltaAngle, -rotationSpeed, rotationSpeed);

        transform.Rotate(transform.forward, clampedDeltaAngle);
    }
}
