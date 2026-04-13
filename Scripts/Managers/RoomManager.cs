using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> normalBattleRooms;
    public List<GameObject> toughBattleRooms;
    public List<GameObject> eventRooms;
    public List<GameObject> healingRooms;
    public List<GameObject> shopRooms;
    public List<GameObject> freeItemRooms;
    public List<GameObject> bossRooms;
    public Transform roomParentTransform;

    [Space]
    [Space]
    public bool roomCleared;
    public bool fullyLooted = false;
    public bool isBossRoom = false;
    public bool isShopRoom = false;

    [Space]
    [Space]
    public GameObject player;
    [SerializeField] private AstarPath astarPath;
    [SerializeField] private MapManager mapManager;
    public int currentRoomNum = 0;
    public int currentBiomeNum = 0;

    [Space]
    [Space]
    [SerializeField] private List<SpriteRenderer> floorSprites;
    [SerializeField] private List<Color> biomeColors;
    [SerializeField] private int numDecorations;
    [SerializeField] private List<PrefabChancePair> groundDecorationPrefabs;
    [SerializeField] private BoxCollider2D roomBounds;

    [Space]
    [Space]
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip defaultMusic;

    private bool changedBackToNormalMusic = false;

    public List<GameObject> trashedObjectsAtRoomEnd;
    private bool openingMap = false;
    private bool firstRoom = true;

    private HitManager playerHitManager;

    public void Start()
    {
        playerHitManager = player.GetComponent<HitManager>();
        normalBattleRooms = Resources.LoadAll<GameObject>("NormalBattleRooms").ToList();
        toughBattleRooms = Resources.LoadAll<GameObject>("ToughBattleRooms").ToList();
        eventRooms = Resources.LoadAll<GameObject>("EventRooms").ToList();
        healingRooms = Resources.LoadAll<GameObject>("HealingRooms").ToList();
        shopRooms = Resources.LoadAll<GameObject>("ShopRooms").ToList();
        freeItemRooms = Resources.LoadAll<GameObject>("FreeItemRooms").ToList();
        bossRooms = Resources.LoadAll<GameObject>("BossRooms").ToList();
    }

    public void NewRoom(ProphecyData newRoomProphecy)
    {
        roomCleared = false;
        fullyLooted = false;
        isShopRoom = false;

        CleanUpRoom();
        ResetPositions();

        SpawnRandomRoom(newRoomProphecy);
        SpawnDecorations();

        currentRoomNum++;
        if (firstRoom)
        {
            currentRoomNum = 0;
            firstRoom = false;
        }
        if (currentRoomNum > 10)
        {
            currentBiomeNum++;
            currentRoomNum = 0;
        }

        SetBiomeColor();

        astarPath.Scan();
    }

    private void ResetPositions()
    {
        player.transform.position = new Vector3(6.25f, 5.6f, 0f);
        List<GameObject> items = player.GetComponent<PlayerUpgradesManager>().itemObjects;
        foreach (GameObject item in items)
        {
            item.transform.position = new Vector3(6.25f, 5.6f, 0f);
        }

        List<GameObject> playerBullets = GameObject.FindGameObjectsWithTag("AllyProjectile").ToList();
        foreach (GameObject bullet in playerBullets)
        {
            bullet.transform.position = new Vector3(6.25f, 5.6f, 0f);
        }
    }

    private void SetBiomeColor()
    {
        foreach (SpriteRenderer renderer in floorSprites)
        {
            renderer.color = biomeColors[Mathf.Min(currentBiomeNum, biomeColors.Count() - 1)];
        }
    }

    private void SpawnDecorations()
    {
        for (int x = 0; x < numDecorations; x++)
        {
            float rand = Random.Range(0f, 100f);
            float rx = Random.Range(roomBounds.transform.position.x + roomBounds.offset.x - roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x, roomBounds.transform.position.x + roomBounds.offset.x + roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x);
            float ry = Random.Range(roomBounds.transform.position.y + roomBounds.offset.y - roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y, roomBounds.transform.position.y + roomBounds.offset.y + roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y);
            Instantiate(SelectGameObjectFromRandom(rand, groundDecorationPrefabs), new Vector3(rx, ry, 0f), Quaternion.identity);
        }
    }

    private void CollectPickups()
    {
        GameObject[] pickupList = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickupList)
        {
            Pickup pickupScript = pickup.GetComponent<Pickup>();
            if (pickupScript) pickupScript.StartPickingUp();
        }
    }

    public void CleanUpRoom()
    {
        if (roomParentTransform.childCount > 0) Destroy(roomParentTransform.GetChild(0).gameObject);
        foreach (GameObject obj in trashedObjectsAtRoomEnd)
            Destroy(obj);
    }

    private void FixedUpdate()
    {
        CheckRoomClear();

        if (roomCleared)
        {
            if (isBossRoom)
            {
                GameObject.FindWithTag("music").GetComponent<AudioSource>().Stop();
                GameObject.FindWithTag("music").GetComponent<AudioSource>().PlayOneShot(defaultMusic);

                isBossRoom = false;
            }

            CollectPickups();
            CheckLooted();
            if (fullyLooted && !openingMap)
                StartCoroutine(OpenMap());
        }
    }

    public IEnumerator OpenMap(bool overrideRequirements = false)
    {
        if (openingMap) yield return null;

        openingMap = true;
        yield return new WaitForSeconds(0.05f);
        if (overrideRequirements || roomCleared && fullyLooted)
        {
            mapManager.OpenMap();
            openingMap = false;
        }
    }

    public void NextMap()
    {
        mapManager.regenerate = true;
    }

    private void CheckRoomClear()
    {
        roomCleared = true;
        if (GameObject.FindWithTag("EnemyBody") != null) roomCleared = false;
    }

    private void CheckLooted()
    {
        fullyLooted = true;
        if (GameObject.FindWithTag("Pickup") != null || GameObject.FindWithTag("Upgrade") != null)
        {
            fullyLooted = false;
        }
    }

    private void SpawnRandomRoom(ProphecyData prophecy)
    {
        List<GameObject> roomOptions = new();
        switch (prophecy.roomType)
        {
            case RoomType.Battle:
                roomOptions = normalBattleRooms;
                break;
            case RoomType.ToughBattle:
                roomOptions = toughBattleRooms;
                break;
            case RoomType.Event:
                roomOptions = eventRooms;
                break;
            case RoomType.Healing:
                roomOptions = healingRooms;
                break;
            case RoomType.Shop:
                isShopRoom = true;
                roomOptions = shopRooms;
                break;
            case RoomType.FreeItem:
                roomOptions = freeItemRooms;
                break;
            case RoomType.Boss:
                isBossRoom = true;
                GameObject.FindWithTag("music").GetComponent<AudioSource>().Stop();
                GameObject.FindWithTag("music").GetComponent<AudioSource>().PlayOneShot(bossMusic);
                roomOptions = bossRooms;
                break;
        }
        int rand = Random.Range(0, roomOptions.Count);
        GameObject selection = roomOptions[rand];

        Instantiate(selection, transform.position, Quaternion.identity, roomParentTransform);
    }

    private GameObject SelectGameObjectFromRandom(float rand, List<PrefabChancePair> prefabList)
    {
        float totalChance = 0;
        foreach (PrefabChancePair prefabChancePair in prefabList)
        {
            totalChance += prefabChancePair.chance;
        }

        float divisor = totalChance / 100f;

        float cumulativeChance = 0;
        foreach (PrefabChancePair prefabChancePair in prefabList)
        {
            cumulativeChance += prefabChancePair.chance;
            if (cumulativeChance / divisor >= rand)
            {
                return prefabChancePair.prefab;
            }
        }
        return null;
    }

    public Vector3 ClampWithinRoom(Vector3 startPos)
    {
        float xmin = roomBounds.transform.position.x + roomBounds.offset.x - roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x;
        float xmax = roomBounds.transform.position.x + roomBounds.offset.x + roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x;

        float ymin = roomBounds.transform.position.y + roomBounds.offset.y - roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y;
        float ymax = roomBounds.transform.position.y + roomBounds.offset.y + roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y;

        float clampedX = Mathf.Max(xmin, Mathf.Min(startPos.x, xmax));
        float clampedY = Mathf.Max(ymin, Mathf.Min(startPos.y, ymax));

        Vector3 clampedEndPos = new(clampedX, clampedY, 0f);

        return clampedEndPos;
    }

    public void TeleportWithinRoom(GameObject toTp, Transform teleportNear, float minDistance, float maxDistance)
    {
        float xmin = roomBounds.transform.position.x + roomBounds.offset.x - roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x;
        float xmax = roomBounds.transform.position.x + roomBounds.offset.x + roomBounds.size.x / 2 * roomBounds.transform.lossyScale.x;

        float ymin = roomBounds.transform.position.y + roomBounds.offset.y - roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y;
        float ymax = roomBounds.transform.position.y + roomBounds.offset.y + roomBounds.size.y / 2 * roomBounds.transform.lossyScale.y;

        float rx = Random.Range(xmin, xmax);
        float ry = Random.Range(ymin, ymax);

        Vector3 startRandomPos = new(rx, ry, 0f);

        Vector3 vectorToward = teleportNear.position - startRandomPos;

        float movementDistance = vectorToward.magnitude - Random.Range(minDistance, maxDistance);

        Vector3 movementVector = vectorToward.normalized * movementDistance;
        Vector3 endPos = startRandomPos + movementVector;

        float clampedX = Mathf.Max(xmin, Mathf.Min(endPos.x, xmax));
        float clampedY = Mathf.Max(ymin, Mathf.Min(endPos.y, ymax));

        Vector3 clampedEndPos = new(clampedX, clampedY, 0f);

        toTp.transform.position = clampedEndPos;
    }
}

[System.Serializable]
public class PrefabChancePair
{
    public GameObject prefab;
    public float chance;
}

