using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabFromList : MonoBehaviour
{
    [SerializeField] private List<PrefabChancePair> prefabList;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float delaySeconds;

    void OnEnable()
    {
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        GameObject selectedPrefab;

        float rand = Random.Range(0f, 100f);
        selectedPrefab = SelectGameObjectFromRandom(rand, prefabList);
        Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity, null);
    }

    private GameObject SelectGameObjectFromRandom(float rand, List<PrefabChancePair> prefabList)
    {
        float totalChance = 0;
        foreach (PrefabChancePair prefabChancePair in prefabList)
        {
            totalChance += prefabChancePair.chance;
        }

        float divisor = totalChance / 100f;

        float cumulativeChance = 0;
        foreach (PrefabChancePair prefabChancePair in prefabList)
        {
            cumulativeChance += prefabChancePair.chance;
            if (cumulativeChance / divisor >= rand)
            {
                Debug.Log("cumulativeChance = " + cumulativeChance / divisor + " w/ " + divisor + ", rand = " + rand + " but " + prefabChancePair.prefab.name + " was selected??");
                return prefabChancePair.prefab; // TODO roomDivisor being a global in RoomManager is kind of clunky
            }
        }

        Debug.LogError("cumulativeChance = " + cumulativeChance + ", rand = " + rand + " but nothing was selected??");
        return null;
    }
}