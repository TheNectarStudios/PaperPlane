using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float despawnDistance;
    public float moveSpeed = 50f;
    public float rotationSpeed = 3f;
    public float attackDistance = 150f;
    public float fireRange = 80f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float circleRadius = 200f;
    public float reengageTime = 3f;
    public ParticleSystem explosionEffect;

    private float fireTimer = 0f;
    private Transform playerPlane;
    private Rigidbody rb;
    private bool isDead = false;
    private bool isCircling = false;

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
        
        if (!isCircling)
        {
            PerformTacticalMove();
        }
    }

    private void PerformTacticalMove()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.position);

        if (distanceToPlayer > attackDistance)
        {
            // Approach the attack distance
            Vector3 directionToPlayer = (playerPlane.position - transform.position).normalized;
            rb.velocity = directionToPlayer * moveSpeed;
        }
        else
        {
            // Begin circling if within attack range
            StartCoroutine(CirclePlayer());
        }
    }

    private IEnumerator CirclePlayer()
    {
        isCircling = true;

        Vector3 circleCenter = playerPlane.position;
        Vector3 directionToCircle = Vector3.Cross((transform.position - circleCenter).normalized, Vector3.up);

        float circleTime = 0f;
        while (circleTime < reengageTime)
        {
            Vector3 circlePosition = circleCenter + directionToCircle * circleRadius;
            Vector3 directionToPosition = (circlePosition - transform.position).normalized;
            
            // Move towards the circular path and rotate towards the player
            rb.velocity = directionToPosition * moveSpeed;
            RotateTowardsPlayer();

            if (Vector3.Distance(transform.position, playerPlane.position) <= fireRange)
            {
                HandleFiring();
            }

            circleTime += Time.deltaTime;
            yield return null;
        }

        isCircling = false;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = (playerPlane.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        bulletRb.velocity = transform.forward * 500f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !isDead)
        {
            if (collision.gameObject.GetComponent<Bullet>().IsFromPlayer)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        isDead = true;

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
