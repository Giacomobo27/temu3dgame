using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        SpeedJumpBoost, // Potion
        CollisionProof,   // Apple
        CameraAngle     // Coin
    }

    public PowerUpType type;

    // Optional: Add rotation for visual flair
    public float rotationSpeed = 100f;

    void Update()
    {
        //transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other) {   
       // Destroy the collectible
     //  Destroy(gameObject);
}
}