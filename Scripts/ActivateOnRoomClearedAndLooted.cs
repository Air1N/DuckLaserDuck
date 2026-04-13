using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnRoomClearedAndLooted : MonoBehaviour
{
    [SerializeField] private GameObject toActivate;
    private RoomManager roomManager;
    private bool rewarded = false;

    [SerializeField] private int afterClears = 1;
    private int clearNum = 0;
    [SerializeField] private float delay = 0f;

    void Start()
    {
        roomManager = FindFirstObjectByType<RoomManager>();
    }

    void FixedUpdate()
    {
        if (!rewarded && roomManager.roomCleared && roomManager.fullyLooted)
        {
            clearNum++;

            if (clearNum >= afterClears)
            {
                if (delay > 0) StartCoroutine(ActivateAfterDelay());
                else toActivate.SetActive(true);

                rewarded = true;
            }
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delay);

        if (roomManager.roomCleared && roomManager.fullyLooted)
            toActivate.SetActive(true);
    }
}
