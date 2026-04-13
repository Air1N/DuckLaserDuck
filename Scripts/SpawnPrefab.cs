using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float delaySeconds;
    [SerializeField] private GameObject toDestroyAfterSpawn;

    void OnEnable()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        Instantiate(prefab, spawnPoint.position, Quaternion.identity, null);
        if (toDestroyAfterSpawn) Destroy(toDestroyAfterSpawn);
    }
}
