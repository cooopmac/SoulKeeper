using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;
    private PlayerStats playerStats;
    
    void Start()
    {
        // Make sure pause menu is initially hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // Find the player stats
        playerStats = FindFirstObjectByType<PlayerStats>();
    }
    
    void Update()
    {
        // Check for pause button (Escape key by default)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    void PauseGame()
    {
        // Show pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        // Freeze the game
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        // Hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Unfreeze the game
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    public void ReturnToMainMenu()
    {
        // Save player data before returning to menu
        if (playerStats != null)
        {
            playerStats.SavePlayerData();
        }
        
        // Reset time scale before changing scenes
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void QuitGame()
    {
        // Save player data before quitting
        if (playerStats != null)
        {
            playerStats.SavePlayerData();
        }
        
        Debug.Log("Quitting game...");
        
        // Quit the application (only works in built game)
        Application.Quit();
        
        // For testing in the editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}