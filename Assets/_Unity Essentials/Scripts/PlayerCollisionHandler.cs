using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

[RequireComponent(typeof(StarterAssets.CorridorRunnerMovement))]


public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Effects (Optional)")]
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
        if (isGameOver)
        {
            return;
        }

        // Check if the collision was with an object tagged "Obstacle"
        if (hit.collider.CompareTag("Obstacle"))
        {
            //  Check if the CollisionProof power-up is active
            //    We access the public property IsCollisionProof from the movement script.
            //    Use ?. for null-safety in case movementScript reference was lost.
            bool collisionIsSafe = movementScript?.IsCollisionProof ?? false; // Default to false if script is null

            if (collisionIsSafe)
            {
                // Collision proof is active - IGNORE the collision consequence
                Debug.Log($"Hit Obstacle ({hit.collider.name}), but CollisionProof is active. Ignoring Game Over.");
               
                return; // Important: Exit the function here, do not proceed to Game Over
            }
            else
            {
                // Collision proof is NOT active - Trigger Game Over sequence
                Debug.Log($"Player hit Obstacle ({hit.collider.name})! Triggering Game Over.");

                isGameOver = true; // Set flag first

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
    }

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

                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogWarning("Collided with PowerUp tagged object, but couldn't find PowerUp script.", other.gameObject);
            }
        }
    }
}