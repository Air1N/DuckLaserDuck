using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float delaySeconds;
    [SerializeField] private GameObject toDestroyAfterSpawn;

    [SerializeField] private float cooldownSeconds;
    [SerializeField] private float durationSeconds;

    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 maxOffsetRandom;

    [SerializeField] private bool realtime;
    [SerializeField] private bool active;

    [SerializeField] private Transform parentPrefabTo = null;

    public bool stopped = false;

    void OnEnable()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        if (realtime) yield return new WaitForSecondsRealtime(delaySeconds);
        else yield return new WaitForSeconds(delaySeconds);

        int n = 0;
        while (!stopped && cooldownSeconds * n < durationSeconds)
        {
            if (realtime) yield return new WaitForSecondsRealtime(cooldownSeconds);
            else yield return new WaitForSeconds(cooldownSeconds);

            GameObject instancedPrefab = Instantiate(prefab, spawnPoint.position, Quaternion.identity, parentPrefabTo);
            instancedPrefab.SetActive(active);
            n++;
        }

        if (toDestroyAfterSpawn) Destroy(toDestroyAfterSpawn);
    }

    public void StartEmitting()
    {
        stopped = false;
        StartCoroutine(SpawnAfterDelay());
    }

    public void StopEmitting()
    {
        stopped = true;
    }
}
