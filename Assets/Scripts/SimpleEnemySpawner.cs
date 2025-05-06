using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleEnemySpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public float spawnRadius = 15f;           // Maximum distance from player
    public float minDistanceFromPlayer = 8f;  // Minimum distance from player
    public float timeBetweenWaves = 5f;
    public float pregameDelay = 5f;
    
    [Header("Map Corners")]
    public bool useMapCorners = true;
    public Vector3[] mapCorners = new Vector3[4]; // The four corners of your map
    public float cornerSpawnRadius = 10f;     // Spawn radius from each corner
    
    [Header("Enemy Prefabs")]
    public GameObject smallEnemyPrefab;
    public GameObject mediumEnemyPrefab;
    public GameObject bigEnemyPrefab;
    
    [Header("Enemy Spawn Rules")]
    public int mediumEnemyFirstWave = 3;
    public int bigEnemyFirstWave = 5;
    public int smallEnemiesPerWave = 3;
    public int mediumEnemiesPerWave = 2;
    public int bigEnemiesPerWave = 1;
    public int waveIncrement = 1;
    
    [Header("References")]
    public Transform player;
    private Camera mainCamera; // Reference to main camera
    
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool isWaitingForNextWave = false;
    private List<int> availableCorners = new List<int>(); // To track which corners to use
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player found automatically");
            }
            else
            {
                Debug.LogError("No Player found! Please assign Player transform or ensure object with 'Player' tag exists");
            }
        }
        
        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found in scene!");
        }
        
        ValidatePrefabs();
        
        // Check if map corners are defined
        if (useMapCorners && mapCorners.Length == 0)
        {
            Debug.LogWarning("Map corners requested but not defined. Using default corner calculation.");
            CalculateDefaultMapCorners();
        }
        
        StartCoroutine(PreGameDelay());
    }

    // Calculate default map corners based on spawn radius if none are provided
    void CalculateDefaultMapCorners()
    {
        float mapSize = spawnRadius * 2;
        mapCorners = new Vector3[4]
        {
            new Vector3(-mapSize, 0, -mapSize), // Bottom-left
            new Vector3(mapSize, 0, -mapSize),  // Bottom-right
            new Vector3(-mapSize, 0, mapSize),  // Top-left
            new Vector3(mapSize, 0, mapSize)    // Top-right
        };
        
        Debug.Log("Generated default map corners using spawn radius: " + mapSize);
    }

    IEnumerator PreGameDelay()
    {
        Debug.Log("Pregame countdown started. First wave begins in " + pregameDelay + " seconds.");
        yield return new WaitForSeconds(pregameDelay);
        Debug.Log("Pregame over. Starting Wave 1!");
        StartNextWave();
    }
    
    void Update()
    {
        if (!waveInProgress && !isWaitingForNextWave && enemiesAlive <= 0)
        {
            StartCoroutine(PrepareNextWave());
        }
    }
    
    IEnumerator PrepareNextWave()
    {
        isWaitingForNextWave = true;
        Debug.Log("Wave " + currentWave + " completed! Next wave in " + timeBetweenWaves + " seconds.");
        
        yield return new WaitForSeconds(timeBetweenWaves);
        
        currentWave++;
        StartNextWave();
        
        isWaitingForNextWave = false;
    }
    
    void StartNextWave()
    {
        waveInProgress = true;
        
        int smallCount = Mathf.Min(smallEnemiesPerWave + (waveIncrement * (currentWave - 1)), 50);
        int rangedCount = (currentWave >= mediumEnemyFirstWave) 
            ? Mathf.Min(mediumEnemiesPerWave + (waveIncrement * (currentWave - mediumEnemyFirstWave)), 30) 
            : 0;
        int bigCount = (currentWave >= bigEnemyFirstWave) 
            ? Mathf.Min(bigEnemiesPerWave + Mathf.FloorToInt(waveIncrement * 0.5f * (currentWave - bigEnemyFirstWave)), 20) 
            : 0;
        
        // Reset available corners for this wave
        ResetAvailableCorners();
        
        Debug.Log($"Starting Wave {currentWave}: Small:{smallCount}, Ranged:{rangedCount}, Big:{bigCount}");
        StartCoroutine(SpawnEnemies(smallCount, rangedCount, bigCount));
    }
    
    void ResetAvailableCorners()
    {
        availableCorners.Clear();
        for (int i = 0; i < mapCorners.Length; i++)
        {
            availableCorners.Add(i);
        }
    }
    
    IEnumerator SpawnEnemies(int smallCount, int rangedCount, int bigCount)
    {
        // Distribute small enemies among corners
        for (int i = 0; i < smallCount; i++)
        {
            if (!SpawnEnemy(smallEnemyPrefab)) Debug.LogWarning("Failed to spawn small enemy");
            yield return new WaitForSeconds(0.3f);
            if (i % 10 == 0) yield return null;
        }
        
        // Distribute medium enemies among corners
        for (int i = 0; i < rangedCount; i++)
        {
            if (!SpawnEnemy(mediumEnemyPrefab)) Debug.LogWarning("Failed to spawn ranged enemy");
            yield return new WaitForSeconds(0.5f);
            if (i % 10 == 0) yield return null;
        }
        
        // Distribute big enemies among corners
        for (int i = 0; i < bigCount; i++)
        {
            if (!SpawnEnemy(bigEnemyPrefab)) Debug.LogWarning("Failed to spawn big enemy");
            yield return new WaitForSeconds(0.7f);
            if (i % 10 == 0) yield return null;
        }
        
        waveInProgress = false;
    }
    
    bool SpawnEnemy(GameObject enemyPrefab)
    {
        if (player == null || enemyPrefab == null || mainCamera == null)
        {
            Debug.LogWarning($"Spawn failed: Player is {(player == null ? "null" : "present")}, Prefab is {(enemyPrefab == null ? "null" : "present")}, Camera is {(mainCamera == null ? "null" : "present")}");
            return false;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        if (enemy == null)
        {
            Debug.LogError("Enemy instantiation failed!");
            return false;
        }
        
        Debug.Log($"Spawned enemy at {spawnPosition}, Distance from player: {Vector3.Distance(spawnPosition, player.position)}");
        enemiesAlive++;
        
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript == null)
        {
            Debug.LogWarning($"Enemy at {spawnPosition} missing BaseEnemy component!");
        }
        else
        {
            EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
            notifier.spawner = this;
            
            float healthMultiplier = 1f + ((currentWave - 1) * 0.1f);
            enemyScript.maxHealth = Mathf.RoundToInt(enemyScript.maxHealth * healthMultiplier);
            enemyScript.currentHealth = enemyScript.maxHealth;
        }
        
        return true;
    }
    
    Vector3 GetSpawnPosition()
    {
        if (useMapCorners && availableCorners.Count > 0 && mapCorners.Length > 0)
        {
            return GetCornerSpawnPosition();
        }
        else
        {
            return GetRadialSpawnPosition();
        }
    }
    
    Vector3 GetCornerSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        int attempts = 0;
        const int maxAttempts = 30;
        bool validPosition = false;
        
        // Get a random corner index from available corners
        int cornerIdx = 0;
        if (availableCorners.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCorners.Count);
            cornerIdx = availableCorners[randomIndex];
            
            // Remove the corner from available list after using it a few times
            // This ensures we use different corners for spawning
            if (Random.value < 0.3f)
            {
                availableCorners.RemoveAt(randomIndex);
                if (availableCorners.Count == 0)
                {
                    ResetAvailableCorners();
                }
            }
        }
        else
        {
            ResetAvailableCorners();
            cornerIdx = availableCorners[0];
        }
        
        Vector3 cornerPosition = mapCorners[cornerIdx];
        
        while (attempts < maxAttempts && !validPosition)
        {
            // Generate random position around the selected corner
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(1f, cornerSpawnRadius);
            spawnPos = cornerPosition + Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
            
            // Adjust height to terrain
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
            {
                spawnPos.y = hit.point.y + 0.5f; // Spawn slightly above ground
                float distanceToPlayer = Vector3.Distance(spawnPos, player.position);
                
                // Check if position is outside camera frustum
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(spawnPos);
                bool outsideView = viewportPoint.x < 0 || viewportPoint.x > 1 || 
                                 viewportPoint.y < 0 || viewportPoint.y > 1 || 
                                 viewportPoint.z < 0;
                
                if (distanceToPlayer >= minDistanceFromPlayer && outsideView)
                {
                    validPosition = true;
                }
            }
            
            attempts++;
        }
        
        if (!validPosition)
        {
            Debug.LogWarning("Failed to find valid corner spawn position outside camera view, using fallback");
            spawnPos = GetRadialSpawnPosition();
        }
        
        return spawnPos;
    }
    
    Vector3 GetRadialSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        int attempts = 0;
        const int maxAttempts = 30;
        bool validPosition = false;

        if (mainCamera == null)
        {
            Debug.LogWarning("No camera available, using fallback spawn logic");
            return FallbackSpawnPosition();
        }

        while (attempts < maxAttempts && !validPosition)
        {
            // Calculate minimum distance to be outside camera view
            float cameraDistance = mainCamera.nearClipPlane + 1f; // Start just beyond near plane
            float fov = mainCamera.fieldOfView;
            float aspect = mainCamera.aspect;
            float cameraHeight = 2f * cameraDistance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float cameraWidth = cameraHeight * aspect;
            float minSpawnDistance = Mathf.Max(minDistanceFromPlayer, Mathf.Sqrt(cameraWidth * cameraWidth + cameraHeight * cameraHeight) * 0.5f);

            // Generate random position outside camera view
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(minSpawnDistance, spawnRadius);
            spawnPos = player.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

            // Adjust height to terrain
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
            {
                spawnPos.y = hit.point.y + 0.5f; // Spawn slightly above ground
                float distanceToPlayer = Vector3.Distance(spawnPos, player.position);

                // Check if position is outside camera frustum
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(spawnPos);
                bool outsideView = viewportPoint.x < 0 || viewportPoint.x > 1 || 
                                 viewportPoint.y < 0 || viewportPoint.y > 1 || 
                                 viewportPoint.z < 0;

                if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= spawnRadius && outsideView)
                {
                    validPosition = true;
                }
            }
            else
            {
                Debug.LogWarning($"No terrain found at attempt {attempts + 1} for position {spawnPos}");
            }

            attempts++;
        }

        if (!validPosition)
        {
            Debug.LogWarning("Failed to find valid spawn position outside camera view, using fallback");
            spawnPos = FallbackSpawnPosition();
        }

        return spawnPos;
    }
    
    Vector3 FallbackSpawnPosition()
    {
        Vector3 spawnPos = player.position + Vector3.forward * minDistanceFromPlayer;
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
        {
            spawnPos.y = hit.point.y + 0.5f;
        }
        else
        {
            spawnPos.y = player.position.y;
        }
        return spawnPos;
    }
    
    public void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive < 0)
        {
            enemiesAlive = 0;
            Debug.LogWarning("Enemies alive went negative, reset to 0");
        }
    }
    
    void ValidatePrefabs()
    {
        if (smallEnemyPrefab == null) Debug.LogError("Small Enemy Prefab not assigned!");
        if (mediumEnemyPrefab == null) Debug.LogError("medium Enemy Prefab not assigned!");
        if (bigEnemyPrefab == null) Debug.LogError("Big Enemy Prefab not assigned!");
    }
    
    // Editor-only method to visualize the corners in the scene
    void OnDrawGizmosSelected()
    {
        if (useMapCorners)
        {
            Gizmos.color = Color.red;
            
            // Draw the defined corners
            if (mapCorners != null && mapCorners.Length > 0)
            {
                foreach (Vector3 corner in mapCorners)
                {
                    Gizmos.DrawSphere(corner, 1f);
                    Gizmos.DrawWireSphere(corner, cornerSpawnRadius);
                }
            }
        }
    }
}

public class EnemyDeathNotifier : MonoBehaviour
{
    public SimpleEnemySpawner spawner;
    
    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyDeath();
        }
    }
}