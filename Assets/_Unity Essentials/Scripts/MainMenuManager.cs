using UnityEngine;
using UnityEngine.SceneManagement; // REQUIRED for scene loading
// using UnityEngine.InputSystem; // Only needed if checking specific Input System actions

public class MainMenuManager : MonoBehaviour
{
    public string gameplaySceneName = "GameplayScene"; // Assign in Inspector

    // Flag to prevent loading the scene multiple times if clicked/keyed rapidly
    private bool isLoading = false;

    // Update is called once per frame
  

    // This function handles the actual scene loading logic
    public void StartGame()
    {
        // Prevent this from running more than once per attempt
        if (isLoading) return;
        isLoading = true; // Set the flag

        Debug.Log("Start triggered (Any Click/Key). Loading scene: " + gameplaySceneName);

        // Ensure time scale is normal when starting gameplay,
        // as it might have been set to 0 by a previous Game Over state.
        Time.timeScale = 1f;

        // Load the main gameplay scene using the name assigned in the Inspector
        SceneManager.LoadScene(gameplaySceneName);
    }

    // --- Optional Quit Function ---
    public void QuitGame()
    {
        Debug.Log("Quit Game called.");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}