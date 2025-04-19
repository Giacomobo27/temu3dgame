using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

public class UIManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional persistence
        }
    }
    // --- End Singleton ---

    [Header("Score Calculation")]
    // Changed Multiplier meaning
    [Tooltip("Points awarded per second of playtime.")]
    public float pointsPerSecond = 10.0f; // Example: 10 points per second

    [Header("UI References")]
    public TextMeshProUGUI scoreTextUI;
    [Header("resistance References")]
    public TextMeshProUGUI rTextUI;

    // Target Reference no longer needed for score calculation
    // [Header("Target Reference")]
    // public Transform targetToTrack;

    // --- Score Variables ---
    public int CurrentScore { get; private set; }
    private float scoreAccumulator = 0f; // Accumulates fractional score from time
    // Removed distance tracking variables: lastTrackedZ, isTracking
    private GameManager gameManager; // Cached reference

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null) Debug.LogWarning("UIManager: Cannot find GameManager instance!");

        if (scoreTextUI == null) {
            Debug.LogError("UIManager: Score Text UI not assigned!", this);
        }

        // No target tracking needed in Start anymore

        ResetScore(); // Initialize score
        UpdateScoreVisibility(); // Set initial UI visibility
    }

    // Renamed Reset method
    public void ResetScore()
    {
         CurrentScore = 0;
         scoreAccumulator = 0f; // Reset time accumulator
         UpdateScoreDisplay(); // Show initial score (0)
         Debug.Log($"Score Reset.");
    }


    void Update()
    {
         UpdateScoreVisibility(); // Keep UI visibility updated

         // Check if game is playing
         bool isPlaying = (gameManager != null && gameManager.currentState == GameManager.GameState.Playing);

        if (!isPlaying)
        {
            return; // Don't update score if not playing
        }

        // --- Calculate Score based on Time ---
        // Add points based on time elapsed this frame * points per second
        scoreAccumulator += pointsPerSecond * Time.deltaTime;

        // Check if accumulator has enough for at least one whole point
        if (scoreAccumulator >= 1.0f)
        {
            int pointsToAdd = Mathf.FloorToInt(scoreAccumulator); // Get whole points earned
            AddScore(pointsToAdd); // Add the points to the main score
            scoreAccumulator -= pointsToAdd; // Subtract the whole points added, keep the fraction
        }
    }

    // Handles visibility of the score UI
    void UpdateScoreVisibility()
    {
        if (scoreTextUI == null) return;
        bool shouldBeVisible = (gameManager != null && gameManager.currentState == GameManager.GameState.Playing);
        if (scoreTextUI.gameObject.activeSelf != shouldBeVisible)
        {
            scoreTextUI.gameObject.SetActive(shouldBeVisible);
        }
    }

    // Adds points from any source (time, powerups, collectibles)
    public void AddScore(int points)
    {
         // Check game state again just to be safe before modifying score
         bool canScore = (gameManager != null && gameManager.currentState == GameManager.GameState.Playing);
         if (!canScore || points <= 0) return; // Also ignore adding zero/negative points

         CurrentScore += points;
         UpdateScoreDisplay(); // Update UI whenever score changes
         // Debug.Log($"Score Added: {points}. New Total Score: {CurrentScore}"); // Optional log
    }

    // Updates the text display
    void UpdateScoreDisplay()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.text = "Score: " + CurrentScore;
        }
    }

    public void UpdateResistanceON()
    {
        if (rTextUI != null)
        {
            rTextUI.text = "undying";
            
            Debug.Log("UNDYINGGGG");
        }
    }

    public void UpdateResistanceOFF()
    {
        if (rTextUI != null)
        {
            rTextUI.text = "  ";
            Debug.Log(" NOTTT UNDYINGGGG");
        }
    }

}