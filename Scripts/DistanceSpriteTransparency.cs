using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSpriteTransparency : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
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
        
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentOpacity);
    }
}
