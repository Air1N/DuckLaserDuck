using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class TrackingRange : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private HitManager hitManager;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private float trackingRange;
    [SerializeField] private float trackingRangeIncreaseWhenHurt = 8f;
    [SerializeField] private Transform playerLocator;
    [SerializeField] private Vector3 baseLocatorOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 maxSingleRandomOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 maxPerFrameRandomOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 maxPerCooldownRandomOffset = new Vector3(0f, 0f, 0f);

    [SerializeField] private Vector2 cooldown;
    private int currentCooldownTick;


    private bool trackingBoosted = false;

    private float storedMaxSpeed = 0f;
    private Vector3 randomOffset;
    private Vector3 perCDRandomOffset;

    private void Start()
    {
        randomOffset = new Vector3(Random.Range(-maxSingleRandomOffset.x, maxSingleRandomOffset.x), Random.Range(-maxSingleRandomOffset.y, maxSingleRandomOffset.y), Random.Range(-maxSingleRandomOffset.z, maxSingleRandomOffset.z));
        player = GameObject.Find("playerFollower").transform;
    }

    private void FixedUpdate()
    {
        currentCooldownTick--;
        if (currentCooldownTick <= 0)
        {
            perCDRandomOffset = new Vector3(Random.Range(-maxPerCooldownRandomOffset.x, maxPerCooldownRandomOffset.x), Random.Range(-maxPerCooldownRandomOffset.y, maxPerCooldownRandomOffset.y), Random.Range(-maxPerCooldownRandomOffset.z, maxPerCooldownRandomOffset.z));
            currentCooldownTick = (int)Random.Range(cooldown.x, cooldown.y);
        }

        if (hitManager != null && hitManager.health < hitManager.maxHealth && !trackingBoosted)
        {
            trackingBoosted = true;
            trackingRange += trackingRangeIncreaseWhenHurt;
        }

        if (Vector3.Distance(player.position, transform.position) < trackingRange)
        {
            Vector3 perFrameRandomOffset = new Vector3(Random.Range(-maxPerFrameRandomOffset.x, maxPerFrameRandomOffset.x), Random.Range(-maxPerFrameRandomOffset.y, maxPerFrameRandomOffset.y), Random.Range(-maxPerFrameRandomOffset.z, maxPerFrameRandomOffset.z));
            playerLocator.position = player.position + baseLocatorOffset + randomOffset + perFrameRandomOffset + perCDRandomOffset;
            if (storedMaxSpeed > 0f && aiPath.maxSpeed == 0f) aiPath.maxSpeed = storedMaxSpeed;
        }
        else if (aiPath.maxSpeed > 0)
        {
            storedMaxSpeed = aiPath.maxSpeed;
            aiPath.maxSpeed = 0f;
        }
    }
}
