using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class BuyableScript : MonoBehaviour
{
    [SerializeField] private int itemID;
    public int cost = 123;

    [SerializeField] public TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Light2D light2D;
    [SerializeField] private PlayerController playerController;
    public bool buyMe = false;

    [SerializeField] public GameObject prefabToSpawnOnBuy;
    [SerializeField] private PlayerBankManager playerBankManager;
    [SerializeField] private AudioSource audioSource;
    private int soundCooldown = 0;

    private PlayerUpgradesManager upgradeManager;

    [SerializeField] private ItemData itemData;

    [SerializeField] private bool useWorms = true;
    [SerializeField] private bool useGrubs;

    [SerializeField] private bool hasPopupDescription = false;
    [SerializeField] private GameObject popupObject;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private LocalizeStringEvent titleLocalize;

    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private LocalizeStringEvent descLocalize;

    [SerializeField] public bool isHat;
    [SerializeField] public bool wearingThis = false;

    [SerializeField] private bool unlockable = false;
    public bool unlocked = false;
    [SerializeField] private int unlockedCost = 0;

    [SerializeField] private GameObject currencyIcon;

    private SaveGameManager saveGameManager;

    private Transform hatContainer;

    void Start()
    {
        GameObject player = GameObject.Find("char");
        playerController = player.GetComponent<PlayerController>();
        upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
        playerBankManager = player.GetComponent<PlayerBankManager>();
        saveGameManager = FindFirstObjectByType<SaveGameManager>();

        if (saveGameManager.saveData.m_UnlockedIds.Contains(itemID)) unlocked = true;

        if (unlockable && unlocked) cost = unlockedCost;
        if (costText) costText.text = cost.ToString();

        hatContainer = GameObject.Find("HatPosition").transform;

        if (itemData && itemData.upgradeData && itemData.isUpgrade && spriteRenderer)
        {
            spriteRenderer.sprite = itemData.upgradeData.sprite;
        }
    }

    void FixedUpdate()
    {
        if (unlockable && unlocked)
        {
            cost = unlockedCost;
            if (costText) costText.text = cost.ToString();
        }
        if (cost == 0) HideCostText();

        if (isHat && wearingThis) return;
        if (soundCooldown > 0) soundCooldown--;

        if (buyMe && hasPopupDescription) ShowDescription();
        else HideDescription();

        if (buyMe && playerController.shooting)
        {
            if (useWorms && playerBankManager.worms + upgradeManager.creditCardDebtLimit >= cost)
            {
                playerBankManager.worms -= cost;
                DoBuy();
            }
            else if (useGrubs && playerBankManager.greenGrubs >= cost)
            {
                playerBankManager.greenGrubs -= cost;
                DoBuy();
            }
            else if (soundCooldown <= 0)
            {
                soundCooldown = 20;
                audioSource.Play();
            }
        }
    }

    private void DoBuy()
    {
        if (isHat)
        {
            if (hatContainer.childCount > 0)
            {
                Destroy(hatContainer.GetChild(0).gameObject);
            }
            BuyableScript[] otherBuyables = FindObjectsOfType<BuyableScript>();

            foreach (BuyableScript buyable in otherBuyables)
            {
                if (buyable.isHat) buyable.wearingThis = false;
            }

            wearingThis = true;
        }

        if (!unlocked)
        {
            unlocked = true;
            saveGameManager.saveData.m_UnlockedIds.Add(itemID);
            saveGameManager.SaveJsonData();
        }

        Instantiate(prefabToSpawnOnBuy, transform.position, Quaternion.identity);
        if (itemData) upgradeManager.items.Add(itemData.title);
        if (!isHat) Destroy(gameObject);
    }

    private void ShowDescription()
    {
        popupObject.SetActive(true);
        UpgradeData upgradeData = prefabToSpawnOnBuy.GetComponent<UpgradeData>();
        prefabToSpawnOnBuy.TryGetComponent(out UpgradeToPlayer upgradeToPlayer);

        List<string> selectedUpgradeOccurences = upgradeManager.upgrades.FindAll(
            x => x.Equals(upgradeData.title)
        );

        List<string> selectedItemOccurences = upgradeManager.items.FindAll(
            x => x.Equals(upgradeData.title)
        );

        Color upgradeTextColor = Color.HSVToRGB(
            (0.9f + upgradeData.rarity * 0.065f) % 1f,
            1f,
            0.92f
        );

        LocalizedString dynamicString = new LocalizedString();
        dynamicString.TableReference = "StringLocalizationTable";
        dynamicString.TableEntryReference = upgradeData.title + "Desc";

        if (upgradeToPlayer)
        {
            descText.text = UpgradeDescriptionSetter.GetLocalizedDescription(dynamicString, upgradeManager, upgradeToPlayer, upgradeData.title + "Desc", false);
            descText.color = upgradeTextColor;
        }
        else
        {
            Debug.Log("Has NO upgrade to player!");
            descLocalize.SetEntry(upgradeData.title + "Desc");
        }

        dynamicString.RefreshString();
        descText.gameObject.GetComponent<FixArabicText>().FixText(null);

        titleText.color = upgradeTextColor;
        titleLocalize.enabled = true;
        titleLocalize.SetEntry(upgradeData.title + "Title");
        titleLocalize.RefreshString();
        titleLocalize.enabled = false;
        titleText.gameObject.GetComponent<FixArabicText>().FixText(null);

        if (upgradeData.maxLevel > 1)
        {
            int oneIndexedLvl = selectedUpgradeOccurences.Count + selectedItemOccurences.Count + 1;
            if (oneIndexedLvl == upgradeData.maxLevel)
                titleText.text += " [MAX]";
            else
                titleText.text += " [lv " + oneIndexedLvl + "]";
        }
    }

    private void HideDescription()
    {
        if (popupObject) popupObject.SetActive(false);
    }

    private void HideCostText()
    {
        costText.gameObject.SetActive(false);
        currencyIcon.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            light2D.color = new Color(light2D.color.r, light2D.color.g, light2D.color.b, 1f);
            buyMe = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            light2D.color = new Color(light2D.color.r, light2D.color.g, light2D.color.b, 0f);
            buyMe = false;
        }
    }
}
