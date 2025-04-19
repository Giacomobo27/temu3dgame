using StarterAssets;
using UnityEngine;

// Ensure this script is on the same GameObject as the CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Effects (Optional)")]
    public GameObject deathEffectPrefab; // Assign a particle effect prefab in Inspector
    public AudioClip deathSound;          // Assign a sound effect in Inspector

    private bool isGameOver = false; // Prevent multiple triggers

    // This function is called automatically by Unity when the CharacterController hits another collider while moving
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Exit if game over already triggered
        if (isGameOver) return;

        // Check if the object we hit has the "Obstacle" tag
        if (hit.collider.CompareTag("Obstacle"))
        {
            // --- Trigger Game Over ---

            Debug.Log("Player hit Obstacle: " + hit.collider.name);
            isGameOver = true; // Set flag to prevent repeat triggers

            // Optional: Play immediate effects
            if (deathEffectPrefab != null) {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            if (deathSound != null) {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            // Disable own movement script immediately
            CorridorRunnerMovement movementScript = GetComponent<CorridorRunnerMovement>();
            if (movementScript != null) {
                movementScript.enabled = false;
            }
            
            // Tell the GameManager to handle the game over state
            if (GameManager.Instance != null) {
                GameManager.Instance.PlayerDied(); // Call the specific method
            } else {
                Debug.LogError("GameManager instance not found! Cannot trigger Game Over state.");
                // Fallback: Just stop time if GameManager is missing (not ideal)
                Time.timeScale = 0f;
            }

            // Disable this collision script too, just in case
            enabled = false;
        }
    }
}