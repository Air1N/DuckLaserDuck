using UnityEngine;
using TMPro;

public class WormTextEnabler : MonoBehaviour
{
    public int tick = 0;
    public int textShowDuration;
    public PlayerBankManager playerBankManager;
    int lastCurrencyTotal = 0;
    public Animator anim;

    void OnEnable()
    {
        tick = 0;
    }

    void FixedUpdate()
    {
        tick++;

        if (playerBankManager.worms + playerBankManager.greenGrubs != lastCurrencyTotal)
        {
            tick = 0;
        }

        if (tick == 0)
        {
            anim.Play("fadeIn");
        }

        if (tick == textShowDuration)
        {
            anim.Play("fadeOut");
        }

        lastCurrencyTotal = playerBankManager.worms + playerBankManager.greenGrubs;
    }

    public void ShowWorms()
    {
        tick = 0;
        anim.Play("fadeIn");
    }

    public void HideWorms()
    {
        tick = textShowDuration;
        anim.Play("fadeOut");
    }

}
