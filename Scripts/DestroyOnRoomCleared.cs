using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnRoomCleared : MonoBehaviour
{
    [SerializeField] private float delay;
    private RoomManager roomManager;

    void Start()
    {
        roomManager = GameObject.Find("roomFloorType1").GetComponent<RoomManager>();
    }

    void FixedUpdate()
    {
        if (roomManager.roomCleared) StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        if (!roomManager.roomCleared) yield return null;

        yield return new WaitForSecondsRealtime(delay);
        Destroy(gameObject);
    }
}
