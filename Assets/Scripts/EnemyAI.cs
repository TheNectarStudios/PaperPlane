using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float despawnDistance;
    public float moveSpeed = 20f;
    public float maxSpeed = 30f;
    public float minSpeed = 10f;
    public float turnSpeed = 3f;
    public float pitchSpeed = 2f;
    public float rollSpeed = 1.5f;
    public float fireRange = 80f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public ParticleSystem explosionEffect;
    public float liftForce = 5f;
    public float gravityScale = 1f;
    public float fireAngleThreshold = 10f; // Only fire if angle to player is within this threshold

    private float fireTimer = 0f;
    private Transform playerPlane;
    private Rigidbody rb;
    private bool isDead = false;
    private float currentSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;  // Enable gravity
        rb.mass = 1.0f;  // Set mass to create realistic lift

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPlane = player.transform;
        }

        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isDead || playerPlane == null) return;

        HandleMovement();
        HandleFiring();
    }

    private void HandleMovement()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (playerPlane.position - transform.position).normalized;

        // Yaw and pitch towards the player
        ApplyYawAndPitch(directionToPlayer);

        // Move forward
        rb.velocity = transform.forward * currentSpeed;

        // Simulate lift to keep plane level
        ApplyLiftForce();

        // Adjust speed based on distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.position);
        if (distanceToPlayer < fireRange)
        {
            currentSpeed = Mathf.Clamp(currentSpeed - Time.deltaTime * turnSpeed, minSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed + Time.deltaTime * turnSpeed, minSpeed, maxSpeed);
        }
    }

    private void ApplyYawAndPitch(Vector3 directionToPlayer)
    {
        // Calculate target rotation based on player position
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Adjust roll to create a banked turn effect
        float rollAngle = Mathf.Lerp(0, 45, Mathf.Abs(directionToPlayer.x)) * -Mathf.Sign(directionToPlayer.x);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, rollAngle);
    }

    private void ApplyLiftForce()
    {
        Vector3 lift = Vector3.up * liftForce * currentSpeed * gravityScale;
        rb.AddForce(lift - Physics.gravity * rb.mass);
    }

    private void HandleFiring()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate && Vector3.Distance(transform.position, playerPlane.position) <= fireRange)
        {
            // Check if plane is approximately pointing towards player
            Vector3 directionToPlayer = (playerPlane.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fireAngleThreshold)
            {
                Fire();
                fireTimer = 0f;
            }
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
        rb.velocity = Vector3.zero;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        Destroy(gameObject, 5f);
    }

    public void SetPlayer(Transform player)
    {
        playerPlane = player;
    }
}
