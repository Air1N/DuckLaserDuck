using UnityEngine;

public class Worms : MonoBehaviour
{
    public int minWormsOnKill;
    public int maxWormsOnKill;
    [SerializeField] private GameObject wormPrefab;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Vector2 randomOffsetMax;
    [SerializeField] private PlayerBankManager playerBankManager;

    public bool giveDirectly;

    public void Start()
    {
        if (giveDirectly)
        {
            playerBankManager = FindFirstObjectByType<PlayerBankManager>();

            int totalWormAmount = Random.Range(minWormsOnKill, maxWormsOnKill + 1);
            playerBankManager.worms += totalWormAmount;
        }
    }

    public void dropWorms()
    {
        int totalWormAmount = Random.Range(minWormsOnKill, maxWormsOnKill + 1);
        int giveWorms = totalWormAmount;

        int wormValue = 1;
        float wormScale = 1f;

        if (totalWormAmount > 50)
        {
            giveWorms = Mathf.FloorToInt(totalWormAmount / 10) + totalWormAmount % 10;
        }

        for (int i = 0; i < giveWorms; i++)
        {
            if (i < Mathf.FloorToInt(totalWormAmount / 10))
            {
                wormValue = 10;
                wormScale = 2f;
            }
            else
            {
                wormValue = 1;
                wormScale = 1f;
            }

            Vector2 trueSpawnPosition = (Vector2)spawnPosition.position + randomOffsetMax * Random.Range(-4f, 4f);
            GameObject wormObject = Instantiate(wormPrefab, trueSpawnPosition, Quaternion.identity, null);
            wormObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f) + 8f), ForceMode2D.Impulse);

            wormObject.GetComponent<Pickup>().worms = wormValue;
            wormObject.transform.localScale = new Vector3(wormScale, wormScale, wormScale);
        }
    }
}
