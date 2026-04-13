using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

public class TrackTransform : MonoBehaviour
{
    public Transform transformToTrack;
    [SerializeField] private Vector2 trackingSpeed;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private Vector2 maxVelocity;
    [SerializeField] private Vector2 drag;
    [SerializeField] private Vector2 timeScaling;

    [SerializeField] private float timeDiff = 0f;
    [SerializeField] private Vector3 offset;

    [SerializeField] private bool drawGizmos = false;

    private float startTime;

    void Awake()
    {
        startTime = Time.time;
    }

    void Start()
    {
        if (!transformToTrack)
        {
            transformToTrack = GameObject.Find("char").transform;
        }
        startTime = Time.time;
    }

    void OnEnable()
    {
        startTime = Time.time;
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = transformToTrack.position + offset;
        if (trackingSpeed == new Vector2(0f, 0f)) transform.position = targetPosition;
        else
        {
            Vector2 jerk = (targetPosition - transform.position).normalized * trackingSpeed;
            if (timeScaling.magnitude > 0f)
            {
                timeDiff = Time.time - startTime;
                jerk *= timeDiff * timeScaling;
            }

            velocity += jerk;
            if (drag.magnitude > 0f) velocity *= new Vector2(1f, 1f) - drag;

            if (velocity.x > maxVelocity.x) velocity.x = maxVelocity.x;
            if (velocity.x < -maxVelocity.x) velocity.x = -maxVelocity.x;

            if (velocity.y > maxVelocity.y) velocity.y = maxVelocity.y;
            if (velocity.y < -maxVelocity.y) velocity.y = -maxVelocity.y;

            transform.position += (Vector3)velocity;
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizmos) Gizmos.DrawWireSphere(transform.position - offset, 0.1f);
    }
}
