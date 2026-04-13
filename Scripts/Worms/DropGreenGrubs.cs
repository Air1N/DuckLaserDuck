using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropGreenGrubs : MonoBehaviour
{
    public int minGreenGrubsOnKill;
    public int maxGreenGrubsOnKill;
    [SerializeField] private GameObject greenGrubPrefab;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Vector2 randomOffsetMax;

    public void dropGreenGrubs()
    {
        int totalGreenGrubsAmount = Random.Range(minGreenGrubsOnKill, maxGreenGrubsOnKill + 1);

        for (int i = 0; i < totalGreenGrubsAmount; i++)
        {
            Vector2 trueSpawnPosition = (Vector2)spawnPosition.position + randomOffsetMax * Random.Range(-4f, 4f);
            GameObject greenGrubObject = Instantiate(greenGrubPrefab, trueSpawnPosition, Quaternion.identity, null);
            greenGrubObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f) + 8f), ForceMode2D.Impulse);
        }
    }
}
