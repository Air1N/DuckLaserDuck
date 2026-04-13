using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoScaleSprite : MonoBehaviour
{
    private Vector2 spriteCenter;
    private Vector2 spriteSize;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float scale;

    // Start is called before the first frame update
    void Start()
    {
        if (spriteRenderer.sprite)
        {
            spriteCenter = spriteRenderer.sprite.bounds.center;
            spriteSize = spriteRenderer.sprite.bounds.extents;
            spriteRenderer.transform.localScale = Mathf.Min(1f / spriteSize.x, 1f / spriteSize.y) * scale * Vector2.one;
        }
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.transform.localScale = Mathf.Min(1f / spriteSize.x, 1f / spriteSize.y) * scale * Vector2.one;
    }
}
