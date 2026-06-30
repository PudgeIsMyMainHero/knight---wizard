using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public enum WaveState { Preparing, Spawning, InProgress, Completed, GameOver }
    
    [Header("Waves")]
    [SerializeField] private WaveData[] waves;
    
    [Header("Timing")]
    [SerializeField] private float preparationTime = 3f;
    [SerializeField] private float timeBetweenWaves = 3f;
    
    [Header("References")]
    [SerializeField] private EnemySpawner spawner;
    
    // State
    private WaveState currentState = WaveState.Preparing;
    private int currentWaveIndex = 0;
    private float stateTimer = 0f;
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isSpawning = false;
    
    // Properties
    public WaveState CurrentState => currentState;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Length;
    public int EnemiesAlive => aliveEnemies.Count;
    
    private void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("No waves configured!");
            return;
        }
        
        stateTimer = preparationTime;
        currentState = WaveState.Preparing;
        
        Debug.Log("=== GAME START === Waves: " + waves.Length);
        Debug.Log("Wave 1 starts in " + preparationTime + "s");
    }
    
    private void Update()
    {
        // Чистим уничтоженных врагов из списка
        aliveEnemies.RemoveAll(e => e == null);
        
        switch (currentState)
        {
            case WaveState.Preparing:
                UpdatePreparing();
                break;
            case WaveState.InProgress:
                UpdateInProgress();
                break;
            case WaveState.Completed:
                UpdateCompleted();
                break;
        }
    }
    
    private void UpdatePreparing()
    {
        stateTimer -= Time.deltaTime;
        
        if (stateTimer <= 0)
        {
            StartWave();
        }
    }
    
    private void StartWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            Victory();
            return;
        }
        
        WaveData wave = waves[currentWaveIndex];
        
        Debug.Log("=== WAVE " + (currentWaveIndex + 1) + ": " + wave.waveName + " ===");
        
        currentState = WaveState.Spawning;
        StartCoroutine(SpawnWave(wave));
    }
    
    private IEnumerator SpawnWave(WaveData wave)
    {
        isSpawning = true;
        
        foreach (EnemySpawnInfo group in wave.enemies)
        {
            if (group.prefab == null) continue;
            
            for (int i = 0; i < group.count; i++)
            {
                GameObject enemy = spawner.SpawnEnemy(group.prefab);
                
                if (enemy != null)
                    aliveEnemies.Add(enemy);
                
                // Задержка между спавном каждого врага
                if (group.spawnDelay > 0)
                    yield return new WaitForSeconds(group.spawnDelay);
            }
            
            // Пауза между группами врагов
            if (wave.timeBetweenGroups > 0)
                yield return new WaitForSeconds(wave.timeBetweenGroups);
        }
        
        isSpawning = false;
        currentState = WaveState.InProgress;
        
        Debug.Log("All enemies spawned! Total: " + aliveEnemies.Count);
    }
    
    private void UpdateInProgress()
    {
        // Все враги мертвы?
        if (aliveEnemies.Count == 0 && !isSpawning)
        {
            WaveCleared();
        }
    }
    
    private void WaveCleared()
    {
        Debug.Log("=== WAVE " + (currentWaveIndex + 1) + " CLEARED! ===");
        
        currentWaveIndex++;
        
        if (currentWaveIndex >= waves.Length)
        {
            Victory();
            return;
        }
        
        // Пауза перед следующей волной
        currentState = WaveState.Completed;
        stateTimer = timeBetweenWaves;
        
        Debug.Log("Next wave in " + timeBetweenWaves + "s");
    }
    
    private void UpdateCompleted()
    {
        stateTimer -= Time.deltaTime;
        
        if (stateTimer <= 0)
        {
            currentState = WaveState.Preparing;
            stateTimer = 0;
            StartWave();
        }
    }
    
    private void Victory()
    {
        currentState = WaveState.GameOver;
        Debug.Log("=== VICTORY! ALL WAVES CLEARED! ===");
    }
}