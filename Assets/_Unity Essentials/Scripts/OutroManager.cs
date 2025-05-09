using UnityEngine;
using UnityEngine.SceneManagement; // REQUIRED for scene loading

public class OutroManager : MonoBehaviour
{
    
    public string gameplaySceneName = "IntroScene"; // Assign in Inspector

    private bool isLoading = false;
    
    // This function handles the actual scene loading logic
    public void RestartGame()
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

}
