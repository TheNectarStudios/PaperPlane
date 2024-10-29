using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PaperPlanePilot : MonoBehaviour
{
    public DynamicJoystick joystick;
    public float speed = 20.0f;
    public float maxSpeed = 25.0f;
    public float minSpeed = 5.0f;
    private float drag = 0.995f;
    private float turnSpeed = 50.0f;
    private float pitchSpeed = 30.0f;
    public float altitudeSpeedMultiplier = 0.5f;
    public float smoothSpeed = 0.125f;

    public LayerMask terrainLayer;
    public float minAltitude = 10.0f;
    public float raycastDistance = 20.0f;

    public Rigidbody rb;
    public float liftForce = 5.0f;
    public float gravityScale = 0.5f;

    public float maxTurnAngle = 45.0f;
    public float rollSmoothTime = 0.5f;

    public float stallSpeed = 8.0f;
    public float gravityMultiplier = 0.98f;

    private float targetRollAngle = 0f;
    private float currentRollAngle = 0f;

    // Firing related variables
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireRate = 1.0f;
    private float fireTimer = 0f;

    // UI Buttons
    public Button fireButton;
    public Button brakeButton;
    public Button boostButton;

    private bool isBraking = false;
    private bool isBoosting = false;
    private float brakeBoostDuration = 3.0f; // Duration to reach min/max speed

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.drag = 2.0f;
        rb.angularDrag = 1.5f;
        rb.useGravity = true;
        rb.mass = 0.1f;
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);

        Physics.gravity = new Vector3(0, -9.81f * gravityScale, 0);

        // Hook the Fire function to the button's click event
        fireButton.onClick.AddListener(Fire);

        // Assign brake and boost functions to respective buttons
        brakeButton.onClick.AddListener(() => isBraking = true);
        boostButton.onClick.AddListener(() => isBoosting = true);
    }

private void Update()
{
    // Calculate desired camera position
    Vector3 moveCamTo = transform.position - transform.forward * 10.0f + Vector3.up * 5.0f;

    // Set maximum distance the camera can be from the plane
    float maxCameraDistance = 15.0f; // Adjust this value as needed
    float currentDistance = Vector3.Distance(transform.position, moveCamTo);

    // If the current distance exceeds the max distance, adjust the camera position
    if (currentDistance > maxCameraDistance)
    {
        Vector3 direction = (moveCamTo - transform.position).normalized;
        moveCamTo = transform.position + direction * maxCameraDistance;
    }

    // Smoothly move the camera to the desired position
    float bias = 0.96f;
    Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);
    Camera.main.transform.LookAt(transform.position + transform.forward * 30.0f);

    rb.velocity = transform.forward * speed;

    float horizontalInput = joystick.Horizontal;
    float verticalInput = joystick.Vertical;

    ApplyYaw(horizontalInput);
    ApplyPitch(verticalInput);
    AdjustSpeedWithAltitude(verticalInput);
    CheckTerrainCollision();

    fireTimer += Time.deltaTime;

    // Apply gradual braking or boosting over time
    if (isBraking)
    {
        GradualBrake();
    }
    else if (isBoosting)
    {
        GradualBoost();
    }
}

    private void ApplyYaw(float horizontalInput)
    {
        float currentYaw = transform.eulerAngles.y;
        float targetYaw = currentYaw + (horizontalInput * turnSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetYaw, transform.eulerAngles.z);

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            targetRollAngle = Mathf.Lerp(0, 45, Mathf.Abs(horizontalInput));
            targetRollAngle = horizontalInput < 0 ? -targetRollAngle : targetRollAngle;
        }
        else
        {
            targetRollAngle = 0f;
        }

        currentRollAngle = Mathf.Lerp(currentRollAngle, targetRollAngle, Time.deltaTime / rollSmoothTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -currentRollAngle);
    }

    private void ApplyPitch(float verticalInput)
    {
        if (verticalInput < 0 && !CanDescend())
        {
            verticalInput = 0;
        }

        float currentPitch = transform.eulerAngles.x;
        float targetPitch = currentPitch - (verticalInput * pitchSpeed * Time.deltaTime);

        transform.eulerAngles = new Vector3(targetPitch, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void AdjustSpeedWithAltitude(float verticalInput)
    {
        if (verticalInput > 0)
        {
            speed -= verticalInput * altitudeSpeedMultiplier;

            if (speed < stallSpeed)
            {
                speed = stallSpeed;
                verticalInput = -1.0f;
            }
        }
        else if (verticalInput < 0)
        {
            speed += Mathf.Abs(verticalInput) * altitudeSpeedMultiplier * gravityMultiplier;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    private bool CanDescend()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, terrainLayer))
        {
            if (hit.distance < minAltitude)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckTerrainCollision()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, terrainLayer))
        {
            if (hit.distance < minAltitude)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + minAltitude, transform.position.z);
            }
        }
    }

    private void Fire()
    {
        if (fireTimer >= fireRate)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.velocity = transform.forward * 200f;
            fireTimer = 0f;
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.IsFromPlayer = true;
            }
        }
    }

    // Gradual brake function
    private void GradualBrake()
    {
        speed = Mathf.MoveTowards(speed, minSpeed, (maxSpeed - minSpeed) / brakeBoostDuration * Time.deltaTime);
        if (speed <= minSpeed)
        {
            isBraking = false;
        }
    }

    // Gradual boost function
    private void GradualBoost()
    {
        speed = Mathf.MoveTowards(speed, maxSpeed, (maxSpeed - minSpeed) / brakeBoostDuration * Time.deltaTime);
        if (speed >= maxSpeed)
        {
            isBoosting = false;
        }
    }
}
