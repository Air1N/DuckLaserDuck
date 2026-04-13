using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SpawnShopItem : MonoBehaviour
{
    private List<GameObject> shopItems;
    private PlayerUpgradesManager upgradeManager;
    public List<string> shownItems = new();

    // Start is called before the first frame update
    private void Start()
    {
        upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
        shopItems = Resources.LoadAll<GameObject>("ShopItems").ToList();
        SpawnRandomShopItem();
    }

    private void SpawnRandomShopItem()
    {
        int rand = Random.Range(0, shopItems.Count);
        GameObject selectedItem = shopItems[rand];
        ItemData itemData = selectedItem.GetComponent<ItemData>();

        int tries = 0;
        bool retry = true;
        while (tries < 5000 && retry)
        {
            tries++;
            rand = Random.Range(0, shopItems.Count);
            selectedItem = shopItems[rand];
            itemData = selectedItem.GetComponent<ItemData>();

            if (itemData.isUpgrade)
            {
                if (!upgradeManager.CheckRequirements(itemData.upgradeData.requirements) || !RollRarity(itemData.upgradeData.rarity) || CheckOverLevel(itemData.upgradeData.title, itemData.upgradeData.maxLevel, isUpgrade: true) || shownItems.Contains(itemData.upgradeData.title))
                {
                    retry = true;
                }
                else retry = false;
            }
            else
            {
                if (!RollRarity(itemData.rarity) || CheckOverLevel(itemData.title, itemData.maxLevel) || shownItems.Contains(itemData.title))
                {
                    retry = true;
                }
                else retry = false;
            }
        }

        if (tries >= 5000)
            Debug.LogError("over 5000 tries to find shop item. probably will bug.");

        SpawnShopItem[] shopItemSpawners = FindObjectsByType<SpawnShopItem>(sortMode: FindObjectsSortMode.None);
        foreach (SpawnShopItem itemSpawner in shopItemSpawners)
        {
            if (itemData.isUpgrade) itemSpawner.shownItems.Add(itemData.upgradeData.title);
            else itemSpawner.shownItems.Add(itemData.title);
        }

        GameObject spawnedItem = Instantiate(selectedItem, transform.position, Quaternion.identity);
        BuyableScript itemBuyableScript = spawnedItem.GetComponent<BuyableScript>();
        itemBuyableScript.cost = (int)(itemBuyableScript.cost * Random.Range(0.8f, 1.2f) * (1 - upgradeManager.rewardsCardDiscount));

        Color rarityTextColor = Color.HSVToRGB(
            (0.9f + itemData.rarity * 0.065f) % 1f,
            1f,
            0.92f
        );
        itemBuyableScript.labelText.color = rarityTextColor;
    }

    private bool RollRarity(int itemRarity)
    {
        return Random.Range(0f, 100f) > itemRarity * 19;
    }

    private bool CheckOverLevel(string title, int maxLevel, bool isUpgrade = false)
    {
        if (isUpgrade)
        {
            List<string> selectedUpgradeOccurences = upgradeManager.upgrades.FindAll(
                x => x.Equals(title)
            );

            List<string> selectedItemOccurences = upgradeManager.items.FindAll(
                x => x.Equals(title)
            );

            int upgradeLevel = selectedUpgradeOccurences.Count + selectedItemOccurences.Count;
            if (upgradeLevel >= maxLevel) return true;
        }
        else
        {
            List<string> selectedItemOccurences = upgradeManager.items.FindAll(
                x => x.Equals(title)
            );

            int upgradeLevel = selectedItemOccurences.Count;
            if (upgradeLevel >= maxLevel) return true;
        }

        return false;
    }
}
