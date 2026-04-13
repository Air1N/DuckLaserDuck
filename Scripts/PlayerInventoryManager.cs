using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour
{
    [SerializeField] private PlayerUpgradesManager upgradeManager;
    [SerializeField] private GameObject defaultItemPrefab;
    [SerializeField] private GameObject itemHolder;

    private List<(GameObject, UpgradeData, UpgradeToPlayer)> inventoryItems = new();

    public void AddItem(GameObject upgradeObject)
    {
        UpgradeData upgradeData = upgradeObject.GetComponent<UpgradeData>();
        UpgradeToPlayer upgradeToPlayer = upgradeObject.GetComponent<UpgradeToPlayer>();

        bool containsThis = inventoryItems.Any(item => item.Item2.title == upgradeData.title);
        if (containsThis) return;

        GameObject newItem = Instantiate(defaultItemPrefab, Vector3.zero, Quaternion.identity, itemHolder.transform);
        SlotMachineUpgradeIconData upgradeIconData = newItem.GetComponent<SlotMachineUpgradeIconData>();

        upgradeIconData.imageRenderer.sprite = upgradeData.sprite;

        newItem.GetComponent<RectTransform>().anchoredPosition = new Vector3(375 + 233 * (inventoryItems.Count % 12f), -300 - 233 * Mathf.Floor(inventoryItems.Count / 12f), 0f);
        inventoryItems.Add((newItem, upgradeData, upgradeToPlayer));
    }

    public void UpdateTitleInformation()
    {
        foreach ((GameObject item, UpgradeData upgradeData, UpgradeToPlayer upgradeToPlayer) in inventoryItems)
        {
            SlotMachineUpgradeIconData upgradeIconData = item.GetComponent<SlotMachineUpgradeIconData>();

            Debug.Log("Enabling panel");
            upgradeIconData.panel.SetActive(true);

            List<string> selectedUpgradeOccurences = upgradeManager.upgrades.FindAll(
                x => x.Equals(upgradeData.title)
            );

            List<string> selectedItemOccurences = upgradeManager.items.FindAll(
                x => x.Equals(upgradeData.title)
            );

            Color titleTextColor = Color.HSVToRGB(
                (0.92f + upgradeData.rarity * 0.065f) % 1f,
                1f,
                0.94f
            );

            Color descTextColor = Color.HSVToRGB(
                (0.9f + upgradeData.rarity * 0.065f) % 1f,
                1f,
                0.92f
            );

            LocalizedString dynamicString = new LocalizedString();
            dynamicString.TableReference = "StringLocalizationTable";
            dynamicString.TableEntryReference = upgradeData.title + "Desc";

            Debug.Log("Check for upgrade to player");
            if (upgradeToPlayer)
            {
                Debug.Log("Has upgrade to player");
                string localizedString = UpgradeDescriptionSetter.GetLocalizedDescription(dynamicString, upgradeManager, upgradeToPlayer, upgradeData.title + "Desc", true);
                Debug.Log("Localized String: `" + localizedString + "`");
                upgradeIconData.descTextObj.text = localizedString;
                upgradeIconData.descTextObj.color = descTextColor;
            }
            else
            {
                Debug.Log("Has NO upgrade to player!");
                upgradeIconData.descLocalizeStringEvent.SetEntry(upgradeData.title + "Desc");
            }

            Debug.Log("Refresh String");
            dynamicString.RefreshString();
            upgradeIconData.descTextObj.gameObject.GetComponent<FixArabicText>().FixText(null);

            Debug.Log("Title Stuff");
            upgradeIconData.titleTextObj.color = titleTextColor;
            upgradeIconData.titleLocalizeStringEvent.enabled = true;
            upgradeIconData.titleLocalizeStringEvent.SetEntry(upgradeData.title + "Title");
            upgradeIconData.titleLocalizeStringEvent.RefreshString();
            upgradeIconData.titleLocalizeStringEvent.enabled = false;
            upgradeIconData.titleTextObj.gameObject.GetComponent<FixArabicText>().FixText(null);

            if (upgradeData.maxLevel > 1)
            {
                int currentUpgradeLevel = selectedUpgradeOccurences.Count + selectedItemOccurences.Count;
                if (currentUpgradeLevel == upgradeData.maxLevel)
                    upgradeIconData.titleTextObj.text += " [MAX]";
                else
                    upgradeIconData.titleTextObj.text += " [lv " + currentUpgradeLevel + "]";
            }

            upgradeIconData.panel.SetActive(false);
        }
    }
}
