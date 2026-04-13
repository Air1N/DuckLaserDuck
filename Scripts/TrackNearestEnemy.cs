using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackNearestEnemy : MonoBehaviour
{
    [SerializeField] Transform distanceFrom;
    void FixedUpdate()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyBody");

        float minDist = 99999f;
        foreach (GameObject enemy in enemies) {
            float dist = Vector3.Distance(enemy.transform.position, distanceFrom.position);
            if (dist <= minDist) {
                minDist = dist;
                transform.position = enemy.transform.position;
            }
        }
    }
}
