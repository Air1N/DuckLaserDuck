using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [System.Serializable]
    public struct EnemyData
    {
        public GameObject m_Prefab;
        public int m_Health;
        public Vector3 m_Position;
    }

    public int m_greenGrubs;

    public int m_totalLifetimeWormsCollected;
    public int m_totalLifetimeGreenGrubsCollected;
    public float m_totalLifetimeDamageDealt;

    public List<int> m_UnlockedIds = new();
    public List<GameObject> m_currentRunUpgrades = new();
    public List<EnemyData> m_currentRunEnemyData = new();
    public int m_currentRunRoomID;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}