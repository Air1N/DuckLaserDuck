using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class TrackToCursor : MonoBehaviour
{
    public Camera mainCamera;

    public Transform distanceFromTransform;

    public Vector2 mousePos;
    public float lerpSpeed;
    public Vector3 maxDistance;
    public Vector3 startPos;
    public Vector3 currentPos;
    public Vector3 distPos;

    void Start()
    {
        if (!mainCamera) mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        startPos = transform.position;

    }

    void FixedUpdate()
    {
        currentPos = transform.position;
        distPos = distanceFromTransform.position;

        currentPos.x = Mathf.Clamp(currentPos.x, distPos.x - maxDistance.x, distPos.x + maxDistance.x);
        currentPos.y = Mathf.Clamp(currentPos.y, distPos.y - maxDistance.y, distPos.y + maxDistance.y);
        currentPos.z = Mathf.Clamp(currentPos.z, distPos.z - maxDistance.z, distPos.z + maxDistance.z);

        mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 moveSpeed = ((Vector3)mousePos - transform.position).normalized * lerpSpeed;
        transform.position += Vector3.ClampMagnitude(moveSpeed, ((Vector3)mousePos - transform.position).magnitude);

        if (Vector3.Distance(transform.position, mousePos) <= Vector3.Distance(currentPos, mousePos))
            transform.position = mousePos;
    }
}