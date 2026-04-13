using UnityEngine;

public class UpgradeData : MonoBehaviour
{
    public Sprite sprite;
    public int rarity;
    public string title;
    public string description;
    public string[] requirements;
    public int maxLevel;

    public bool parentToPlayerHead = false;
    public bool parentToPlayerFeet = false;

    void Start()
    {
        GameObject player = GameObject.Find("char");
        PlayerUpgradesManager upgradeManager = player.GetComponent<PlayerUpgradesManager>();

        upgradeManager.upgrades.Add(title);
        upgradeManager.AddItemToInventory(gameObject);

        Debug.Log("Got upgrade " + title);

        if (parentToPlayerHead)
        {
            transform.position = upgradeManager.projectileHeadSpawnPoint.position;
            transform.parent = upgradeManager.projectileHeadSpawnPoint;
        }

        if (parentToPlayerFeet)
        {
            transform.position = upgradeManager.projectileFeetSpawnPoint.position;
            transform.parent = upgradeManager.projectileFeetSpawnPoint;
        }
    }

}
