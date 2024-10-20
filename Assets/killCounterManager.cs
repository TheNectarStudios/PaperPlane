using UnityEngine;

public class KillCounterManager : MonoBehaviour
{
    public KillCounter killCounter;  // Assign your KillCounter script here

    private void Awake()
    {
        // Ensure only one instance of the KillCounterManager exists
        if (FindObjectsOfType<KillCounterManager>().Length > 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // Call this method to register a kill
    public void RegisterKill()
    {
        if (killCounter != null)
        {
            killCounter.EnemyKilled();
        }
    }
}
