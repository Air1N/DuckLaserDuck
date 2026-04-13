using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanUpRoom : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private float delaySeconds;

    private void Start()
    {
        StartCoroutine(BeginCleanUp());
    }

    private IEnumerator BeginCleanUp()
    {
        yield return new WaitForSeconds(delaySeconds);

        roomManager.CleanUpRoom();
    }
}
