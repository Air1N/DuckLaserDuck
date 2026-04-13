using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSpriteScaling : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float minScaleDistance = 0.01f;
    [SerializeField] private float maxScaleDistance = 0.1f;
    [SerializeField] private float minScale = 0f;
    [SerializeField] private float maxScale = 1f;
    [SerializeField] private Transform distanceFrom;
    private float currentScale = 1f;

    private void Start() {
        if (!distanceFrom) distanceFrom = GameObject.Find("playerFollower").transform;
    }

    private void Update() {
        if ((distanceFrom.position - transform.position).magnitude > maxScaleDistance) {
            currentScale = maxScale;
        } else if ((distanceFrom.position - transform.position).magnitude < minScaleDistance) {
            currentScale = minScale;
        } else {
            currentScale = Mathf.Lerp(minScale, maxScale, (distanceFrom.position - transform.position).magnitude / maxScaleDistance);
        }
        
        spriteRenderer.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }
}
