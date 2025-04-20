using UnityEngine;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    [Header("Score Calculation")]
    [Tooltip("Points awarded per second of playtime.")]
    public float pointsPerSecond = 10.0f;

    [Header("UI References")]
    public TextMeshProUGUI scoreTextUI;
    [Header("resistance References")]
    public TextMeshProUGUI rTextUI;


    // --- Score Variables ---
    public int CurrentScore { get; private set; }
    private float scoreAccumulator = 0f; // Accumulates fractional score from time
    private GameManager gameManager; // Cached reference

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null) Debug.LogWarning("UIManager: Cannot find GameManager instance!");

        if (scoreTextUI == null) {
            Debug.LogError("UIManager: Score Text UI not assigned!", this);
        }

        ResetScore(); // Initialize score
        UpdateScoreVisibility(); 
    }

    public void ResetScore()
    {
         CurrentScore = 0;
         scoreAccumulator = 0f; // Reset time accumulator
         UpdateScoreDisplay(); // Show initial score (0)
         Debug.Log($"Score Reset.");
    }


    void Update()
    {
         UpdateScoreVisibility(); 

         // Check if game is playing
         bool isPlaying = (gameManager != null && gameManager.currentState == GameManager.GameState.Playing);

        if (!isPlaying)
        {
            return; // Don't update score if not playing
        }

        // Calculate Score based on Time 
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

    // Adds points from any source 
    public void AddScore(int points)
    {
         // Check game state again just to be safe before modifying score
         bool canScore = (gameManager != null && gameManager.currentState == GameManager.GameState.Playing);
         if (!canScore || points <= 0) return; // ignore adding zero/negative points

         CurrentScore += points;
         UpdateScoreDisplay(); 
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