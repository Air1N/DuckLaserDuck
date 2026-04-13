using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AllyPortalController : MonoBehaviour
{
    private RoomManager roomManager;
    [SerializeField] private bool nextLevel;

    private void Start()
    {
        roomManager = FindFirstObjectByType<RoomManager>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (nextLevel) roomManager.NextMap(); // pass through for mapManager.regenerate because MapManager is disabled and so can't be found
            StartCoroutine(roomManager.OpenMap(true));
        }
    }
}
