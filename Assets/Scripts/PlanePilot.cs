using UnityEngine;
using UnityEngine.UI;  // Make sure to include this for Button functionality

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
    public float rollSmoothTime = 0.5f;  // Time taken for roll to smoothly return to normal

    public float stallSpeed = 8.0f;
    public float gravityMultiplier = 0.98f;

    private float targetRollAngle = 0f; // Target roll angle we will smooth towards
    private float currentRollAngle = 0f; // Current roll angle for smoothing

    // Firing related variables
    public Transform firePoint;  // Where bullets spawn from
    public GameObject bulletPrefab;  // The player's bullet prefab
    public float fireRate = 1.0f;  // Time between shots
    private float fireTimer = 0f;
    
    // Button for firing
    public Button fireButton;

    void Start()
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
    }

    void Update()
    {
        Vector3 moveCamTo = transform.position - transform.forward * 10.0f + Vector3.up * 5.0f;
        float bias = 0.96f;
        Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);
        Camera.main.transform.LookAt(transform.position + transform.forward * 30.0f);

        rb.velocity = transform.forward * speed;

        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        // Apply yaw (left-right turning)
        ApplyYaw(horizontalInput);

        // Apply pitch (altitude adjustment)
        ApplyPitch(verticalInput);

        // Adjust speed based on vertical input
        AdjustSpeedWithAltitude(verticalInput);

        // Prevent the plane from going through terrain
        CheckTerrainCollision();
        
        fireTimer += Time.deltaTime;
    }

    void ApplyYaw(float horizontalInput)
    {
        // Yaw (rotation around Y axis)
        float currentYaw = transform.eulerAngles.y;
        float targetYaw = currentYaw + (horizontalInput * turnSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetYaw, transform.eulerAngles.z);

        // Handle roll (tilting for visual feedback)
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // Set the target roll angle based on the joystick input
            targetRollAngle = Mathf.Lerp(0, 45, Mathf.Abs(horizontalInput));
            targetRollAngle = horizontalInput < 0 ? -targetRollAngle : targetRollAngle;  // Negative roll for left turns
        }
        else
        {
            // Smoothly return the roll angle to zero when no input is detected
            targetRollAngle = 0f;
        }

        // Smoothly interpolate the roll angle back to the target roll angle
        currentRollAngle = Mathf.Lerp(currentRollAngle, targetRollAngle, Time.deltaTime / rollSmoothTime);

        // Apply roll to the plane (rotation around Z axis)
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -currentRollAngle);
    }

    void ApplyPitch(float verticalInput)
    {
        if (verticalInput < 0 && !CanDescend())
        {
            verticalInput = 0;
        }

        float currentPitch = transform.eulerAngles.x;
        float targetPitch = currentPitch - (verticalInput * pitchSpeed * Time.deltaTime);

        transform.eulerAngles = new Vector3(targetPitch, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    void AdjustSpeedWithAltitude(float verticalInput)
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

    bool CanDescend()
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

    void CheckTerrainCollision()
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

    void Fire()
    {
        if (fireTimer >= fireRate)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.velocity = transform.forward * 200f;  // Set bullet speed
            fireTimer = 0f;
        }
    }
}
