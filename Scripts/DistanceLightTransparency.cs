using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DistanceLightTransparency : MonoBehaviour
{
    [SerializeField] private Light2D targetLight2D;
    [SerializeField] private float minOpacityDistance;
    [SerializeField] private float maxOpacityDistance;
    [SerializeField] private float minOpacity;
    [SerializeField] private float maxOpacity;
    [SerializeField] private Transform distanceFrom;
    private float currentOpacity = 1f;

    private void Start() {
        if (!distanceFrom) distanceFrom = GameObject.Find("playerFollower").transform;
    }

    private void Update() {
        if ((distanceFrom.position - transform.position).magnitude > maxOpacityDistance) {
            currentOpacity = maxOpacity;
        } else if ((distanceFrom.position - transform.position).magnitude < minOpacityDistance) {
            currentOpacity = minOpacity;
        } else {
            currentOpacity = Mathf.Lerp(minOpacity, maxOpacity, (distanceFrom.position - transform.position).magnitude / maxOpacityDistance);
        }
        
        targetLight2D.color = new Color(targetLight2D.color.r, targetLight2D.color.g, targetLight2D.color.b, currentOpacity);
    }
}
