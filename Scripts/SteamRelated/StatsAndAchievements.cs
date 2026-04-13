using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class StatsAndAchievements : MonoBehaviour
{
    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    private CGameID m_GameID;

    private bool m_bRequestedStats;
    private bool m_bStatsValid;

    public bool m_bOpenedGame;
    private bool m_bStoreStats;

    public float m_flMaxSingleHitDmg;
    public int m_intMaxWormsCollected;
    public int m_intTotalWormsCollected;
    public float m_flTotalDamageDealt;

    private bool m_bHasOpenGame;
    private bool m_bHasWarhead;
    private bool m_bHasWarmachine;
    private bool m_bHasMillionaire;

    void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;

        m_GameID = new CGameID(SteamUtils.GetAppID());

        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        m_bRequestedStats = false;
        m_bStatsValid = false;
    }

    //-----------------------------------------------------------------------------
    // Purpose: We have stats data from Steam. It is authoritative, so update
    //			our data with those results now.
    //-----------------------------------------------------------------------------
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");

                m_bStatsValid = true;

                // load stats
                SteamUserStats.GetStat("single_hit_dmg", out m_flMaxSingleHitDmg);
                SteamUserStats.GetStat("max_worms", out m_intMaxWormsCollected);
                SteamUserStats.GetStat("total_worms_collected", out m_intTotalWormsCollected);
                SteamUserStats.GetStat("total_dmg_dealt", out m_flTotalDamageDealt);

                SteamUserStats.GetAchievement("WARHEAD", out m_bHasWarhead);
                SteamUserStats.GetAchievement("MILLIONAIRE", out m_bHasMillionaire);
                SteamUserStats.GetAchievement("WARMACHINE", out m_bHasWarmachine);
                SteamUserStats.GetAchievement("OPEN_GAME", out m_bHasOpenGame);
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Our stats data was stored!
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }

            // If yes, request our stats
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            m_bRequestedStats = bSuccess;
        }

        if (!m_bStatsValid)
            return;

        if (!m_bHasOpenGame)
        {
            SteamUserStats.SetAchievement("OPEN_GAME");
            m_bStoreStats = true;
        }

        if (m_flMaxSingleHitDmg >= 250f && !m_bHasWarhead)
            m_bStoreStats = true;

        if (m_flTotalDamageDealt > 100000000 && !m_bHasWarmachine)
            m_bStoreStats = true;

        if (m_intMaxWormsCollected >= 1000000 && !m_bHasMillionaire)
            m_bStoreStats = true;



        //Store stats in the Steam database if necessary
        if (m_bStoreStats)
        {
            SteamUserStats.SetStat("single_hit_dmg", m_flMaxSingleHitDmg);
            SteamUserStats.SetStat("max_worms", m_intMaxWormsCollected);
            SteamUserStats.SetStat("total_worms_collected", m_intTotalWormsCollected);
            SteamUserStats.SetStat("total_dmg_dealt", m_flTotalDamageDealt);

            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_bStoreStats = !bSuccess;
        }
    }

    public void UpdateMaxSingleHitDmg(float dmg)
    {
        if (dmg > m_flMaxSingleHitDmg)
            m_flMaxSingleHitDmg = dmg;
    }

    public void UpdateMaxWormsCollected(int worms)
    {
        if (worms > m_intMaxWormsCollected)
            m_intMaxWormsCollected = worms;
    }
}
