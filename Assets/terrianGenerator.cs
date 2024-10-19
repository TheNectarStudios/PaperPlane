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
    for (int i = 0; i < geometricShapeCount; i++)
    {
        // Select a random geometric shape from the array
        GameObject randomShape = geometricPrefabs[Random.Range(0, geometricPrefabs.Length)];

        // Randomize the position within the terrain chunk
        Vector3 randomPosition = new Vector3(
            Random.Range(-terrainChunkSize / 2, terrainChunkSize / 2),  // X-axis within the chunk width
            Random.Range(0f, 5f),  // Y-axis for height (adjust based on your design)
            Random.Range(-terrainChunkSize / 2, terrainChunkSize / 2)   // Z-axis within the chunk length
        );

        // Instantiate the shape on the terrain
        GameObject shapeInstance = Instantiate(randomShape, randomPosition + terrainTransform.position, Quaternion.identity);
        shapeInstance.transform.SetParent(terrainTransform);  // Set the shape as a child of the terrain for proper chunk management

        // Randomize the scale of the shape
        float randomScale = Random.Range(1f, 10f); // Adjust the range as needed
        shapeInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
    }
}

}
