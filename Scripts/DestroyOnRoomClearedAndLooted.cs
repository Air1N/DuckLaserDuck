using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnRoomClearedAndLooted : MonoBehaviour
{
    [SerializeField] private float delay;
    private RoomManager roomManager;

    void Start()
    {
        roomManager = GameObject.Find("roomFloorType1").GetComponent<RoomManager>();
    }

    void FixedUpdate()
    {
        if (roomManager.roomCleared && roomManager.fullyLooted) StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delay);

        if (roomManager.roomCleared && roomManager.fullyLooted)
            Destroy(gameObject);
    }
}
