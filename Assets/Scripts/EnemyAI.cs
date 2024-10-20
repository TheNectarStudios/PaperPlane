using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 10f;  // Speed at which the enemy moves
    public float rotationSpeed = 3f;  // Speed of turning to face the player
    public float fireRange = 50f;  // Firing range of the enemy
    public GameObject bulletPrefab;  // The bullet prefab for firing
    public Transform firePoint;  // The point where bullets are fired from
    public float fireRate = 2.0f;  // Fire rate of the enemy
    private float fireTimer = 0f;
    
    private Transform playerPlane;  // Reference to the player's plane
    public float despawnDistance = 200f;  // Distance at which enemy despawns
    private bool isDead = false;  // Track if enemy has been hit

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();  // Cache Rigidbody component
    }

    private void Update()
    {
        if (playerPlane == null || isDead) return;  // Exit if playerPlane is not assigned or enemy is dead

        // Check if the enemy is too far from the player and despawn
        float distanceFromPlayer = Vector3.Distance(transform.position, playerPlane.position);
        if (distanceFromPlayer > despawnDistance)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards the player's plane
        Vector3 direction = playerPlane.position - transform.position;
        float distance = direction.magnitude;

        // Rotate to face the player's plane
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // If within range, fire at the player
        if (distance <= fireRange)
        {
            HandleFiring();
        }
    }

    private void HandleFiring()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            Fire();
            fireTimer = 0f;
        }
    }

    private void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.velocity = transform.forward * 500f;  // Bullet speed
    }

    // Handle collision with bullet
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Die();
        }
    }

    // Called when the enemy dies
    private void Die()
    {
        isDead = true;

        // Disable enemy controls
        moveSpeed = 0;
        rotationSpeed = 0;

        // Apply falling effect (simulate engine failure)
        rb.useGravity = true;  // Let the plane fall due to gravity
        rb.constraints = RigidbodyConstraints.None;  // Free the rigidbody

        // Destroy enemy after 5 seconds
        Destroy(gameObject, 5.0f);
    }

    // This method allows setting the playerPlane from outside the script
    public void SetPlayer(Transform player)
    {
        playerPlane = player;
    }
}
