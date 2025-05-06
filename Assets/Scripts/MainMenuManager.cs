using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Name of your gameplay scene - change this to match your scene name
    [SerializeField] private string gameplaySceneName = "Gameplay";
    
    // Called when Play button is clicked
    public void PlayGame()
    {
        // Load the gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    // Called when Exit button is clicked
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        
        // This will only work in a built game, not in the editor
        Application.Quit();
        
        // This is useful for testing in the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}