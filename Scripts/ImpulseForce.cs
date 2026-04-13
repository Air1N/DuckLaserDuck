using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ImpulseForce : MonoBehaviour
{
    [SerializeField] private Vector2 maxLeftUpForce;
    [SerializeField] private Vector2 maxRightDownForce;

    [SerializeField] private float baseUpDownForce;
    [SerializeField] private float baseLeftRightForce;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-maxLeftUpForce.x, maxRightDownForce.x) + baseLeftRightForce, Random.Range(-maxLeftUpForce.y, maxRightDownForce.y) + baseUpDownForce), ForceMode2D.Impulse);
    }

    void OnEnable()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-maxLeftUpForce.x, maxRightDownForce.x) + baseLeftRightForce, Random.Range(-maxLeftUpForce.y, maxRightDownForce.y) + baseUpDownForce), ForceMode2D.Impulse);
    }
}
