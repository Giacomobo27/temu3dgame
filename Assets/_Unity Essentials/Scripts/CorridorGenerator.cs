using UnityEngine;
using System.Collections.Generic; // Required for using Lists or Queues

public class CorridorGenerator : MonoBehaviour
{
    //public variables

    [Header("Level Progression Corridors")]
[Tooltip("Corridor prefabs for each level (Level 1, Level 2, Level 3)")]
public GameObject[] corridorPrefabsByLevel = new GameObject[3];

    [Header("PowerUp Settings")] 
public GameObject[] powerUpPrefabs;
[Range(0f, 1f)]
public float powerUpSpawnProbability = 0.1f; // Lower chance than obstacles

    [Header("Corridor Settings")]
    public GameObject corridorSectionPrefab;
    public float sectionLength = 12f; // *** RE-VERIFY THIS VALUE IS ACCURATE ***
    public int initialSections = 5;
    public int sectionsToMaintainAhead = 3; // Should be less than initialSections

     [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs; // Array to hold your obstacle prefabs

    [Tooltip("The probability (0.0 to 1.0) that an obstacle will spawn in a new section.")]
    [Range(0f, 1f)]
    public float obstacleSpawnProbability = 0.4f; 


    [Header("References")]
    public Transform playerTransform;

    // --- Private Variables ---
    private Queue<GameObject> activeSections = new Queue<GameObject>();
    private Vector3 nextSpawnPosition = Vector3.zero;
    private float currentTriggerSpawnZ = float.MinValue; // Store the calculated trigger Z
    private GameManager gameManager; 
    void Start()
    {
        // --- Error Checking (Keep this) ---
        if (corridorSectionPrefab == null) {
            Debug.LogError("Corridor Section Prefab is not assigned!", this);
            enabled = false; return;
        }
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) { playerTransform = player.transform; }
            else { Debug.LogError("Player Transform is not assigned and couldn't be found by tag!", this); enabled = false; return; }
        }
         if (sectionLength <= 0) {
             Debug.LogError("Section Length must be a positive value!", this); enabled = false; return;
         }
         if (sectionsToMaintainAhead >= initialSections) {
             Debug.LogWarning("sectionsToMaintainAhead should ideally be less than initialSections to prevent immediate destruction.", this);
         }

