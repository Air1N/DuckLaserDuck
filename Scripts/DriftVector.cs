using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class DriftVector : MonoBehaviour
{
    private int ticksTilNextChange = 0;

    [SerializeField]
    private int minBetweenChanges = 50;

    [SerializeField]
    private int maxBetweenChanges = 300;

    [SerializeField]
    private Vector2 minSpeedLR_FB = new Vector2(-1f, -1f);

    [SerializeField]
    private Vector2 maxSpeedLR_FB = new Vector2(1f, 1f);

    [SerializeField]
    private AIPath aiPath;

    private float dirLR = 0f;
    private float dirFB = 0f;


    // Update is called once per frame
    void FixedUpdate()
    {
        ticksTilNextChange--;

        if (ticksTilNextChange <= 0) {
            dirLR = Random.Range(minSpeedLR_FB.x, minSpeedLR_FB.x);
            dirFB = Random.Range(maxSpeedLR_FB.y, maxSpeedLR_FB.y);

            ticksTilNextChange = Random.Range(minBetweenChanges, maxBetweenChanges);
        }
        
        if (aiPath != null) aiPath.Move(dirLR * transform.right + dirFB * transform.up);
        else transform.position += dirLR * transform.right + dirFB * transform.up;
    }
}
