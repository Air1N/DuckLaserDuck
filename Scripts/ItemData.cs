using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public int rarity;
    public string title;
    public int maxLevel = 1;
    public bool isUpgrade;
    public UpgradeData upgradeData;

    private void Start()
    {
        if (isUpgrade)
        {
            rarity = upgradeData.rarity;
            title = upgradeData.title;
            maxLevel = upgradeData.maxLevel;
        }
    }
}
