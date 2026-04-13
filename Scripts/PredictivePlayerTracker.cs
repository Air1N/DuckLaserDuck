using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictivePlayerTracker : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private float distanceScaleFactor;
    private GameObject player;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        player = GameObject.Find("char");
        roomManager = FindObjectOfType<RoomManager>();
    }

    void FixedUpdate()
    {
        Vector3 moveVector = player.transform.position - lastPlayerPosition;
        Vector3 predictedPosition = player.transform.position + moveVector * distanceScaleFactor;

        predictedPosition = roomManager.ClampWithinRoom(predictedPosition);

        transform.position = predictedPosition;
        lastPlayerPosition = player.transform.position;
    }
}
