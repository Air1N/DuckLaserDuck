using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ActiveSortingScript : MonoBehaviour
{
    [Header("Only pick one of the following:")]
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float heightOffset;
    [SerializeField] private bool drawGizmos = false;

    void LateUpdate()
    {
        if (sortingGroup)
        {
            sortingGroup.sortingOrder = -(int)((transform.position.y + heightOffset) * 100f) + 8;
        }

        if (spriteRenderer)
        {
            spriteRenderer.sortingOrder = -(int)((transform.position.y + heightOffset) * 100f) + 8;
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizmos) Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + heightOffset, transform.position.z), 0.1f);
    }
}
