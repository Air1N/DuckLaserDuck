using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeWithShots : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float delaySeconds;
    [SerializeField] public int amountToSpawn = 12;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float randomSpreadRange = 10f;
    void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    // Update is called once per frame
    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        float rotation = 0f;
        for (int i = 0; i < amountToSpawn; i++)
        {
            rotation += 360f / amountToSpawn;
            rotation += Random.Range(-randomSpreadRange, randomSpreadRange);

            Instantiate(projectilePrefab, spawnPoint.position, Quaternion.Euler(0f, 0f, rotation));
        }
    }
}
