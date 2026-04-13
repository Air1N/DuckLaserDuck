using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupController : MonoBehaviour
{
    public bool goodCup = false;

    public GameObject redArrow;

    [SerializeField] private Worms worms;
    [SerializeField] private DropGreenGrubs dropGreenGrubs;

    private int minWormsOnKill = 0;
    private int maxWormsOnKill = 0;

    private int minGreenGrubsOnKill = 0;
    private int maxGreenGrubsOnKill = 0;

    private void Start()
    {
        minWormsOnKill = worms.minWormsOnKill;
        maxWormsOnKill = worms.maxWormsOnKill;

        minGreenGrubsOnKill = dropGreenGrubs.minGreenGrubsOnKill;
        maxGreenGrubsOnKill = dropGreenGrubs.maxGreenGrubsOnKill;
    }

    private void FixedUpdate()
    {
        if (goodCup)
        {
            worms.minWormsOnKill = minWormsOnKill;
            worms.maxWormsOnKill = maxWormsOnKill;

            dropGreenGrubs.minGreenGrubsOnKill = minGreenGrubsOnKill;
            dropGreenGrubs.maxGreenGrubsOnKill = maxGreenGrubsOnKill;
        }
        else
        {
            worms.minWormsOnKill = 0;
            worms.maxWormsOnKill = 0;

            dropGreenGrubs.minGreenGrubsOnKill = 0;
            dropGreenGrubs.maxGreenGrubsOnKill = 0;
        }
    }

    private void OnDestroy()
    {
        CupController[] cupControllers = FindObjectsOfType<CupController>();

        foreach (CupController cupC in cupControllers)
        {
            if (cupC.goodCup)
            {
                cupC.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.5f, 0.8f);
                cupC.goodCup = false;
            }

            Destroy(cupC.gameObject, 2f);
        }
    }
}
