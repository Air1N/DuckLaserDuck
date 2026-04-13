using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private int swapDuration;
    [SerializeField] private List<CupController> cups;
    public bool started = false;
    private int tick = 0;

    private bool swapAccounted = false;

    void FixedUpdate()
    {
        if (!started)
        {
            cups[0].goodCup = false;
            cups[1].goodCup = true;
            cups[2].goodCup = false;
        }

        started = true; // Create a trigger event somehow
        if (!started) return;

        AnimatorStateInfo animatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo animatorClipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];
        string currentClip = animatorClipInfo.clip.name;

        Debug.Log("animatorStateInfo.normalizedTime: " + animatorStateInfo.normalizedTime.ToString());

        if (animatorStateInfo.normalizedTime >= 0.9f)
        {
            if (swapAccounted) return;

            switch (currentClip)
            {
                case "spider_swap2_01":
                    SwapCups(0, 1);
                    break;
                case "spider_swap2_02":
                    SwapCups(0, 2);
                    break;
                case "spider_swap2_12":
                    SwapCups(1, 2);
                    break;
                default:
                    break;
            }

            swapAccounted = true;
        }
        else swapAccounted = false;

        tick++;
        if (tick < swapDuration)
            anim.SetBool("swapping", true);
        else anim.SetBool("swapping", false);
    }

    private void SwapCups(int a, int b)
    {
        bool tempGoodCupValueA = cups[a].goodCup;
        bool tempGoodCupValueB = cups[b].goodCup;

        cups[a].goodCup = tempGoodCupValueB;
        cups[b].goodCup = tempGoodCupValueA;

        // FOR DEBUGGING
        // foreach (CupController cup in cups)
        // {
        //     if (cup.goodCup) cup.redArrow.SetActive(true);
        //     else cup.redArrow.SetActive(false);
        // }
    }
}
