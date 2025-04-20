using StarterAssets;
using UnityEngine;

// Ensure this script is on the same GameObject as the CharacterController
[RequireComponent(typeof(CharacterController))]

// Require the movement script to apply power-up effects
[RequireComponent(typeof(StarterAssets.CorridorRunnerMovement))]


public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Effects (Optional)")]
    public GameObject deathEffectPrefab; // Assign a particle effect prefab in Inspector
    public AudioClip deathSound;    
    public AudioClip powerUpCollectSound;       // Assign a sound effect in Inspector
     private CorridorRunnerMovement movementScript;
    private bool isGameOver = false; // Prevent multiple triggers

    void Awake()
    {
        // Get reference to the movement script on the same GameObject
        movementScript = GetComponent<CorridorRunnerMovement>();
        if (movementScript == null)
        {
            Debug.LogError("PlayerCollisionHandler cannot find CorridorRunnerMovement script!", this);
            enabled = false; // Disable if movement script is missing
        }
    }

       // This function is called automatically by Unity when the CharacterController hits another SOLID collider while moving
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 1. Early exit if game is already over
        if (isGameOver)
        {
            return;
        }

        // 2. Check if the collision was with an object tagged "Obstacle"
        if (hit.collider.CompareTag("Obstacle"))
        {
            // 3. Check if the CollisionProof power-up is active
            //    We access the public property IsCollisionProof from the movement script.
            //    Use ?. for null-safety in case movementScript reference was lost.
            bool collisionIsSafe = movementScript?.IsCollisionProof ?? false; // Default to false if script is null

            if (collisionIsSafe)
            {
                // Collision proof is active - IGNORE the collision consequence
                Debug.Log($"Hit Obstacle ({hit.collider.name}), but CollisionProof is active. Ignoring Game Over.");
                // Optional: Play a deflection sound/effect here
                // AudioSource.PlayClipAtPoint(deflectSound, transform.position);
                // Instantiate(deflectEffect, hit.point, Quaternion.LookRotation(hit.normal));
                return; // Important: Exit the function here, do not proceed to Game Over
            }
            else
            {
                // Collision proof is NOT active - Trigger Game Over sequence
                Debug.Log($"Player hit Obstacle ({hit.collider.name})! Triggering Game Over.");

                isGameOver = true; // Set flag first

                // Play optional death effects
                if (deathEffectPrefab != null) {
                    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                }
                if (deathSound != null) {
                    AudioSource.PlayClipAtPoint(deathSound, transform.position);
                }

                // Disable movement script
                if (movementScript != null) {
                    movementScript.enabled = false;
                }

                // Tell the GameManager
                if (GameManager.Instance != null) {
                    GameManager.Instance.PlayerDied();
                } else {
                    Debug.LogError("GameManager instance not found! Cannot trigger Game Over state properly.");
                    Time.timeScale = 0f; // Fallback pause
                }

                // Disable this script after triggering game over
                enabled = false;
            }
        }
        // If we hit something else (not tagged "Obstacle"), we just ignore it here.
        // You could add logic for other tags if needed.
    }

     // --- ADD THIS METHOD: Handles entering TRIGGER colliders (like power-ups) ---
    void OnTriggerEnter(Collider other)
    {
        // Exit if game is over, don't collect powerups after dying
        if (isGameOver) return;

        // Check if the object we entered has the "PowerUp" tag
        if (other.CompareTag("PowerUp"))
        {
            // Try to get the PowerUp script component from the trigger object
            PowerUp powerUp = other.GetComponent<PowerUp>();

            if (powerUp != null)
            {
                Debug.Log("Collected PowerUp: " + powerUp.type);

                // Play collection sound if assigned
                if (powerUpCollectSound != null)
                {
                    AudioSource.PlayClipAtPoint(powerUpCollectSound, transform.position);
                }

                // Activate the specific power-up effect based on its type
                // Calling methods we already added to CorridorRunnerMovement
                switch (powerUp.type)
                {
                    case PowerUp.PowerUpType.SpeedJumpBoost:
                        movementScript?.ActivateSpeedJumpBoost(); // Use ?. for safety
                        break;
                    case PowerUp.PowerUpType.CollisionProof:
                        movementScript?.ActivateCollisionProof();
                        UIManager.Instance?.AddScore(100);
                        break;
                    case PowerUp.PowerUpType.CameraAngle:
                        movementScript?.ActivateCameraAngleChange();
                        UIManager.Instance?.AddScore(300);
                        break;
                }

                // Destroy the power-up object after collecting it
                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogWarning("Collided with PowerUp tagged object, but couldn't find PowerUp script.", other.gameObject);
            }
        }
        // Add other trigger checks here if needed (e.g., finish line, collectibles)
        // else if (other.CompareTag("Collectible")) { ... }
    }
}