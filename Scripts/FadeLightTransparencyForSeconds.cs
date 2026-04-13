using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FadeLightTransparencyForSeconds : MonoBehaviour
{
    [SerializeField] private float delaySeconds;
    [SerializeField] private float fadeSeconds;

    [SerializeField] private float startOpacity;
    [SerializeField] private float endOpacity;
    [SerializeField] private Light2D targetLight2D;

    private int tick = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tick >= (delaySeconds + fadeSeconds) * 50) return;
        tick++;

        if (tick < delaySeconds * 50) {
            return;
        }

        float currentOpacity = Mathf.Lerp(startOpacity, endOpacity, tick / (fadeSeconds * 50));
        
        targetLight2D.color = new Color(targetLight2D.color.r, targetLight2D.color.g, targetLight2D.color.b, currentOpacity);
    }
}
