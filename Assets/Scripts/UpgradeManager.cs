using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject upgradePanel;
    public Button openUpgradeButton;
    public Button closeUpgradeButton;
    
    [Header("Stat Buttons")]
    public Button upgradeAttButton;
    public Button upgradeSpeedButton;
    public Button upgradeDexButton;
    public Button upgradeWisButton;
    
    [Header("UI Text")]
    public TextMeshProUGUI soulCountText;
    public TextMeshProUGUI attStatText;
    public TextMeshProUGUI spdStatText;
    public TextMeshProUGUI dexStatText;
    public TextMeshProUGUI wisStatText;
    public TextMeshProUGUI costText;
    
    [Header("Upgrade Costs")]
    public int baseCost = 5;
    public float costMultiplier = 1.5f;
    
    [Header("Player Reference")]
    public GameObject player; // Just need to assign the player GameObject
    
    // Component references - these will be found automatically
    private PlayerStats playerStats;
    private PlayerController playerController;
    private PlayerAiming playerAiming;
    
    private bool isPanelOpen = false;
    
    void Start()
    {
        // Find components from the player GameObject if assigned
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            playerController = player.GetComponent<PlayerController>();
            playerAiming = player.GetComponent<PlayerAiming>();
        }
        
        // Fallback to finding components in the scene
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
            
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
            
        if (playerAiming == null)
            playerAiming = FindFirstObjectByType<PlayerAiming>();
        
        // Close upgrade panel on start
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
        
        // Set up button listeners
        if (openUpgradeButton != null)
            openUpgradeButton.onClick.AddListener(OpenUpgradePanel);
            
        if (closeUpgradeButton != null)
            closeUpgradeButton.onClick.AddListener(CloseUpgradePanel);
            
        if (upgradeAttButton != null)
            upgradeAttButton.onClick.AddListener(() => UpgradeStat("att"));
            
        if (upgradeSpeedButton != null)
            upgradeSpeedButton.onClick.AddListener(() => UpgradeStat("spd"));
            
        if (upgradeDexButton != null)
            upgradeDexButton.onClick.AddListener(() => UpgradeStat("dex"));
            
        if (upgradeWisButton != null)
            upgradeWisButton.onClick.AddListener(() => UpgradeStat("wis"));
        
        // Update UI initially
        UpdateUpgradeUI();
    }
    
    void Update()
    {
        // Update UI when panel is open
        if (isPanelOpen)
        {
            UpdateUpgradeUI();
        }
        
        // Optional: Allow toggle of upgrade panel with a key
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (isPanelOpen)
                CloseUpgradePanel();
            else
                OpenUpgradePanel();
        }
    }
    
    void OpenUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            isPanelOpen = true;
            
            // Optional: Pause game while upgrading
            Time.timeScale = 0f;
        }
        
        UpdateUpgradeUI();
    }
    
    void CloseUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            isPanelOpen = false;
            
            // Resume game
            Time.timeScale = 1f;
        }
    }
    
    void UpdateUpgradeUI()
    {
        if (playerStats == null) return;
        
        // Update soul count text
        if (soulCountText != null)
            soulCountText.text = "Souls: " + playerStats.soulCount;
        
        // Update stat texts
        if (attStatText != null)
            attStatText.text = "Attack: " + playerStats.attackStat;
            
        if (spdStatText != null)
            spdStatText.text = "Speed: " + playerStats.speedStat;
            
        if (dexStatText != null)
            dexStatText.text = "Dexterity: " + playerStats.dexterityStat;
            
        if (wisStatText != null)
            wisStatText.text = "Wisdom: " + playerStats.wisdomStat;
        
        // Update cost text
        if (costText != null)
            costText.text = "Upgrade Cost: " + CalculateCost() + " souls";
        
        // Enable/disable buttons based on whether player can afford upgrades
        bool canAfford = (playerStats.soulCount >= CalculateCost());
        
        if (upgradeAttButton != null)
            upgradeAttButton.interactable = canAfford;
            
        if (upgradeSpeedButton != null)
            upgradeSpeedButton.interactable = canAfford;
            
        if (upgradeDexButton != null)
            upgradeDexButton.interactable = canAfford;
            
        if (upgradeWisButton != null)
            upgradeWisButton.interactable = canAfford;
    }
    
    int CalculateCost()
    {
        // Get average of all stats to determine level
        int averageLevel = (playerStats.attackStat + playerStats.speedStat + 
                          playerStats.dexterityStat + playerStats.wisdomStat) / 4;
        
        // Calculate cost based on level (more expensive as you level up)
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, averageLevel - 1));
    }
    
    void UpgradeStat(string statType)
    {
        // Check if player has enough souls
        int cost = CalculateCost();
        if (playerStats.soulCount < cost)
        {
            Debug.Log("Not enough souls to upgrade!");
            return;
        }
        
        // Deduct souls
        playerStats.soulCount -= cost;
        
        // Upgrade the specified stat
        switch (statType)
        {
            case "att":
                playerStats.attackStat++;
                ApplyAttackUpgrade();
                break;
                
            case "spd":
                playerStats.speedStat++;
                ApplySpeedUpgrade();
                break;
                
            case "dex":
                playerStats.dexterityStat++;
                ApplyDexterityUpgrade();
                break;
                
            case "wis":
                playerStats.wisdomStat++;
                ApplyWisdomUpgrade();
                break;
        }
        
        // Save after upgrading
        playerStats.SavePlayerData();
        
        // Update UI
        UpdateUpgradeUI();
    }
    
    // Methods to apply the stat upgrades to actual gameplay mechanics
    void ApplyAttackUpgrade()
    {
        // If you have a player attack script, modify damage based on attack stat
        if (playerAiming != null)
        {
            // Call the UpdateAttackStats method which will apply both ATT and DEX effects
            playerAiming.UpdateAttackStats();
            Debug.Log("Attack upgraded! New ATT: " + playerStats.attackStat);
        }
    }
    
    void ApplySpeedUpgrade()
    {
        // If you have a player controller script, modify move speed based on speed stat
        if (playerController != null)
        {
            // Updates player movement speed based on SPD stat
            playerController.UpdateMoveSpeed();
            Debug.Log("Speed upgraded! New SPD: " + playerStats.speedStat);
        }
    }
    
    void ApplyDexterityUpgrade()
    {
        // If you have a player attack script, modify fire rate based on dexterity stat
        if (playerAiming != null)
        {
            // Call the UpdateAttackStats method which will apply both ATT and DEX effects
            playerAiming.UpdateAttackStats();
            Debug.Log("Dexterity upgraded! New DEX: " + playerStats.dexterityStat);
        }
    }
    
    void ApplyWisdomUpgrade()
    {
        // Modify mana regen rate in the player stats
        if (playerStats != null)
        {
            // Calculate the mana regen rate based on wisdom stat
            float manaRegenMultiplier = 1f + (playerStats.wisdomStat - 1) * 0.1f;
            playerStats.manaRegenRate = 5f * manaRegenMultiplier;
            Debug.Log("Wisdom upgraded! New WIS: " + playerStats.wisdomStat + ", Mana Regen: " + playerStats.manaRegenRate);
        }
    }
}