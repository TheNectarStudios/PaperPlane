using UnityEngine;

public class PlanePilot : MonoBehaviour
{
    public float speed = 50.0f;

    // Called once at the start
    void Start()
    {
        Debug.Log("Plane pilot script added to: " + gameObject.name);
    }

    // Called once per frame
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

        // Ensure speed does not drop below 35
        if (speed < 35.0f)
        {
            speed = 35.0f;
        }

        // Rotate the plane based on player input
        transform.Rotate(Input.GetAxis("Vertical"), 0.0f, -Input.GetAxis("Horizontal"));

        // Check terrain height to prevent flying into the ground
        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
        {
            transform.position = new Vector3(transform.position.x, terrainHeightWhereWeAre, transform.position.z);
        }
    }     
}
