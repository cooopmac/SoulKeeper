using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    // Keys for PlayerPrefs
    private const string SOULS_KEY = "PlayerSouls";
    private const string ATT_KEY = "PlayerAttack";
    private const string SPD_KEY = "PlayerSpeed";
    private const string DEX_KEY = "PlayerDexterity";
    private const string WIS_KEY = "PlayerWisdom";
    
    // Save player souls and stats
    public static void SavePlayerData(PlayerStats playerStats)
    {
        if (playerStats == null) return;
        
        // Save souls and stats using PlayerPrefs (simple built-in Unity storage)
        PlayerPrefs.SetInt(SOULS_KEY, playerStats.soulCount);
        PlayerPrefs.SetInt(ATT_KEY, playerStats.attackStat);
        PlayerPrefs.SetInt(SPD_KEY, playerStats.speedStat);
        PlayerPrefs.SetInt(DEX_KEY, playerStats.dexterityStat);
        PlayerPrefs.SetInt(WIS_KEY, playerStats.wisdomStat);
        
        // Save immediately
        PlayerPrefs.Save();
        
        Debug.Log("Player data saved. Souls: " + playerStats.soulCount);
    }
    
    // Load player souls and stats
    public static void LoadPlayerData(PlayerStats playerStats)
    {
        if (playerStats == null) return;
        
        // Load souls and stats from PlayerPrefs
        playerStats.soulCount = PlayerPrefs.GetInt(SOULS_KEY, 0); // Default 0 if not found
        playerStats.attackStat = PlayerPrefs.GetInt(ATT_KEY, 1);  // Default 1 if not found
        playerStats.speedStat = PlayerPrefs.GetInt(SPD_KEY, 1);
        playerStats.dexterityStat = PlayerPrefs.GetInt(DEX_KEY, 1);
        playerStats.wisdomStat = PlayerPrefs.GetInt(WIS_KEY, 1);
        
        Debug.Log("Player data loaded. Souls: " + playerStats.soulCount);
    }
}