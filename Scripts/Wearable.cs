using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wearable : MonoBehaviour
{
    void Start()
    {
        Transform hatParent = GameObject.Find("HatPosition").transform;
        transform.SetParent(hatParent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }
}
