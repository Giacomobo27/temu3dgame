using UnityEngine;
using UnityEngine.SceneManagement; // Required for reloading the scene

public class GameManager : MonoBehaviour
{
    public string gameplaySceneName = "EndScene"; // Assign in Inspector
    // --- Singleton Pattern ---
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // If an Instance already exists, destroy this new one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // This is the first instance, make it the singleton
            Instance = this;
            // Optional: Keep the GameManager across scene loads
            // DontDestroyOnLoad(gameObject);
        }
    }
    // --- End Singleton ---


    public enum GameState
    {
        Playing,    // Gameplay is active
        GameOver    // Player has lost
    }

    [Header("Game State")]
    public GameState currentState = GameState.Playing;

    [Header("UI References (Optional)")]

    public GameObject playingUI; // e.g., Score, HUD elements
    public GameObject gameOverUI;

    [Header("Other References")]
    public GameObject playerObject; // Assign the player GameObject


    void Start()
    {
        // Initial setup based on the starting state
        SetState(GameState.Playing); // Start in the Ready state

        if (playerObject == null) {
             // Try to find the player automatically if not assigned (optional)
            playerObject = GameObject.FindGameObjectWithTag("Player"); // Make sure your player has the "Player" tag
            if (playerObject == null) {
                 Debug.LogError("Player Object is not assigned in GameManager and couldn't be found by tag!", this);
            }
            // Add lines here to get references to player scripts if needed
            // playerMovement = playerObject?.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        // Handle input based on the current state
        switch (currentState)
        {
           
            case GameState.Playing:
                // Game logic happens here or in other scripts that check this state
                // For example, player movement script should only work if state is Playing
                // Check for game over conditions (e.g., player health <= 0)
                break;

            case GameState.GameOver:
                // Check for input to restart the game (e.g., 'R' key)
                SceneManager.LoadScene(gameplaySceneName);
               
                break;
        }
    }

    // --- Public Methods to Change State ---

    public void SetState(GameState newState)
    {
        if (currentState == newState) return; // No change

        currentState = newState;
        Debug.Log("Game State changed to: " + currentState);

        playingUI?.SetActive(currentState == GameState.Playing);
        gameOverUI?.SetActive(currentState == GameState.GameOver);

        switch (currentState)
        {

            case GameState.Playing:
                 Time.timeScale = 1f; // Resume game physics and time
                // Enable player movement script
                // playerMovement?.SetMovementEnabled(true);
                break;

            case GameState.GameOver:
                 Time.timeScale = 0f; // Optional: Pause game on game over
                // Disable player movement script
                // playerMovement?.SetMovementEnabled(false);
                // Maybe play a game over sound
                break;
        }
    }

    public void StartGame()
    {
            SetState(GameState.Playing);
    
    }

    // Call this method from your player script when it dies/collides
    public void PlayerDied()
    {
        if (currentState == GameState.Playing)
        {
            Debug.Log("GameManager: PlayerDied called. Setting state to GameOver.");
            SetState(GameState.GameOver);
        }
        else{
        Debug.LogWarning("GameManager: PlayerDied called but game state was not Playing (" + currentState + "). Ignoring.");
        }
    }


}