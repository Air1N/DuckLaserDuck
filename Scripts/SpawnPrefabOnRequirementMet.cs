using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabOnRequirementMet : MonoBehaviour
{
    [SerializeField] private PlayerUpgradesManager upgradesManager;
    [SerializeField] private string[] requirements;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int amountToSpawn = 1;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform parent = null;

    [SerializeField] private float delaySeconds = 0.5f;

    private int spawnCount = 0;

    void Start()
    {
        upgradesManager = FindFirstObjectByType<PlayerUpgradesManager>();
    }

    void FixedUpdate()
    {
        if (spawnCount >= amountToSpawn) return;

        if (upgradesManager.CheckRequirements(requirements))
        {
            spawnCount++;
            StartCoroutine(SpawnPrefab());
        }
    }

    private IEnumerator SpawnPrefab()
    {
        yield return new WaitForSeconds(delaySeconds);
        Instantiate(prefab, spawnPoint.position, Quaternion.identity, parent);
    }
}