          if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) {
            Debug.LogWarning("CorridorGenerator: No obstacle prefabs assigned. No obstacles will spawn.", this);
        }
        // --- End Error Checking ---

        gameManager = GameManager.Instance;
     if (gameManager == null) Debug.LogWarning("CorridorGenerator: GameManager not found!", this);

        // Initialize starting position
        nextSpawnPosition = transform.position; // Start at the generator's origin
        // Spawn initial sections
        for (int i = 0; i < initialSections; i++)
        {
            SpawnSection(i == 0); // This will correctly update nextSpawnPosition // i==0  mean no obstacle in the first one
        }
        Debug.Log($"Initial spawning complete. Next spawn position Z: {nextSpawnPosition.z}");
    }

        void Update()
    {
        if (playerTransform == null)
        {
             // If it became null after Start(), log an error once.
             if(Time.frameCount % 100 == 1) // Log periodically, not every frame
                 Debug.LogError("CorridorGenerator: Player Transform reference lost!");
             return;
        }

        // --- Calculate the trigger point dynamically each frame ---
        currentTriggerSpawnZ = nextSpawnPosition.z - (sectionsToMaintainAhead * sectionLength);
        // Check if the player has moved far enough forward
        if (playerTransform.position.z > currentTriggerSpawnZ)
        {
            Debug.Log($"--- [CorridorGenerator] Triggered Spawn! ---"); // Make sure this logs!
            SpawnSection();
            DestroyOldestSection();
        }
    }
    void SpawnSection(bool isFirstSection = false)
    {

         // --- Determine Prefab based on Level ---
    GameObject prefabToSpawn = corridorSectionPrefab; // Default fallback

    if (gameManager != null && corridorPrefabsByLevel != null)
    {
        // Get the prefab for the current level
        // Make sure array elements are not null in Inspector!
        GameObject levelPrefab = corridorPrefabsByLevel[(int)gameManager.CurrentLevel];
        if (levelPrefab != null) {
            prefabToSpawn = levelPrefab;
        } else {
            // Warn if specific level prefab is missing, use default
            Debug.LogWarning($"Corridor prefab for Level {(int)gameManager.CurrentLevel + 1} is not assigned! Using default.", this);
        }
    }
     else if(corridorPrefabsByLevel == null || corridorPrefabsByLevel.Length < 3)
    {
        if(Time.frameCount % 120 == 0) // Log periodically
            Debug.LogWarning("corridorPrefabsByLevel array not configured correctly in Inspector! Using default.", this);
    }


    if (prefabToSpawn == null) { // Final check if default is also null
        Debug.LogError("No valid corridor prefab found to spawn!", this);
        return;
    }

    // Instantiate the chosen section prefab
    GameObject newSection = Instantiate(prefabToSpawn, nextSpawnPosition, Quaternion.identity, transform);
    activeSections.Enqueue(newSection);






        if(!isFirstSection){
        // --- Try to Spawn an Obstacle in the new section ---
        float rnumber= Random.value;
        if (rnumber < obstacleSpawnProbability)
        {
             TrySpawnObstacle(newSection);
        }
        else if (rnumber < obstacleSpawnProbability + powerUpSpawnProbability) {
            TrySpawnPowerUp(newSection); 
        }
        }
      



        // --- CRITICAL: Update the position for the *next* section ---
        Vector3 oldSpawnPos = nextSpawnPosition; // For logging
        nextSpawnPosition.z += sectionLength;
        Debug.Log($"Spawned section at Z: {oldSpawnPos.z:F2}. Next spawn will be at Z: {nextSpawnPosition.z:F2}");
    }

    
     void TrySpawnObstacle(GameObject sectionInstance)
     {
        // Check if we actually have obstacle prefabs assigned
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) {
             return; // No obstacles to spawn
        }

        // Find all potential spawn points within the newly instantiated section
        // Using the ObstacleSpawnPoint component marker script
        ObstacleSpawnPoint[] spawnPoints = sectionInstance.GetComponentsInChildren<ObstacleSpawnPoint>();

        // Check if any spawn points were found in the prefab
        if (spawnPoints == null || spawnPoints.Length == 0) {
            // Debug.LogWarning("No ObstacleSpawnPoint components found in the corridor section prefab.", sectionInstance);
            return; // No places to spawn obstacles in this section prefab
        }

        // --- Spawn Logic ---
        // 1. Select a random spawn point from the found points
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnTransform = spawnPoints[spawnIndex].transform;

        // 2. Select a random obstacle prefab from the array
        int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstacleToSpawn = obstaclePrefabs[prefabIndex];

        // 3. Instantiate the chosen obstacle at the chosen point
        if (obstacleToSpawn != null) {
            // Instantiate the obstacle at the spawn point's position and rotation,
            // and make it a child of the section so it gets destroyed automatically.
            Instantiate(obstacleToSpawn, spawnTransform.position, spawnTransform.rotation, sectionInstance.transform);
             // Debug.Log($"Spawned obstacle {obstacleToSpawn.name} in section {sectionInstance.name}");
        } else {
             Debug.LogWarning($"Obstacle prefab at index {prefabIndex} is null.");
        }
     }

      void TrySpawnPowerUp(GameObject sectionInstance)
 {
    // Check if we have power-up prefabs
    if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

    // Find spawn points (reuse obstacle points or make specific ones)
    ObstacleSpawnPoint[] spawnPoints = sectionInstance.GetComponentsInChildren<ObstacleSpawnPoint>();
    if (spawnPoints == null || spawnPoints.Length == 0) return;

    // --- Spawn Logic ---
    // 1. Select a random spawn point
    // CONSIDER: Maybe avoid spawning on the same point as an obstacle? Needs extra logic.
    int spawnIndex = Random.Range(0, spawnPoints.Length);
    Transform spawnTransform = spawnPoints[spawnIndex].transform;

    // 2. Select a random power-up prefab
    int prefabIndex = Random.Range(0, powerUpPrefabs.Length);
    GameObject powerUpToSpawn = powerUpPrefabs[prefabIndex];

    // 3. Instantiate
    if (powerUpToSpawn != null) {
        Instantiate(powerUpToSpawn, spawnTransform.position, spawnTransform.rotation, sectionInstance.transform);
         // Debug.Log($"Spawned power-up {powerUpToSpawn.name} in section {sectionInstance.name}");
    } else {
         Debug.LogWarning($"PowerUp prefab at index {prefabIndex} is null.");
    }
 }



    void DestroyOldestSection()
    {
        int safeSectionCount = initialSections + 2; // Example buffer

        if (activeSections.Count > safeSectionCount)
        {
            GameObject oldestSection = activeSections.Dequeue();
            if (oldestSection != null) { // Check if it wasn't already destroyed somehow
                 Debug.Log($"Destroying oldest section at Z: {oldestSection.transform.position.z:F2}. Remaining: {activeSections.Count}");
                 Destroy(oldestSection);
            }
        }
    }

    // Optional: Visualize the trigger and next spawn points
    void OnDrawGizmos()
    {
        if(!Application.isPlaying) return; // Only draw gizmos while playing

        // Next Spawn Position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(nextSpawnPosition, 1f);
        Gizmos.DrawLine(nextSpawnPosition + Vector3.left * 2, nextSpawnPosition + Vector3.right * 2); // Mark spawn Z

        // Trigger Position (Calculated using the runtime value)
        if (playerTransform != null) // Ensure player exists
        {
             // Recalculate here just for visualization if needed, or use the stored value
            float triggerZ = nextSpawnPosition.z - (sectionsToMaintainAhead * sectionLength);
            Vector3 triggerPos = new Vector3(playerTransform.position.x, playerTransform.position.y, triggerZ); // Visualize at player's X/Y
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(triggerPos, 0.8f);
            Gizmos.DrawLine(triggerPos + Vector3.left, triggerPos + Vector3.right); // Mark trigger Z
        }
    }
}