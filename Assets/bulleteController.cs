using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject hitEffectPrefab;  // Particle effect to play on collision
    private bool hasCollided = false;   // To ensure the effect only plays on first collision

    void Start()
    {
        // Destroy bullet after 3 seconds
        Destroy(gameObject, 3.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)  // Ensure effect plays only once
        {
            hasCollided = true;

            // Play hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the bullet on collision
        }
    }
}
