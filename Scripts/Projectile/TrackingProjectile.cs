using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackingProjectile : MonoBehaviour
{
    public float RotationSpeed;
    public float TrackingDistance = 6f;

    [SerializeField] private List<string> trackTags = new() { "EnemyBody" };
    private Rigidbody2D rb;
    private GameObject closest;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (string trackTag in trackTags)
        {
            GameObject close = FindClosest(trackTag);

            if (close)
            {
                RotateToward(close.transform, transform);
            }
        }
    }

    public GameObject FindClosest(string findTag)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(findTag);
        closest = null;
        float distance = Mathf.Infinity;
        foreach (GameObject go in gos)
        {
            float curDistance = Vector3.Distance(go.transform.position, transform.position);
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return distance < TrackingDistance ? closest : null;
    }

    void RotateToward(Transform target, Transform ye)
    {
        Vector3 dir = target.position - ye.position;

        float targetAngle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 360) % 360;
        float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, targetAngle);
        float newAngle = Mathf.Min(Mathf.Abs(deltaAngle), RotationSpeed) * Mathf.Sign(deltaAngle);

        ye.Rotate(transform.forward, newAngle);
    }

}
