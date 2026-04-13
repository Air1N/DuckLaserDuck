using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesetController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private List<MovesetItem> moveset;

    [SerializeField] private int currentCooldown = 0;

    void FixedUpdate()
    {
        currentCooldown -= 1;
        if (currentCooldown <= 0)
        {
            float rand = Random.Range(0f, 100f);
            MovesetItem selectedMove = SelectFromMoveset(rand, moveset);
            animator.SetInteger("move_id", selectedMove.moveId);
            currentCooldown = selectedMove.cooldown;
        }
        else animator.SetInteger("move_id", -1);
    }

    private MovesetItem SelectFromMoveset(float rand, List<MovesetItem> movesList)
    {
        float totalChance = 0;
        foreach (MovesetItem movesetItem in movesList)
        {
            totalChance += movesetItem.chance;
        }

        float divisor = totalChance / 100f;

        float cumulativeChance = 0;
        foreach (MovesetItem movesetItem in movesList)
        {
            cumulativeChance += movesetItem.chance;
            if (cumulativeChance / divisor >= rand)
            {
                return movesetItem;
            }
        }

        Debug.LogError("cumulativeChance = " + cumulativeChance + ", rand = " + rand + " but nothing was selected??");
        return null;
    }
}

[System.Serializable]
public class MovesetItem
{
    [Tooltip("Title of the move, just for developer viewing, doesn't actually do anything")]
    public string moveTitle;
    public int moveId;
    public float chance;
    public int cooldown;
}
