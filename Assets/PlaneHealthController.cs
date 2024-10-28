using UnityEngine;
using UnityEngine.SceneManagement; // For loading the Game Over scene
using System.Collections;

public class PlaneHealthController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public ParticleSystem smokeEffect;
    public ParticleSystem explosionEffect;
    public AudioSource explosionSound; // Optional, for explosion sound
    public PaperPlanePilot planeController; // Reference to the plane controller script
    
    private bool isDestroyed = false;

    public GameObject[] planeParts; // Array of plane parts to detach on destruction

    void Start()
    {
        currentHealth = maxHealth;
        if (smokeEffect != null)
            smokeEffect.Stop(); // Ensure smoke effect is initially off
    }

    void Update()
    {
        if (!isDestroyed && currentHealth <= maxHealth / 2)
        {
            if (smokeEffect != null && !smokeEffect.isPlaying)
                smokeEffect.Play(); // Start smoke when health is at or below 50%
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            TriggerDestruction();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with something other than bullets or missiles
        if (!collision.collider.CompareTag("Bullet") && !collision.collider.CompareTag("Missile") && !collision.collider.CompareTag("Enemy"))
        {
            TriggerDestruction();
        }
        {
            TriggerDestruction();
        }
    }

    void TriggerDestruction()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        // Stop player control
        planeController.enabled = false;

        // Play explosion effect
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        // Play explosion sound, if available
        if (explosionSound != null)
        {
            explosionSound.Play();
        }

        // Simulate loss of control by applying a downward force
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * 5f;
            rb.useGravity = true;
        }

        // Detach each part in sequence
        StartCoroutine(BreakApartSequence());

        // Wait 3 seconds and then load the Game Over scene
        // Invoke("LoadGameOverScene", 3f);
    }

    IEnumerator BreakApartSequence()
    {
        foreach (GameObject part in planeParts)
        {
            if (part != null)
            {
                // Detach the part from the main plane
                part.transform.parent = null;

                // Enable Rigidbody to let it fall and apply random force
                Rigidbody partRb = part.GetComponent<Rigidbody>();
                if (partRb == null)
                    partRb = part.AddComponent<Rigidbody>(); // Add Rigidbody if not already attached

                partRb.useGravity = true;

                // Apply random force to simulate breakage
                Vector3 randomForce = Random.onUnitSphere * Random.Range(5f, 15f);
                partRb.AddForce(randomForce, ForceMode.Impulse);

                // Wait a bit before breaking the next part
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver"); // Make sure the Game Over scene is added to the build settings
    }
}
