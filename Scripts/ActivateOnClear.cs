using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnClear : MonoBehaviour
{
    [SerializeField] private GameObject toActivate;
    private RoomManager roomManager;
    private bool rewarded = false;

    [SerializeField] private int afterClears = 1;
    private int clearNum = 0;

    void Start()
    {
        roomManager = FindFirstObjectByType<RoomManager>();
    }

    void FixedUpdate()
    {
        if (!rewarded && roomManager.roomCleared)
        {
            clearNum++;

            if (clearNum >= afterClears)
            {
                toActivate.SetActive(true);
                rewarded = true;
            }
        }
    }
}
