using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleRPGUI : MonoBehaviour
{
    [Header("Panel")]
    public Image panelBackground;
    
    [Header("Wave Display")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI soulsText;
    
    [Header("HP/MP Bars")]
    public Image hpBar;
    public Image mpBar;
    
    [Header("Stats Display")]
    public TextMeshProUGUI attText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI dexText;
    public TextMeshProUGUI wisText;
    
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button pauseButton;
    
    // Reference to player stats
    private PlayerStats playerStats;
    private SimpleEnemySpawner currentWave;
    private PauseManager pauseManager;
    
    void Start()
    {
        // Find player stats component
        playerStats = FindAnyObjectByType<PlayerStats>();
        
        if (playerStats == null)
        {
            Debug.LogError("No PlayerStats component found in the scene!");
        }
        else
        {
            Debug.Log("PlayerStats found successfully!");
        }
        
        // Find wave spawner component
        currentWave = FindAnyObjectByType<SimpleEnemySpawner>();
        
        // Get or Add PauseManager
        pauseManager = GetComponent<PauseManager>();
        if (pauseManager == null)
        {
            pauseManager = gameObject.AddComponent<PauseManager>();
            pauseManager.pauseMenuPanel = pauseMenuPanel;
        }
        
        // Set up pause button if available
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(pauseManager.TogglePause);
        }
        
        // Check UI references
        CheckUIReferences();
        
        // Initial UI update
        UpdateUI();
    }
    
    void CheckUIReferences()
    {
        if (hpBar == null) Debug.LogWarning("HP Bar not assigned!");
        if (mpBar == null) Debug.LogWarning("MP Bar not assigned!");
        if (attText == null) Debug.LogWarning("ATT Text not assigned!");
        if (spdText == null) Debug.LogWarning("SPD Text not assigned!");
        if (dexText == null) Debug.LogWarning("DEX Text not assigned!");
        if (wisText == null) Debug.LogWarning("WIS Text not assigned!");
        if (pauseMenuPanel == null) Debug.LogWarning("Pause Menu Panel not assigned!");
    }
    
    void Update()
    {
        // Only update the UI when game is not paused
        if (Time.timeScale > 0)
        {
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        if (playerStats == null) return;
        
        // Update wave text
        if (waveText != null && currentWave != null)
        {
            waveText.text = "Wave " + currentWave.currentWave;
        }
        else if (waveText != null)
        {
            waveText.text = "Wave 1"; // Default if no spawner found
        }

        if (soulsText != null)
        {
            soulsText.text = "Souls: " + playerStats.soulCount;
        }
        
        // Update HP and MP bars
        if (hpBar != null)
        {
            hpBar.fillAmount = playerStats.currentHealth / playerStats.maxHealth;
        }
        
        if (mpBar != null)
        {
            mpBar.fillAmount = playerStats.currentMana / playerStats.maxMana;
        }
        
        // Update individual stat texts
        if (attText != null)
        {
            attText.text = "ATT: " + playerStats.attackStat;
        }
        
        if (spdText != null)
        {
            spdText.text = "SPD: " + playerStats.speedStat;
        }
        
        if (dexText != null)
        {
            dexText.text = "DEX: " + playerStats.dexterityStat;
        }
        
        if (wisText != null)
        {
            wisText.text = "WIS: " + playerStats.wisdomStat;
        }
    }
}