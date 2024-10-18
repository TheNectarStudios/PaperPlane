using UnityEngine;

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

    public Transform cameraTransform;  
    public Vector3 cameraOffset = new Vector3(0.0f, 5.0f, -10.0f);  
    public float smoothSpeed = 0.125f;  
    public float rollTiltAmount = 30.0f;

    public LayerMask terrainLayer;  
    public float minAltitude = 10.0f;  
    public float raycastDistance = 20.0f;  

    public Rigidbody rb;  // Reference to the Rigidbody component
    public float liftForce = 5.0f;  // Force applied upwards to simulate lift
    public float gravityScale = 0.5f;  // Scale down gravity to slow descent

    public float maxTurnAngle = 45.0f;  // Maximum turning angle in degrees

    private Vector3 cameraVelocity = Vector3.zero;  // Used for smoothing the camera

    public float stallSpeed = 8.0f;  // Speed at which the plane stalls
    public float gravityMultiplier = 0.98f;  // Factor for increasing speed during descent

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Set up rigidbody drag and gravity settings
        rb.drag = 2.0f;  // Increase drag to slow down falling speed
        rb.angularDrag = 1.5f;  // Higher angular drag to prevent rapid rotations
        rb.useGravity = true;
        rb.mass = 0.1f;  // Lower mass for a lightweight paper plane effect
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);  // Initial lift

        Physics.gravity = new Vector3(0, -9.81f * gravityScale, 0);  // Scaled-down gravity
    }

    void Update()
    {
        // Apply forward movement
        rb.velocity = transform.forward * speed;

        // Joystick inputs for horizontal and vertical movement
        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        // Apply limited yaw rotation (left-right turning)
        ApplyYaw(horizontalInput);

        // Apply pitch for altitude adjustment
        ApplyPitch(verticalInput);

        // Adjust speed based on altitude and energy conservation principles
        AdjustSpeedWithAltitude(verticalInput);

        // Prevent the plane from going through terrain
        CheckTerrainCollision();
    }

    void LateUpdate()
    {
        // Follow the plane with the camera in LateUpdate for smoother movement
        FollowCamera();
    }

    void ApplyYaw(float horizontalInput)
    {
        // Get the current yaw (rotation around the Y axis)
        float currentYaw = transform.eulerAngles.y;

        // Calculate the new yaw after applying the input
        float targetYaw = currentYaw + (horizontalInput * turnSpeed * Time.deltaTime);

        // Clamp the yaw to ensure it stays within the allowed angle range
        float clampedYaw = ClampAngle(targetYaw, -maxTurnAngle, maxTurnAngle);

        // Apply the clamped yaw back to the plane
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, clampedYaw, transform.eulerAngles.z);
    }

    void ApplyPitch(float verticalInput)
    {
        // Block descending if terrain is too close
        if (verticalInput < 0 && !CanDescend())
        {
            verticalInput = 0;
        }

        // Manually adjust the pitch angle without rotating the plane on the X-axis
        float currentPitch = transform.eulerAngles.x;
        float targetPitch = currentPitch - (verticalInput * pitchSpeed * Time.deltaTime);

        // Apply only pitch without changing any other axis rotations
        transform.eulerAngles = new Vector3(targetPitch, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    void AdjustSpeedWithAltitude(float verticalInput)
    {
        // Adjust speed based on the vertical input and altitude
        if (verticalInput > 0) // Ascending
        {
            speed -= verticalInput * altitudeSpeedMultiplier;

            // If speed drops below stall speed, stop ascending and start descending
            if (speed < stallSpeed)
            {
                speed = stallSpeed;  // Prevent speed from dropping too much
                verticalInput = -1.0f;  // Force the plane to descend
            }
        }
        else if (verticalInput < 0) // Descending
        {
            speed += Mathf.Abs(verticalInput) * altitudeSpeedMultiplier * gravityMultiplier;
        }

        // Clamp speed within the min and max limits
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

    void FollowCamera()
    {
        // Target position for the camera (relative to the plane with offset)
        Vector3 targetPosition = transform.position + cameraOffset;

        // Smoothly move the camera to the target position using SmoothDamp
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPosition, ref cameraVelocity, smoothSpeed);

        // Make the camera look at the plane
        cameraTransform.LookAt(transform.position);
    }

    // Helper function to clamp angles between -maxTurnAngle and maxTurnAngle
    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
