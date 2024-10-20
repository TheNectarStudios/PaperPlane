using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // Assign your enemy prefab in the inspector
    public Transform player;         // The player that moves
    public float spawnRange = 100f; // Range around the player to spawn enemies
    public float despawnDistance = 200f; // Distance at which enemies are despawned
    public int maxEnemies = 10;     // Max number of enemies to spawn
    private List<GameObject> enemies = new List<GameObject>();

    private void Start()
    {
        // Ensure the player is assigned
        if (player == null)
        {
            Debug.LogWarning("Player is not assigned. Attempting to find player by tag.");
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("Player not found! Please assign the Player in the Inspector.");
                return;
            }
        }

        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Check if the number of enemies is less than the max allowed
            if (enemies.Count < maxEnemies)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-spawnRange, spawnRange),
                    0,
                    UnityEngine.Random.Range(-spawnRange, spawnRange)
                );

                // Calculate spawn position
                Vector3 spawnPosition = player.position + randomOffset;
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                enemies.Add(enemy);
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.despawnDistance = despawnDistance; // Set despawn distance
                }
            }

            yield return new WaitForSeconds(1f); // Adjust spawn rate here
        }
    }

    private void Update()
    {
        // Check for enemies to despawn
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null || Vector3.Distance(enemies[i].transform.position, player.position) > despawnDistance)
            {
                Destroy(enemies[i]);
                enemies.RemoveAt(i);
            }
        }
    }
}
