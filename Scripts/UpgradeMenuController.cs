using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.Localization;

public class UpgradeMenuController : MonoBehaviour
{
    private List<GameObject> upgradeOptions;
    [SerializeField] public GameObject player;
    [SerializeField] private PlayerUpgradesManager upgradeManager;
    [SerializeField] private MapManager mapManager;
    int rand;

    [SerializeField] private List<UpgradeSlot> upgradeSlots;
    [SerializeField] private List<Transform> slotTransforms;

    [SerializeField] private List<float> rollSpeed = new() { 5000f, 5000f, 5000f };
    [SerializeField] private List<bool> upgradesChosen = new() { false, false, false };
    [SerializeField] private List<string> skippedOne = new() { "", "", "" };
    [SerializeField] private List<int> skippedCount = new() { 0, 0, 0 };
    [SerializeField] private List<int> amountToSkip = new() { 0, 0, 0 };

    [SerializeField] private float topPosition = 430f;
    [SerializeField] private float bottomPosition = -400f;
    [SerializeField] private float lockedInHeight = 313f;

    [SerializeField] private GameObject titlePrefab;
    [SerializeField] private int numTitles = 3;
    [SerializeField] private float titleSpacing = 500f;

    [SerializeField] private Button pullArmButton;
    [SerializeField] private RerollButton rerollButtonScript;

    public bool shooting = false;
    private bool neverInstancedTitles = true;
    [SerializeField] private PlayerController playerController;

    public bool animationStarted = false;

    void Start()
    {
        bottomPosition = topPosition - titleSpacing * numTitles;

        if (neverInstancedTitles)
        {
            InstanceNewTitles();
            Reset();
            neverInstancedTitles = false;

            foreach (UpgradeSlot slot in upgradeSlots)
            {
                foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
                {
                    PickAndShowRandomUpgradeIcon(upgradeIcon);
                }
            }
        }
    }

    void OnEnable()
    {
        rerollButtonScript.pullCount = 0;
        Reset();
    }

    public void Reset()
    {
        Time.timeScale = 0f;
        upgradeOptions = Resources.LoadAll<GameObject>("UpgradePrefabs").ToList();

        rollSpeed = new() { 10000f, 10000f, 10000f };
        upgradesChosen = new() { false, false, false };
        skippedOne = new() { "", "", "" };

        skippedCount = new() { 0, 0, 0 };
        amountToSkip = new() { Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2) }; // 1,3 is 1-2

        animationStarted = false;

        SetOtherIconsActive(true);
        ResetTitlePositions();

