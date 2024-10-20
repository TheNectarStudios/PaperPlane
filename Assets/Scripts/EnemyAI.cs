using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 20f;  // Speed of the enemy plane
    public float despawnDistance; 
    public float rotationSpeed = 2f;  // Speed of turning (banking)
    public float fireRange = 80f;  // Firing range
    public GameObject bulletPrefab;  // Bullet prefab for firing
    public Transform firePoint;  // The point where bullets are fired from
    public float fireRate = 1.5f;  // Fire rate
    public float bankAngle = 45f;  // Maximum angle for banking
    public float dodgeRange = 30f;  // Range to dodge objects
    public LayerMask obstacleMask;  // Layer for detecting obstacles
    public LayerMask enemyMask;  // Layer for detecting other enemies
    public float obstacleAvoidanceStrength = 5f;  // Force applied to avoid obstacles
    public float enemyAvoidanceRange = 20f;  // Minimum distance to maintain between enemies
    public float abovePlayerHeight = 50f;  // Height to ascend above player before spamming fire
    public ParticleSystem explosionEffect;

    private float fireTimer = 0f;
    private Transform playerPlane;
    private Rigidbody rb;
    private bool isDodging = false;
    private bool isSpammingFire = false;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPlane = player.transform;
        }
    }

    private void Update()
    {
        if (isDead || playerPlane == null) return;

        // Enemy fighter behaviors
        AvoidObstacles();
        AvoidOtherEnemies();
        ManeuverAndFire();
    }

    private void AvoidObstacles()
    {
        // Use raycast to detect obstacles in front of the enemy
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, dodgeRange, obstacleMask))
        {
            Vector3 avoidanceDirection = Vector3.Reflect(transform.forward, hit.normal);
            rb.AddForce(avoidanceDirection * obstacleAvoidanceStrength, ForceMode.VelocityChange);
            isDodging = true;
        }
        else
        {
            isDodging = false;
        }
    }

    private void AvoidOtherEnemies()
    {
        // Use sphere overlap to check for nearby enemies and steer away from them
        Collider[] enemiesNearby = Physics.OverlapSphere(transform.position, enemyAvoidanceRange, enemyMask);
        foreach (Collider enemy in enemiesNearby)
        {
            if (enemy.transform != transform)
            {
                Vector3 awayFromEnemy = (transform.position - enemy.transform.position).normalized;
                rb.AddForce(awayFromEnemy * obstacleAvoidanceStrength, ForceMode.VelocityChange);
            }
        }
    }

    private void ManeuverAndFire()
    {
        if (isDodging) return;  // Skip firing while dodging

        // Ascend above the player occasionally to spam fire
        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.position);
        if (!isSpammingFire && distanceToPlayer <= fireRange)
        {
            if (Random.Range(0f, 1f) > 0.7f)  // Random chance to ascend and spam fire
            {
                AscendAndSpamFire();
                return;
            }
        }

        // Move towards and engage the player normally
        Vector3 directionToPlayer = playerPlane.position - transform.position;
        directionToPlayer.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Apply banking effect
        float angleDifference = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);
        float bank = Mathf.Clamp(-angleDifference / 90f, -1f, 1f);  // Bank based on turn
        transform.Rotate(Vector3.forward, bank * bankAngle * Time.deltaTime);

        // Move forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (distanceToPlayer <= fireRange)
        {
            HandleFiring();
        }
    }

    private void AscendAndSpamFire()
    {
        // Ascend above the player plane and spam fire while ascending
        Vector3 ascendPosition = playerPlane.position + Vector3.up * abovePlayerHeight;
        Vector3 directionToAscend = ascendPosition - transform.position;
        Quaternion ascendRotation = Quaternion.LookRotation(directionToAscend);

        transform.rotation = Quaternion.Slerp(transform.rotation, ascendRotation, rotationSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Start spamming fire when above the player
        if (Vector3.Distance(transform.position, ascendPosition) < 10f)
        {
            StartCoroutine(SpamFire());
        }
    }

    private System.Collections.IEnumerator SpamFire()
    {
        isSpammingFire = true;
        for (int i = 0; i < 5; i++)  // Spam 5 bullets in quick succession
        {
            Fire();
            yield return new WaitForSeconds(0.2f);  // Fire rapidly
        }
        isSpammingFire = false;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !isDead)
        {
            // Check if the bullet is from the player
            if (collision.gameObject.GetComponent<Bullet>().IsFromPlayer)  // Assuming you have a Bullet script with an IsFromPlayer property
            {
                Die();
            }
        }
    }

    private void Die()
    {
        isDead = true;

        // Update the kill counter
        KillCounterManager killCounterManager = FindObjectOfType<KillCounterManager>();
        if (killCounterManager != null)
        {
            killCounterManager.RegisterKill();
        }

        if (explosionEffect != null)
        {
            explosionEffect.transform.SetParent(null);
            explosionEffect.Play();
        }
        moveSpeed = 0;
        rotationSpeed = 0;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        Destroy(gameObject, 5f);
    }

    public void SetPlayer(Transform player)
    {
        playerPlane = player;
    }
}
