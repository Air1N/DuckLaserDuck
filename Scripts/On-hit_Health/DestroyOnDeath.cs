using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDeath : MonoBehaviour
{
    [SerializeField] private HitManager hitManager;
    [SerializeField] private float delay;

    void OnEnable()
    {
        StartCoroutine(DestroyUponDeath());
    }

    private IEnumerator DestroyUponDeath()
    {
        yield return new WaitUntil(() => hitManager.dead);

        yield return new WaitForSecondsRealtime(delay);

        Destroy(gameObject);
        StartCoroutine(DestroyUponDeath());
    }
}
