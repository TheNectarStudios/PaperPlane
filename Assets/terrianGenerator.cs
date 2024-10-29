using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject terrainPrefab;  // Assign your terrain prefab in the inspector
    public GameObject[] geometricPrefabs; // Array of geometric shapes to spawn
    public Transform player;          // The player that moves forward
    public int terrainChunkSize = 1000; // Adjusted for 1000x1000 terrain chunks
    public int chunksVisible = 2;      // Reduce this to lower the number of loaded chunks (2 chunks in each direction)
    public int geometricShapeCount = 10; // Number of shapes to spawn per chunk
    public float minSpawnDistance = 50f; // Minimum distance between shapes to avoid overlap

    private Vector3 lastPlayerPosition;
    private Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();

    void Start()
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
                Debug.LogError("Player is not assigned and could not be found. Please assign the Player in the Inspector.");
                return;
            }
        }

        lastPlayerPosition = player.position;
        GenerateInitialTerrain();
    }

    void Update()
    {
        if (player != null)
        {
            GenerateTerrainAroundPlayer();
        }
    }

    void GenerateInitialTerrain()
    {
        // Generate the initial grid of terrain around the player
        GenerateTerrainAroundPlayer();
    }

    void GenerateTerrainAroundPlayer()
    {
        int playerChunkX = Mathf.RoundToInt(player.position.x / terrainChunkSize);
        int playerChunkZ = Mathf.RoundToInt(player.position.z / terrainChunkSize);

        for (int zOffset = -chunksVisible; zOffset <= chunksVisible; zOffset++)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++)
            {
                Vector2 chunkCoords = new Vector2(playerChunkX + xOffset, playerChunkZ + zOffset);
                if (!terrainChunks.ContainsKey(chunkCoords))
                {
                    SpawnTerrainChunk(chunkCoords);
                }
            }
        }

        // Remove faraway chunks
        List<Vector2> chunksToRemove = new List<Vector2>();
        foreach (var chunk in terrainChunks)
        {
            if (Mathf.Abs(chunk.Key.x - playerChunkX) > chunksVisible || Mathf.Abs(chunk.Key.y - playerChunkZ) > chunksVisible)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkCoords in chunksToRemove)
        {
            Destroy(terrainChunks[chunkCoords]);
            terrainChunks.Remove(chunkCoords);
        }
    }

    void SpawnTerrainChunk(Vector2 chunkCoords)
    {
        Vector3 spawnPosition = new Vector3(chunkCoords.x * terrainChunkSize, 0, chunkCoords.y * terrainChunkSize);
        GameObject newChunk = Instantiate(terrainPrefab, spawnPosition, Quaternion.identity);
        terrainChunks.Add(chunkCoords, newChunk);

        // Spawn geometric shapes on the new chunk
        SpawnGeometricShapes(newChunk.transform);
    }

    void SpawnGeometricShapes(Transform terrainTransform)
    {
        List<Vector3> spawnPositions = new List<Vector3>(); // Store positions to check for overlap

        for (int i = 0; i < geometricShapeCount; i++)
        {
            GameObject randomShape = geometricPrefabs[Random.Range(0, geometricPrefabs.Length)];
            Vector3 randomPosition;

            // Try to find a position far enough from other shapes
            int maxAttempts = 10;
            int attempts = 0;
            bool positionFound = false;
            do
            {
                randomPosition = new Vector3(
                    Random.Range(-terrainChunkSize / 2, terrainChunkSize / 2),  // X-axis within the chunk width
                    0,  // Y-axis set to 0 to maintain consistent height
                    Random.Range(-terrainChunkSize / 2, terrainChunkSize / 2)   // Z-axis within the chunk length
                ) + terrainTransform.position;

                positionFound = true;
                foreach (Vector3 pos in spawnPositions)
                {
                    if (Vector3.Distance(pos, randomPosition) < minSpawnDistance)
                    {
                        positionFound = false;
                        break;
                    }
                }
                attempts++;
            } while (!positionFound && attempts < maxAttempts);

            if (!positionFound)
            {
                Debug.LogWarning("Failed to find non-overlapping position for shape " + i);
                continue;
            }

            // Instantiate the shape with a rotation of -90 degrees on the X-axis
            Quaternion rotation = Quaternion.Euler(-90, 0, 0);
            GameObject shapeInstance = Instantiate(randomShape, randomPosition, rotation);
            shapeInstance.transform.SetParent(terrainTransform);

            // Randomize the scale of the shape
            float randomScale = Random.Range(20f, 40f); // Adjust the range as needed
            shapeInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Add the position to the list for future overlap checking
            spawnPositions.Add(randomPosition);
        }
    }
}
