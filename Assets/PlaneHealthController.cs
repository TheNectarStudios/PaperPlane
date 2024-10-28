using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlaneHealthController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public ParticleSystem smokeEffect;
    public ParticleSystem explosionEffect;
    public AudioSource explosionSound;
    public PaperPlanePilot planeController;

    private bool isDestroyed = false;
    public GameObject[] planeParts;

    void Start()
    {
        currentHealth = maxHealth;

        // Ensure particle effects are stopped initially
        if (smokeEffect != null)
        {
            smokeEffect.Stop();
            Debug.Log("Smoke effect stopped initially.");
        }
        
        if (explosionEffect != null)
        {
            explosionEffect.Stop();
            Debug.Log("Explosion effect stopped initially.");
        }
    }

    void Update()
    {
        if (!isDestroyed && currentHealth <= maxHealth / 2)
        {
            if (smokeEffect != null && !smokeEffect.isPlaying)
            {
                smokeEffect.Play();
                Debug.Log("Smoke effect started due to low health.");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;
        Debug.Log("Plane took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
            TriggerDestruction();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet") || collision.collider.CompareTag("Missile") || collision.collider.CompareTag("Enemy"))
        {
            TakeDamage(10);
        }
        else
        {
            TriggerDestruction();
        }
    }

    void TriggerDestruction()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        planeController.enabled = false;

        // Trigger explosion effect and sound
            explosionEffect.Play();
            Debug.Log("Explosion effect triggered.");
        
        if (explosionSound != null && !explosionSound.isPlaying)
        {
            explosionSound.Play();
            Debug.Log("Explosion sound played.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * 5f;
            rb.useGravity = true;
        }

        StartCoroutine(BreakApartSequence());

        // Load Game Over scene after a delay
        Invoke("LoadGameOverScene", 3f);
    }

    IEnumerator BreakApartSequence()
    {
        foreach (GameObject part in planeParts)
        {
            if (part != null)
            {
                part.transform.parent = null;

                Rigidbody partRb = part.GetComponent<Rigidbody>();
                if (partRb == null)
                    partRb = part.AddComponent<Rigidbody>();

                partRb.useGravity = true;
                Vector3 randomForce = Random.onUnitSphere * Random.Range(5f, 15f);
                partRb.AddForce(randomForce, ForceMode.Impulse);

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
}
