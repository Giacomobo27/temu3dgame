using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public string gameplaySceneName = "EndScene"; 
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
            // This is the first instance
            Instance = this;
           
        }
    }


    public enum GameState
    {
        Playing,    
        GameOver    
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

    [Header("Game State")]
    public GameState currentState = GameState.Playing;

    [Header("UI References (Optional)")]


    [Header("Other References")]
    public GameObject playerObject; 


    void Start()
    {
         UpdateLevel(true);

        if (playerObject == null) {
            playerObject = GameObject.FindGameObjectWithTag("Player"); // Make sure your player has the "Player" tag
            if (playerObject == null) {
                 Debug.LogError("Player Object is not assigned in GameManager and couldn't be found by tag!", this);
            }
        }
    }

    void Update()
    {
        UpdateLevel();
        // Handle input based on the current state
        switch (currentState)
        {
           
            case GameState.Playing:
                // Game logic in CorridorRunnerMovement
                break;

            case GameState.GameOver:
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
               
                break;

            case GameState.GameOver:
                 Time.timeScale = 0f; 
                break;
        }
    }

    public void StartGame()
    {
            SetState(GameState.Playing);
    
    }

    
    private void UpdateLevel(bool forceUpdate = false)
    {
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
        }
    }


    // Called this method from PlayerCollisionHandler when it dies/collides
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