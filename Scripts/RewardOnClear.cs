using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RewardOnClear : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;
    private RoomManager roomManager;
    private bool rewarded = false;

    void Start()
    {
        roomManager = FindFirstObjectByType<RoomManager>();
    }

    void FixedUpdate()
    {
        if (!rewarded && roomManager.roomCleared)
        {
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            rewarded = true;
        }
    }
}
