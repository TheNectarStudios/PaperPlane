using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // The enemy plane prefab
    public Transform playerPlane;  // Reference to the player's plane
    public float spawnRange = 200f;  // Range within which enemies will spawn
    public int maxEnemies = 5;  // Maximum number of enemies at a time
    public float spawnInterval = 5.0f;  // Time between spawns
    public float despawnDistance = 200f;  // Distance after which enemies despawn

    private float spawnTimer = 0f;
    private int currentEnemies = 0;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        // Generate a random position near the player's plane
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            Random.Range(10f, 50f),  // Keep enemies in the air
            Random.Range(-spawnRange, spawnRange)
        );
        Vector3 spawnPosition = playerPlane.position + randomOffset;

        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Set the player's plane on the enemy AI script
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.SetPlayer(playerPlane);  // Assign the player's plane to the enemy
            enemyAI.despawnDistance = despawnDistance;  // Set despawn distance
        }

        currentEnemies++;  // Increment enemy count

        // Decrement the count when enemy is destroyed (handled in EnemyAI)
    }
}
