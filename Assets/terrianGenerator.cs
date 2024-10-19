using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject terrainPrefab;  // Assign your terrain prefab in the inspector
    public Transform player;          // The player that moves forward
    public int terrainChunkSize = 100; // Size of each terrain chunk (e.g., 100 units wide)
    public int chunksVisible = 3;     // Number of chunks visible ahead of the player

    private Vector3 lastPlayerPosition;
    private Queue<GameObject> terrainChunks = new Queue<GameObject>();
    private float terrainZOffset = 0;  // Z position where the next chunk will be spawned

    void Start()
    {
        // Check if the player has been assigned manually or find it by tag
        if (player == null)
        {
            Debug.LogWarning("Player is not assigned in the Inspector, trying to find the player by tag.");
            GameObject foundPlayer = GameObject.FindWithTag("Player"); // Ensure your player GameObject has the "Player" tag

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                Debug.Log("Player found and assigned dynamically.");
            }
            else
            {
                Debug.LogError("Player is not assigned and could not be found in the scene. Please assign the Player in the Inspector or ensure the Player has the 'Player' tag.");
                return;  // Prevent further execution if the player is not assigned
            }
        }

        // If player is assigned, initialize the player's position and terrain generation
        lastPlayerPosition = player.position;
        GenerateInitialTerrain();
    }

    void Update()
    {
        // Check if the player has moved beyond the threshold to generate new terrain
        if (player != null && player.position.z > lastPlayerPosition.z + terrainChunkSize)
        {
            SpawnTerrainChunk();
            lastPlayerPosition = player.position;
        }
    }

    void GenerateInitialTerrain()
    {
        // Generate the initial terrain chunks
        for (int i = 0; i < chunksVisible; i++)
        {
            SpawnTerrainChunk();
        }
    }

    void SpawnTerrainChunk()
    {
        // Instantiate the terrain prefab at the next position
        Vector3 spawnPosition = new Vector3(0, 0, terrainZOffset);
        GameObject newChunk = Instantiate(terrainPrefab, spawnPosition, Quaternion.identity);
        terrainChunks.Enqueue(newChunk);
        terrainZOffset += terrainChunkSize;

        // Remove and destroy old terrain chunks that are far behind the player
        if (terrainChunks.Count > chunksVisible)
        {
            GameObject oldChunk = terrainChunks.Dequeue();
            Destroy(oldChunk);
        }
    }
}
