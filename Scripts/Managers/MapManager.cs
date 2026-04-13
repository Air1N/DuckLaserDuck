using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private RoomManager roomManager;

    [SerializeField] private GameObject mapIconPrefab;
    [SerializeField] private GameObject mapConnectionPrefab;
    [SerializeField] private List<ProphecyData> possibleRooms;

    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private Vector2 spacing;
    [SerializeField] private Vector2 jiggle;
    [SerializeField] private Transform startTransform;
    [SerializeField] public GameObject bossIcon;

    public Transform contentTransform;
    [SerializeField] private Transform iconParent;

    [SerializeField] private int currentActiveFloor = 0;

    private int maxNextRooms = 3;

    private MapRoomData[][] mapLayout;
    private MapRoomData bossIconRoomData;
    public bool regenerate = true;

    private List<GameObject> currentMapIcons;
    private List<Button> activeMapButtons;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private Sprite lineImage;
    [SerializeField] private float lineWidth;
    [SerializeField] private Vector2 graphScale;
    [SerializeField] private Canvas canvas;

    public void OnEnable()
    {
        if (regenerate)
        {
            bossIconRoomData = new()
            {
                button = bossIcon.GetComponent<Button>(),
                gridPosition = new Vector2Int(0, mapSize.y),
                spritePosition = bossIcon.transform.position + Vector3.down * 2f,
                localSpritePosition = bossIcon.transform.localPosition + Vector3.down * 2f,
                roomType = RoomType.Boss
            };
            bossIconRoomData.button.onClick.RemoveAllListeners();
            bossIconRoomData.button.onClick.AddListener(delegate { SpawnRoom(bossIconRoomData); });
            bossIconRoomData.button.interactable = false;

            Debug.Log("Making new map");
            currentActiveFloor = 0;
            currentMapIcons = new();
            activeMapButtons = new();
            foreach (Transform child in iconParent)
            {
                Debug.Log("Destroying stuff");
                Destroy(child.gameObject);
            }

            spacing.x = canvas.pixelRect.width / (mapSize.x + 1) / 2.1545f;
            spacing.y = (bossIcon.transform.position.y - 100f - startTransform.position.y) / (mapSize.y + 1);
            jiggle.y = spacing.y / 4;
            jiggle.x = spacing.x / 4;

            GenerateMap();

            regenerate = false;
        }
    }

    private void GenerateMap()
    {
        mapLayout = new MapRoomData[mapSize.y][];
        for (int y = 0; y < mapSize.y; y++)
        {
            mapLayout[y] = new MapRoomData[mapSize.x];
            for (int x = 0; x < mapSize.x; x++)
            {
                MapRoomData roomData = new()
                {
                    gridPosition = new Vector2Int(x, y)
                };

                mapLayout[y][x] = roomData;
            }
        }

        GenFirstFloor();

        for (int y = 0; y < mapSize.y - 1; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                if (mapLayout[y][x].roomType != RoomType.Null)
                {
                    int randNextRoomsAmount = Random.Range(1, maxNextRooms + 1);
                    for (int i = 0; i < randNextRoomsAmount; i++)
                    {
                        GenNextRoom(x, y);
                    }
                }
            }
        }

        for (int x = 0; x < mapSize.x; x++)
        {
            if (mapLayout[mapSize.y - 1][x].roomType != RoomType.Null)
                ConnectToBoss(x);
        }

        ConstructMapUI();
        SetActiveButtons(true, 0);
    }

    private void GenFirstFloor()
    {
        List<float> pickedNums = new();
        int tries = 0;
        while (pickedNums.Count < 2 || tries < mapSize.x)
        {
            int pickedX = Random.Range(0, mapSize.x);
            pickedNums.Add(pickedX);
            tries++;
        }

        foreach (int xPosition in pickedNums)
        {
            SetRoomType(xPosition, 0, RoomType.Battle);
        }
    }

    private void GenNextRoom(int x, int y)
    {
        MapRoomData currentRoom = mapLayout[y][x];

        int nextRoomDir = Random.Range(0, 3); // 0-2
        int newX = Mathf.Clamp(x + (nextRoomDir - 1), 0, mapSize.x - 1);
        int newY = y + 1;

        while (ConnectionWouldCross(x, y, newX))
        {
            nextRoomDir = Random.Range(0, 3); // 0-2
            newX = Mathf.Clamp(x + (nextRoomDir - 1), 0, mapSize.x - 1);
        }

        RoomType nextRoomType = SelectRandomProphecy(newY).roomType;
        if (newY == 5)
            nextRoomType = RoomType.Shop;
        else
            while ((currentRoom.roomType == RoomType.Healing || currentRoom.roomType == RoomType.Shop || currentRoom.roomType == RoomType.FreeItem || currentRoom.roomType == RoomType.ToughBattle) && nextRoomType == currentRoom.roomType)
            {
                nextRoomType = SelectRandomProphecy(newY).roomType;
            }

        SetRoomType(newX, newY, nextRoomType);
        mapLayout[y][x].nextRooms.Add(mapLayout[newY][newX]);
    }

    private void ConnectToBoss(int x)
    {
        mapLayout[mapSize.y - 1][x].nextRooms.Add(bossIconRoomData);
    }

    private void SetRoomType(int x, int y, RoomType roomType)
    {
        mapLayout[y][x].roomType = roomType;
    }

    private bool ConnectionWouldCross(int x, int y, int newX)
    {
        MapRoomData rightNeighbor = null;
        MapRoomData leftNeighbor = null;

        if (x > 0)
            leftNeighbor = mapLayout[y][x - 1];
        else if (x < mapSize.x - 1)
            rightNeighbor = mapLayout[y][x + 1];

        if (leftNeighbor != null && newX < x)
        {
            foreach (MapRoomData nextRoom in leftNeighbor.nextRooms)
            {
                if (nextRoom.gridPosition.x > newX) return true;
            }
        }

        if (rightNeighbor != null && newX > x)
        {
            foreach (MapRoomData nextRoom in rightNeighbor.nextRooms)
            {
                if (nextRoom.gridPosition.x < newX) return true;
            }
        }

        return false;
    }

    private void ConstructMapUI()
    {
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                SetSpritePosition(mapLayout[y][x]);
            }
        }

        Color lineColor = new(1f, 1f, 1f, 0.08f);
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                MapRoomData currentRoom = mapLayout[y][x];
                foreach (MapRoomData nextRoom in currentRoom.nextRooms)
                {
                    MakeLine(currentRoom.localSpritePosition.x, currentRoom.localSpritePosition.y, nextRoom.localSpritePosition.x, nextRoom.localSpritePosition.y, lineColor);
                }
            }
        }

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                InstanceSprite(mapLayout[y][x]);
            }
        }
    }

    private void SetSpritePosition(MapRoomData mapRoom)
    {
        if (mapRoom.roomType == RoomType.Null) return;

        mapRoom.spritePosition = startTransform.position + new Vector3((mapRoom.gridPosition.x + 0.5f - mapSize.x / 2f) * spacing.x + Random.Range(-jiggle.x, jiggle.x), (mapRoom.gridPosition.y + 1) * spacing.y + Random.Range(-jiggle.y, jiggle.y), 0f);
        mapRoom.localSpritePosition = iconParent.InverseTransformPoint(mapRoom.spritePosition);
    }

    private void InstanceSprite(MapRoomData mapRoom)
    {
        if (mapRoom.roomType == RoomType.Null) return;

        GameObject mapIcon = Instantiate(mapIconPrefab, mapRoom.spritePosition, Quaternion.identity, iconParent);
        mapIcon.GetComponent<Image>().sprite = GetRoomTypeData(mapRoom.roomType).sprite;

        mapRoom.button = mapIcon.GetComponent<Button>();
        mapRoom.button.interactable = false;
        mapRoom.button.onClick.AddListener(delegate { SpawnRoom(mapRoom); });

        currentMapIcons.Add(mapIcon);
    }

    private float SortLeftToRight(Button a, Button b)
    {
        return a.transform.position.x - b.transform.position.x;
    }

    private void GenerateControllerUIMovement()
    {
        // Sort by X position first
        activeMapButtons.OrderBy(item => item.transform.position.x);

        for (int i = 0; i < activeMapButtons.Count; i++)
        {
            Button leftButton = null;
            Button rightButton = null;

            if (i > 0) leftButton = activeMapButtons[i - 1];
            if (i < activeMapButtons.Count - 1) rightButton = activeMapButtons[i + 1];

            Navigation navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = leftButton,
                selectOnRight = rightButton
            };

            activeMapButtons[i].navigation = navigation;
        }
    }

    private void RemoveControllerUIMovement()
    {
        for (int i = 0; i < activeMapButtons.Count; i++)
        {
            Navigation navigation = new()
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = null,
                selectOnRight = null
            };

            activeMapButtons[i].navigation = navigation;
        }
    }

    public void SpawnRoom(MapRoomData mapRoom)
    {
        roomManager.NewRoom(GetRoomTypeData(mapRoom.roomType));

        RemoveControllerUIMovement();

        activeMapButtons = new();
        SetActiveButtons(false, currentActiveFloor);
        SetNextRoomsActive(mapRoom);

        CloseMap();

        currentActiveFloor++;
    }

    private void SetActiveButtons(bool active, int floor)
    {
        // This mainly disables buttons from the previous floor
        if (floor >= mapSize.y) return;

        for (int x = 0; x < mapSize.x; x++)
        {
            if (mapLayout[floor][x].roomType != RoomType.Null)
            {
                mapLayout[floor][x].button.interactable = active;
                if (active) activeMapButtons.Add(mapLayout[floor][x].button);
            }
        }
    }

    private void SetNextRoomsActive(MapRoomData mapRoom)
    {
        if (currentActiveFloor >= mapSize.y) return;

        foreach (MapRoomData nextRoom in mapRoom.nextRooms)
        {
            nextRoom.button.interactable = true;
            activeMapButtons.Add(nextRoom.button);
        }
    }

    public void OpenMap()
    {
        mapMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(activeMapButtons[0].gameObject);

        GenerateControllerUIMovement();

        Time.timeScale = 0f;
    }

    public void CloseMap()
    {
        mapMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    private ProphecyData GetRoomTypeData(RoomType roomType)
    {
        return possibleRooms.Where(pRoom => pRoom.roomType == roomType).First();
    }

    private float CurrentChanceOfRoom(int floor, RoomType roomType)
    {
        List<RoomNumChancePair> roomNumChancePairs = GetRoomTypeData(roomType).roomNumChancePairs;

        if (roomNumChancePairs.Count > 0)
            return roomNumChancePairs.Where(p => p.roomNum <= floor).Last().chance;
        else return 0;
    }

    private ProphecyData SelectRandomProphecy(int floor)
    {
        float rand = Random.Range(0f, 100f);
        Debug.Log("Select random room, floor=" + floor);

        float totalChance = 0;
        foreach (ProphecyData prophecyData in possibleRooms)
        {
            float chanceAtRoom = CurrentChanceOfRoom(floor, prophecyData.roomType);
            totalChance += chanceAtRoom;
        }
        float divisor = totalChance / 100f;

        float cumulativeChance = 0;
        foreach (ProphecyData prophecyData in possibleRooms)
        {
            float chanceAtRoom = CurrentChanceOfRoom(floor, prophecyData.roomType);
            // Debug.Log(prophecyData.roomType + " chance=" + chanceAtRoom);
            cumulativeChance += chanceAtRoom;
            if (cumulativeChance / divisor >= rand)
                return prophecyData;
        }


        return null;
    }

    private void MakeLine(float ax, float ay, float bx, float by, Color col)
    {
        GameObject NewObj = new GameObject();
        NewObj.name = "line from " + ax + " to " + bx;
        Image NewImage = NewObj.AddComponent<Image>();
        NewImage.sprite = lineImage;
        NewImage.color = col;
        RectTransform rect = NewObj.GetComponent<RectTransform>();
        rect.SetParent(iconParent);
        rect.localScale = Vector3.one;

        Vector3 a = new Vector3(ax * graphScale.x, ay * graphScale.y, 0);
        Vector3 b = new Vector3(bx * graphScale.x, by * graphScale.y, 0);

        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        rect.sizeDelta = new Vector3(dif.magnitude, lineWidth);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }
}

public enum RoomType
{
    Null = -1,
    Battle = 0,
    Healing = 1,
    Shop = 2,
    FreeItem = 3,
    Event = 4,
    Boss = 5,
    ToughBattle = 6
};

[System.Serializable]
public class MapRoomData
{
    public RoomType roomType = RoomType.Null;
    public List<MapRoomData> nextRooms = new();
    public Vector2Int gridPosition;
    public Vector3 spritePosition;
    public Vector3 localSpritePosition;

    public Button button;
}

[System.Serializable]
public class ProphecyData
{
    public List<RoomNumChancePair> roomNumChancePairs;
    public RoomType roomType;
    public Sprite sprite;
}

[System.Serializable]
public class RoomNumChancePair
{
    public int roomNum;
    public int chance;
}
