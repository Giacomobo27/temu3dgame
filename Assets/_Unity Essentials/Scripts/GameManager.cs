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

      // 
    public enum GameLevel { Level1, Level2, Level3 }
    [Header("Level Progression")]
    [Tooltip("Read-only view of the current level")]
    [SerializeField] // Show in inspector but not editable directly
    private GameLevel currentLevel = GameLevel.Level1;
    public GameLevel CurrentLevel => currentLevel; // Public read-only property

    // Score thresholds for level changes
    public int level2Threshold = 1000;
    public int level3Threshold = 2000;
    // --- END LEVEL TRACKING ---

    [Header("Game State")]
    public GameState currentState = GameState.Playing;

    [Header("UI References (Optional)")]


    [Header("Other References")]
    public GameObject playerObject; // Assign the player GameObject


    void Start()
    {
        // Initial setup based on the starting state
        //SetState(GameState.Playing); // Start in the Ready state
         UpdateLevel(true);

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
        UpdateLevel();
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

    
    // --- ADD UpdateLevel Method ---
    private void UpdateLevel(bool forceUpdate = false)
    {
        // Check if UIManager exists and we can get the score
        if (UIManager.Instance == null) return;

        int score = UIManager.Instance.CurrentScore;
        GameLevel calculatedLevel;

        // Determine level based on score thresholds
        if (score >= level3Threshold)
        {
            calculatedLevel = GameLevel.Level3;
        }
        else if (score >= level2Threshold)
        {
            calculatedLevel = GameLevel.Level2;
        }
        else
        {
            calculatedLevel = GameLevel.Level1;
        }

        // Update level only if it changed (or if forced)
        if (calculatedLevel != currentLevel || forceUpdate)
        {
            currentLevel = calculatedLevel;
            Debug.Log($"--- Level Changed to: {currentLevel} (Score: {score}) ---");
            // Optional: Trigger an event here if other scripts need to react instantly
            // OnLevelChanged?.Invoke(currentLevel);
        }
    }
    // --- END UpdateLevel Method ---


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