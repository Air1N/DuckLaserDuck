using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class FadeSpriteTransparencyForSeconds : MonoBehaviour
{
    [SerializeField] private float minDelaySeconds;
    [SerializeField] private float maxDelaySeconds;

    [SerializeField] private float fadeSeconds;

    [SerializeField] private float startOpacity;
    [SerializeField] private float endOpacity;
    [SerializeField] private SpriteRenderer spriteRenderer;


    private int tick = 0;
    private float delaySeconds = 0;

    void Start()
    {
        delaySeconds = Random.Range(minDelaySeconds, maxDelaySeconds);
    }

    void FixedUpdate()
    {
        if (tick >= (delaySeconds + fadeSeconds) * 50) return;
        tick++;

        if (tick < delaySeconds * 50)
            return;

        float currentOpacity = Mathf.Lerp(startOpacity, endOpacity, tick / ((delaySeconds + fadeSeconds) * 50));

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentOpacity);
    }
}
