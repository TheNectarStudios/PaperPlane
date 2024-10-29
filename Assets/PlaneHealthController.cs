using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this for Image component
using System.Collections;

public class PlaneHealthController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public GameObject smokeEffect;
    public GameObject explosionEffect;
    public AudioSource explosionSound;
    public PaperPlanePilot planeController;
    public Image healthBarImage; // Image component for the health bar

    private bool isDestroyed = false;
    public GameObject[] planeParts;

    void Start()
    {
        currentHealth = maxHealth;

        // Set the health bar to full
        UpdateHealthUI();

        // Ensure particle effects are inactive initially
        if (smokeEffect != null)
        {
            smokeEffect.SetActive(false);
            Debug.Log("Smoke effect set inactive initially.");
        }
        
        if (explosionEffect != null)
        {
            explosionEffect.SetActive(false);
            Debug.Log("Explosion effect set inactive initially.");
        }
    }

    void Update()
    {
        if (!isDestroyed && currentHealth <= maxHealth / 1.2f)
        {
            if (smokeEffect != null && !smokeEffect.activeSelf)
            {
                smokeEffect.SetActive(true);
                Debug.Log("Smoke effect activated due to low health.");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;
        Debug.Log("Plane took damage. Current health: " + currentHealth);

        // Update health UI
        UpdateHealthUI();

        if (currentHealth <= 0)
            TriggerDestruction();
    }

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
        {
            // Set fill amount based on the health percentage
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }
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

    // Set health bar instantly to zero
    if (healthBarImage != null)
    {
        healthBarImage.fillAmount = 0f;
    }

    // Trigger explosion effect and sound
    if (explosionEffect != null)
    {
        explosionEffect.SetActive(true);
        Debug.Log("Explosion effect activated.");
    }
    
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
