using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthRegenRate = 2f;      
    public float healthRegenDelay = 5f; 
    private float lastDamageTime = -100f;
    
    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;
    public float baseRegenRate = 5f; // Base mana regenerated per second
    [HideInInspector] public float manaRegenRate; // Current regen rate after wisdom bonuses
    
    [Header("RPG Stats")]
    public int attackStat = 1;
    public int speedStat = 1;
    public int dexterityStat = 1;
    public int wisdomStat = 1; 
    public int soulCount = 0;
    
    [Header("VFX")]
    public GameObject damageVFX;
    
    [Header("Component References")]
    private PlayerController playerController;
    private PlayerAiming playerAttack;
    
    // Events for UI updates
    public System.Action<float> OnHealthChanged;
    public System.Action<float> OnManaChanged;
    public System.Action<int> OnSoulCountChanged;
    public System.Action<int, int, int, int> OnStatsChanged;
    
    // PlayerPrefs keys for saving data
    private const string SOULS_KEY = "PlayerSouls";
    private const string ATT_KEY = "PlayerAttack";
    private const string SPD_KEY = "PlayerSpeed";
    private const string DEX_KEY = "PlayerDexterity";
    private const string WIS_KEY = "PlayerWisdom";

    [SerializeField] private GameObject diedPanel;
    
    void Start()
    {
        // Find components
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAiming>();
        
        // Load saved stats and souls
        LoadPlayerData();
        
        // Initialize current values
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // Apply stat effects
        ApplyStatEffects();
    }
    
    void Update()
    {
        // Regenerate mana over time
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
            
            // Notify listeners that mana changed
            if (OnManaChanged != null)
                OnManaChanged(currentMana);
        }

        // Regenerate health over time (if it's been long enough since taking damage)
        if (currentHealth < maxHealth && Time.time > lastDamageTime + healthRegenDelay)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            
            // Notify listeners that health changed
            if (OnHealthChanged != null)
                OnHealthChanged(currentHealth);
        }
    }

    public void CollectSoul()
    {
        soulCount++;
        Debug.Log("Soul collected! Total souls: " + soulCount);
        
        // Notify listeners that soul count changed
        if (OnSoulCountChanged != null)
            OnSoulCountChanged(soulCount);
        
        // Auto-save when souls are collected
        SavePlayerData();
    }
    
    // Method to handle taking damage
    public void TakeDamage(float damage)
    {
        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        // Show damage VFX if available
        if (damageVFX != null)
        {
            Instantiate(damageVFX, transform.position, Quaternion.identity);
        }
        
        // Notify listeners that health changed
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Handle player death
    void Die()
{
    Debug.Log("Player died");
    
    // Pause the game
    Time.timeScale = 0f;
    
    // Show the died panel
    if (diedPanel != null)
    {
        diedPanel.SetActive(true);
    }
    else
    {
        Debug.LogWarning("DiedPanel is not assigned!");
    }
    
    // Disable player control scripts - safer approach
    MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
    foreach (MonoBehaviour script in scripts)
    {
        // Don't disable this script
        if (script != this)
        {
            script.enabled = false;
        }
    }
}

public void ReturnToMainMenu()
{
    // Resume normal time scale before loading a new scene
    Time.timeScale = 1f;
    
    // Load the main menu scene
    // Make sure "MainMenu" is the exact name of your main menu scene
    // Or replace with the proper scene name/index
    SceneManager.LoadScene("MainMenu");
}
    
    // Apply the effects of stats to gameplay
    void ApplyStatEffects()
    {
        // Apply WIS stat effect - Mana regen
        manaRegenRate = baseRegenRate * (1f + (wisdomStat - 1) * 0.1f); // +10% per WIS level
        
        // Apply SPD stat effect - Movement speed
        if (playerController != null)
        {
            playerController.UpdateMoveSpeed();
        }
        
        // Apply ATT and DEX stat effects - Damage and fire rate
        if (playerAttack != null)
        {
            playerAttack.UpdateAttackStats();
        }
        
        Debug.Log("Applied stat effects - Mana Regen: " + manaRegenRate);
    }
    
    // Save player souls and stats
    public void SavePlayerData()
    {
        // Save souls and stats using PlayerPrefs
        PlayerPrefs.SetInt(SOULS_KEY, soulCount);
        PlayerPrefs.SetInt(ATT_KEY, attackStat);
        PlayerPrefs.SetInt(SPD_KEY, speedStat);
        PlayerPrefs.SetInt(DEX_KEY, dexterityStat);
        PlayerPrefs.SetInt(WIS_KEY, wisdomStat);
        
        // Save immediately
        PlayerPrefs.Save();
        
        Debug.Log("Player data saved. Souls: " + soulCount);
    }
    
    // Load player souls and stats
    public void LoadPlayerData()
    {
        // Load souls and stats from PlayerPrefs
        soulCount = PlayerPrefs.GetInt(SOULS_KEY, 0); // Default 0 if not found
        attackStat = PlayerPrefs.GetInt(ATT_KEY, 1);  // Default 1 if not found
        speedStat = PlayerPrefs.GetInt(SPD_KEY, 1);
        dexterityStat = PlayerPrefs.GetInt(DEX_KEY, 1);
        wisdomStat = PlayerPrefs.GetInt(WIS_KEY, 1);
        
        // Notify listeners that stats changed
        if (OnStatsChanged != null)
            OnStatsChanged(attackStat, speedStat, dexterityStat, wisdomStat);
            
        Debug.Log("Player data loaded. Souls: " + soulCount);
    }
    
    // Update a specific stat when upgraded
    public void UpgradeStat(string statType)
    {
        switch (statType)
        {
            case "att":
                attackStat++;
                break;
            case "spd":
                speedStat++;
                break;
            case "dex":
                dexterityStat++;
                break;
            case "wis":
                wisdomStat++;
                break;
        }
        
        // Apply new stat effects
        ApplyStatEffects();
        
        // Notify listeners that stats changed
        if (OnStatsChanged != null)
            OnStatsChanged(attackStat, speedStat, dexterityStat, wisdomStat);
        
        // Save after upgrading
        SavePlayerData();
    }
    
    // Auto-save on quit
    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}