        foreach (UpgradeSlot slot in upgradeSlots)
        {
            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                upgradeIcon.button.interactable = false;

                upgradeIcon.infoPanel.SetActive(true);
                PickAndShowRandomUpgradeIcon(upgradeIcon);
                upgradeIcon.infoPanel.SetActive(false);
            }
        }
    }

    private void SlowdownRolls()
    {
        if (!animationStarted) return;

        for (int i = 0; i < upgradeSlots.Count; i++)
        {
            if (rollSpeed[i] > 10f) rollSpeed[i] = Mathf.Max(10f, rollSpeed[i] - 20000f / Mathf.Sqrt(i + 1) * Time.unscaledDeltaTime);
        }
    }

    void Update()
    {
        if (!upgradesChosen.All(x => x == true)) SlotMachineAnimation();
        else if (animationStarted)
        {
            animationStarted = false;
            GenerateControllerUIMovement();
        }

        if (shooting) SlowdownRolls();
    }

    private void SlotMachineAnimation()
    {
        if (!animationStarted) return;

        int slotIdx = -1;
        foreach (UpgradeSlot slot in upgradeSlots)
        {
            slotIdx++;
            float rSpeed = rollSpeed[slotIdx];

            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                Vector2 startPos = upgradeIcon.iconRectTransform.anchoredPosition;
                upgradeIcon.iconRectTransform.anchoredPosition += Vector2.down * rSpeed * Time.unscaledDeltaTime;

                if (rSpeed > 0f && upgradeIcon.iconRectTransform.anchoredPosition.y < bottomPosition)
                {
                    // Move back to the top of the roller thing
                    upgradeIcon.iconRectTransform.anchoredPosition += Vector2.up * (topPosition - bottomPosition);
                }
                else if (rSpeed < 0f && upgradeIcon.iconRectTransform.anchoredPosition.y > topPosition)
                {
                    // Backward rolling
                    upgradeIcon.iconRectTransform.anchoredPosition += Vector2.down * (topPosition - bottomPosition);
                }
                if (rSpeed < 0f && Mathf.Abs(upgradeIcon.iconRectTransform.anchoredPosition.y - lockedInHeight) < 20f)
                {
                    if (skippedOne[slotIdx] != "" && upgradeIcon.upgradeSelected.title != skippedOne[slotIdx])
                    {
                        if (skippedCount[slotIdx] < amountToSkip[slotIdx] - 1)
                        {
                            skippedCount[slotIdx] += 1;
                            skippedOne[slotIdx] = "";
                            continue;
                        }

                        // Move one title to center and lock in
                        ShowFullyChosenUpgrade(slot, upgradeIcon);
                        rollSpeed[slotIdx] = 0f;
                        upgradesChosen[slotIdx] = true;
                    }
                    else
                    {
                        skippedOne[slotIdx] = upgradeIcon.upgradeSelected.title;
                    }
                }
            }

            if (!upgradesChosen[slotIdx])
            {
                if (rSpeed < 5000f) rollSpeed[slotIdx] -= 1500f * Time.unscaledDeltaTime;
            }
        }

    }

    private void InstanceNewTitles()
    {
        for (int i = 0; i < slotTransforms.Count; i++)
        {
            for (int j = 0; j < numTitles; j++)
            {
                GameObject newIconGameObject = Instantiate(titlePrefab, Vector3.zero, Quaternion.identity, slotTransforms[i]);
                SlotMachineUpgradeIconData slotMachineUpgradeIconData = newIconGameObject.GetComponent<SlotMachineUpgradeIconData>();

                UpgradeIcon newIcon = new()
                {
                    iconRectTransform = newIconGameObject.GetComponent<RectTransform>(),

                    infoPanel = slotMachineUpgradeIconData.panel,
                    imageRenderer = slotMachineUpgradeIconData.imageRenderer,

                    titleTextObj = slotMachineUpgradeIconData.titleTextObj,
                    titleLocalizeStringEvent = slotMachineUpgradeIconData.titleLocalizeStringEvent,

                    descTextObj = slotMachineUpgradeIconData.descTextObj,
                    descLocalizeStringEvent = slotMachineUpgradeIconData.descLocalizeStringEvent,

                    button = newIconGameObject.GetComponentInChildren<Button>()
                };

                newIcon.button.interactable = false;

                int idx = i;
                newIcon.button.onClick.AddListener(delegate { SelectUpgrade(idx); });
                newIcon.iconRectTransform.anchoredPosition = new Vector2(0f, 0f);

                upgradeSlots[i].upgradeIcons.Add(newIcon);
            }
        }
    }

    private void ResetTitlePositions()
    {
        foreach (UpgradeSlot slot in upgradeSlots)
        {
            int titleIdx = 0;
            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                Vector2 startPos = upgradeIcon.iconRectTransform.anchoredPosition;
                upgradeIcon.iconRectTransform.anchoredPosition = new Vector2(startPos.x, topPosition - titleSpacing * titleIdx);
                titleIdx++;
            }
        }
    }

    private void SetOtherIconsActive(bool active)
    {
        foreach (UpgradeSlot slot in upgradeSlots)
        {
            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                if (!active && slot.upgradeChosen.GetComponent<UpgradeData>().title != upgradeIcon.upgradeSelected.title)
                {
                    upgradeIcon.imageRenderer.gameObject.SetActive(false);
                }
                else
                {
                    upgradeIcon.imageRenderer.gameObject.SetActive(true);
                }
            }
        }
    }

    private void PickAndShowRandomUpgradeIcon(UpgradeIcon upgradeIcon)
    {
        rand = Random.Range(0, upgradeOptions.Count);
        upgradeIcon.upgradeSelected = upgradeOptions[rand].GetComponent<UpgradeData>();

        int tries = 0;
        while (tries < 5000 && (CheckRepeatsAndNulls(upgradeIcon) || upgradeManager.CheckOverLevel(upgradeIcon.upgradeSelected) || !upgradeManager.CheckRequirements(upgradeIcon.upgradeSelected.requirements) || !upgradeManager.RollRarity(upgradeIcon.upgradeSelected.rarity)))
        {
            rand = Random.Range(0, upgradeOptions.Count);
            upgradeIcon.upgradeSelected = upgradeOptions[rand].GetComponent<UpgradeData>();

            tries++;
        }

        if (tries >= 5000)
            Debug.LogError("over 5000 tries to find upgrade. probably will bug.");

        upgradeIcon.imageRenderer.sprite = upgradeIcon.upgradeSelected.sprite;
    }

    private void ShowFullyChosenUpgrade(UpgradeSlot upgradeSlot, UpgradeIcon upgradeIcon)
    {
        upgradeIcon.infoPanel.SetActive(true);

        List<string> selectedUpgradeOccurences = upgradeManager.upgrades.FindAll(
            x => x.Equals(upgradeIcon.upgradeSelected.title)
        );

        List<string> selectedItemOccurences = upgradeManager.items.FindAll(
            x => x.Equals(upgradeIcon.upgradeSelected.title)
        );

        upgradeIcon.button.interactable = true;
        upgradeIcon.iconRectTransform.anchoredPosition = new Vector2(upgradeIcon.iconRectTransform.anchoredPosition.x, lockedInHeight);
        upgradeSlot.upgradeChosen = upgradeOptions[upgradeOptions.IndexOf(upgradeIcon.upgradeSelected.gameObject)];

        Color titleTextColor = Color.HSVToRGB(
            (0.92f + upgradeIcon.upgradeSelected.rarity * 0.065f) % 1f,
            1f,
            0.94f
        );

        Color descTextColor = Color.HSVToRGB(
            (0.9f + upgradeIcon.upgradeSelected.rarity * 0.065f) % 1f,
            1f,
            0.92f
        );

        UpgradeToPlayer upgradeToPlayer = upgradeSlot.upgradeChosen.GetComponent<UpgradeToPlayer>();
        LocalizedString dynamicString = new LocalizedString();
        dynamicString.TableReference = "StringLocalizationTable";
        dynamicString.TableEntryReference = upgradeIcon.upgradeSelected.title + "Desc";

        if (upgradeToPlayer)
        {
            upgradeIcon.descTextObj.text = UpgradeDescriptionSetter.GetLocalizedDescription(dynamicString, upgradeManager, upgradeToPlayer, upgradeIcon.upgradeSelected.title + "Desc", false);
            upgradeIcon.descTextObj.color = descTextColor;
        }
        else
        {
            upgradeIcon.descLocalizeStringEvent.SetEntry(upgradeIcon.upgradeSelected.title + "Desc");
        }

        dynamicString.RefreshString();
        upgradeIcon.descTextObj.gameObject.GetComponent<FixArabicText>().FixText(null);

        upgradeIcon.titleTextObj.color = titleTextColor;
        upgradeIcon.titleLocalizeStringEvent.enabled = true;
        upgradeIcon.titleLocalizeStringEvent.SetEntry(upgradeIcon.upgradeSelected.title + "Title");
        upgradeIcon.titleLocalizeStringEvent.RefreshString();

        upgradeIcon.titleTextObj.gameObject.GetComponent<FixArabicText>().FixText(null);

        upgradeIcon.titleLocalizeStringEvent.enabled = false;


        if (upgradeIcon.upgradeSelected.maxLevel > 1)
        {
            int oneIndexedLvl = selectedUpgradeOccurences.Count + selectedItemOccurences.Count + 1;
            if (oneIndexedLvl == upgradeIcon.upgradeSelected.maxLevel)
                upgradeIcon.titleTextObj.text += " [MAX]";
            else
                upgradeIcon.titleTextObj.text += " [lv " + oneIndexedLvl + "]";
        }

        upgradeIcon.infoPanel.SetActive(false);
    }

    public void SelectUpgrade(int slotIndex)
    {
        GameObject upgradeObject = upgradeOptions[upgradeOptions.IndexOf(upgradeSlots[slotIndex].upgradeChosen)];

        GameObject upgradePrefab = Instantiate(
            upgradeObject,
            Vector3.zero,
            Quaternion.identity
        );

        SetOtherIconsActive(true);
        Time.timeScale = 1f;
        HideUpgradeMenu();
    }

    private void HideUpgradeMenu()
    {
        gameObject.SetActive(false);
    }

    bool CheckRepeatsAndNulls(UpgradeIcon titleToCheck = null, string titleString = null)
    {
        if (titleToCheck.upgradeSelected == null) return true;

        foreach (UpgradeSlot slot in upgradeSlots)
        {
            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                if (upgradeIcon != titleToCheck && upgradeIcon.upgradeSelected && upgradeIcon.upgradeSelected.title == titleToCheck.upgradeSelected.title)
                    return true;
            }
        }

        return false;
    }

    private void GenerateControllerUIMovement()
    {
        List<Button> selectedUpgradeButtons = new();

        foreach (UpgradeSlot slot in upgradeSlots)
        {
            foreach (UpgradeIcon upgradeIcon in slot.upgradeIcons)
            {
                upgradeIcon.button.navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = null,
                    selectOnRight = null
                };

                if (slot.upgradeChosen.GetComponent<UpgradeData>().title == upgradeIcon.upgradeSelected.title)
                {
                    selectedUpgradeButtons.Add(upgradeIcon.button);
                }
            }
        }

        selectedUpgradeButtons.Add(pullArmButton);
        selectedUpgradeButtons.OrderBy(item => item.transform.position.x);

        for (int i = 0; i < selectedUpgradeButtons.Count; i++)
        {
            Button leftButton = null;
            Button rightButton = null;

            if (i > 0) leftButton = selectedUpgradeButtons[i - 1];
            if (i < selectedUpgradeButtons.Count - 1) rightButton = selectedUpgradeButtons[i + 1];

            Navigation navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = leftButton,
                selectOnRight = rightButton
            };

            selectedUpgradeButtons[i].navigation = navigation;
        }
    }

    [System.Serializable]
    class UpgradeSlot
    {
        public List<UpgradeIcon> upgradeIcons;
        public GameObject upgradeChosen;
    }

    [System.Serializable]
    class UpgradeIcon
    {
        public RectTransform iconRectTransform;

        public GameObject infoPanel;
        public Image imageRenderer;

        public TextMeshProUGUI titleTextObj;
        public LocalizeStringEvent titleLocalizeStringEvent;

        public TextMeshProUGUI descTextObj;
        public LocalizeStringEvent descLocalizeStringEvent;

        public UpgradeData upgradeSelected;

        public Button button;
    }
}
