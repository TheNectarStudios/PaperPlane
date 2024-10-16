using UnityEngine;

public class PlanePilot : MonoBehaviour
{
    public DynamicJoystick joystick;  // Reference to the joystick
    public float speed = 50.0f;

    void Start()
    {
        Debug.Log("Plane pilot script added to: " + gameObject.name);
    }

    void Update()
    {
        // Move the camera to follow the plane
        Vector3 moveCamTo = transform.position - transform.forward * 10.0f + Vector3.up * 5.0f;
        float bias = 0.96f;
        Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);
        Camera.main.transform.LookAt(transform.position + transform.forward * 30.0f);

        // Move the plane forward
        transform.position += transform.forward * Time.deltaTime * speed;
        speed -= transform.forward.y * Time.deltaTime * 50.0f;

        if (speed < 35.0f)
        {
            speed = 35.0f;
        }

        // Rotate the plane using joystick input
        float verticalInput = joystick.Vertical;  // Vertical input from joystick (up/down)
        float horizontalInput = joystick.Horizontal;  // Horizontal input from joystick (left/right)
        transform.Rotate(verticalInput, 0.0f, -horizontalInput);  // Apply joystick input to rotation

        // Check terrain height to prevent flying into the ground
        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
        {
            transform.position = new Vector3(transform.position.x, terrainHeightWhereWeAre, transform.position.z);
        }
    }
}
