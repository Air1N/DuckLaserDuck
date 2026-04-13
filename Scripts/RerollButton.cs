using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RerollButton : MonoBehaviour
{
    [SerializeField] private PlayerBankManager playerBankManager;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private UpgradeMenuController upgradeMenuController;
    private PlayerUpgradesManager upgradeManager;

    public int cost = 0;
    public int pullCount = 0;

    private void Start()
    {
        upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
    }

    public void StartReroll()
    {
        if (pullCount == 0)
        {
            pullCount++;

            upgradeMenuController.animationStarted = true;
        }
        else if (playerBankManager.worms + upgradeManager.creditCardDebtLimit >= cost)
        {
            pullCount++;

            playerBankManager.worms -= cost;

            upgradeMenuController.Reset();
            upgradeMenuController.animationStarted = true;
        }
        else
        {
            audioSource.Play();
        }
    }
}
