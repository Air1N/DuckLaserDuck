using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyOnRoomLoad : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    // Start is called before the first frame update
    void Start()
    {
        roomManager = FindObjectOfType<RoomManager>();
        if (roomManager) roomManager.trashedObjectsAtRoomEnd.Add(gameObject);
    }
}
