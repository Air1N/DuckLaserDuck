using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class PlayerBankManager : MonoBehaviour
{
    public int worms = 0;
    public int greenGrubs = 0;

    private int lastWorms = 0;
    private int lastGreenGrubs = 0;
    public SaveGameManager saveGameManager;
    private StatsAndAchievements statsAndAchievements;

    private void Start()
    {
        statsAndAchievements = FindObjectOfType<StatsAndAchievements>();
        greenGrubs = saveGameManager.saveData.m_greenGrubs;
    }

    private void FixedUpdate()
    {
        if (worms != lastWorms)
        {
            statsAndAchievements.UpdateMaxWormsCollected(worms);

            saveGameManager.saveData.m_totalLifetimeWormsCollected += worms - lastWorms;
            statsAndAchievements.m_intTotalWormsCollected = saveGameManager.saveData.m_totalLifetimeWormsCollected;

            saveGameManager.SaveJsonData();
        }

        if (greenGrubs != lastGreenGrubs)
        {
            saveGameManager.saveData.m_greenGrubs = greenGrubs;
            saveGameManager.saveData.m_totalLifetimeGreenGrubsCollected += greenGrubs - lastGreenGrubs;

            saveGameManager.SaveJsonData();
        }

        lastWorms = worms;
        lastGreenGrubs = greenGrubs;
    }
}
