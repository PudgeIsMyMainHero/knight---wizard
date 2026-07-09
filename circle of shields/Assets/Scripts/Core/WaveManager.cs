using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public enum WaveState { Preparing, Spawning, InProgress, Completed, GameOver }
    
    [Header("Waves")]
    [SerializeField] private WaveBudgetData[] waves;
    
    [Header("Global Enemy Pool")]
    [Tooltip("Пул врагов используемый если в волне не указан свой")]
    [SerializeField] private EnemyData[] globalEnemyPool;
    
    [Header("Timing")]
    [SerializeField] private float preparationTime = 3f;
    [SerializeField] private float timeBetweenWaves = 3f;
    
    [Header("References")]
    [SerializeField] private EnemySpawner spawner;
    
    [Header("Smart Spawn")]
    [SerializeField] private bool useSmartSpawn = true;
    [SerializeField] private float maxThreatForSpawn = 15f;
    [SerializeField] private float threatCheckInterval = 0.5f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private bool logThreat = false;
    
    // State
    private WaveState currentState = WaveState.Preparing;
    private int currentWaveIndex = 0;
    private float stateTimer = 0f;
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isSpawning = false;
    
    // Tracking для лимитов
    private Dictionary<EnemyData, int> spawnedThisWave = new Dictionary<EnemyData, int>();
    private Dictionary<EnemyData, float> lastSpawnTime = new Dictionary<EnemyData, float>();
    
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
    }
    
    private void Update()
    {
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
            StartWave();
    }
    
    private void StartWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            Victory();
            return;
        }
        
        WaveBudgetData wave = waves[currentWaveIndex];
        Debug.Log("=== WAVE " + (currentWaveIndex + 1) + ": " + wave.waveName + 
                  " (budget: " + wave.budget + ") ===");
        
        currentState = WaveState.Spawning;
        spawnedThisWave.Clear();
        lastSpawnTime.Clear();
        
        StartCoroutine(SpawnWaveByBudget(wave));
        
        MageDialogue dialogue = FindObjectOfType<MageDialogue>();
        if (dialogue != null)
            dialogue.OnWaveStart();
    }
    
    private IEnumerator SpawnWaveByBudget(WaveBudgetData wave)
    {
        isSpawning = true;
        int remainingBudget = wave.budget;
        int goldensLeft = wave.guaranteedGoldenCount;
        
        EnemyData[] pool = (wave.availableEnemies != null && wave.availableEnemies.Length > 0)
            ? wave.availableEnemies
            : globalEnemyPool;
        
        if (pool == null || pool.Length == 0)
        {
            Debug.LogError("No enemies in pool!");
            isSpawning = false;
            currentState = WaveState.InProgress;
            yield break;
        }
        
        int safetyCounter = 200;
        while (remainingBudget > 0 && safetyCounter-- > 0)
        {
            // === ЛИМИТ ЖИВЫХ ВРАГОВ ===
            yield return StartCoroutine(WaitForEnemyLimit(wave.maxAliveEnemies));
            
            // === SMART SPAWN (проверка threat) ===
            if (useSmartSpawn)
            {
                yield return StartCoroutine(WaitForThreatDrop());
            }
            
            EnemyData chosen = PickAffordableEnemy(pool, remainingBudget);
            
            if (chosen == null)
            {
                Debug.Log("No affordable enemies left. Remaining budget: " + remainingBudget);
                break;
            }
            
            bool isGolden = false;
            if (chosen.canBeGolden)
            {
                if (goldensLeft > 0)
                {
                    isGolden = true;
                    goldensLeft--;
                }
                else if (wave.randomGoldenChance > 0f && Random.value < wave.randomGoldenChance)
                {
                    isGolden = true;
                }
            }
            
            // === ВЫБОР ТОЧКИ СПАВНА ===
            int pointIndex = GetRandomAllowedPoint(wave);
            
            // Спавним
            GameObject enemy = spawner.SpawnEnemy(chosen.prefab, pointIndex, isGolden);
            
            if (enemy != null)
            {
                aliveEnemies.Add(enemy);
                
                if (!spawnedThisWave.ContainsKey(chosen))
                    spawnedThisWave[chosen] = 0;
                spawnedThisWave[chosen]++;
                lastSpawnTime[chosen] = Time.time;
                
                remainingBudget -= chosen.cost;
                
                Debug.Log("Spawned " + chosen.enemyName + " at point " + pointIndex +
                          " (cost " + chosen.cost + 
                          ", budget: " + remainingBudget + 
                          ", alive: " + aliveEnemies.Count + "/" + wave.maxAliveEnemies + ")" + 
                          (isGolden ? " ★" : ""));
            }
            
            float delay = wave.spawnInterval + Random.Range(-wave.spawnIntervalVariance, wave.spawnIntervalVariance);
            delay = Mathf.Max(0.1f, delay);
            yield return new WaitForSeconds(delay);
        }
        
        isSpawning = false;
        currentState = WaveState.InProgress;
        
        Debug.Log("Wave spawn complete.");
    }
    
    /// Выбирает случайную точку из разрешённых для этой волны.
    /// Если список пуст — возвращает -1 (случайная из всех).
    private int GetRandomAllowedPoint(WaveBudgetData wave)
    {
        if (wave.allowedSpawnPointIndices == null || wave.allowedSpawnPointIndices.Length == 0)
            return -1;   // случайная из всех
        
        return wave.allowedSpawnPointIndices[Random.Range(0, wave.allowedSpawnPointIndices.Length)];
    }
    
    private IEnumerator WaitForEnemyLimit(int maxAlive)
    {
        while (true)
        {
            aliveEnemies.RemoveAll(e => e == null);
            
            if (aliveEnemies.Count < maxAlive)
                yield break;
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private IEnumerator WaitForThreatDrop()
    {
        float waitedTime = 0f;
        
        while (waitedTime < maxWaitTime)
        {
            float currentThreat = ThreatCalculator.CalculateCurrentThreat();
            
            if (logThreat)
            {
                Debug.Log("[Threat] " + currentThreat.ToString("F1") + 
                          " / " + maxThreatForSpawn + " (" + 
                          ThreatCalculator.GetThreatCategory(currentThreat) + ")");
            }
            
            if (currentThreat <= maxThreatForSpawn)
                yield break;
            
            yield return new WaitForSeconds(threatCheckInterval);
            waitedTime += threatCheckInterval;
        }
        
        if (logThreat)
            Debug.Log("[Threat] Max wait reached, spawning anyway");
    }
    
    /// Выбирает случайного врага которого можем себе позволить (с учётом лимитов)
    private EnemyData PickAffordableEnemy(EnemyData[] pool, int budget)
    {
        int currentWave = CurrentWaveNumber;
        
        List<EnemyData> affordable = new List<EnemyData>();
        List<float> weights = new List<float>();
        
        foreach (EnemyData enemy in pool)
        {
            if (enemy == null || enemy.prefab == null) continue;
            
            // Цена
            if (enemy.cost > budget) continue;
            
            // Мин волна
            if (currentWave < enemy.minWaveNumber) continue;
            
            // Лимит одновременно
            if (enemy.maxAtOnce > 0)
            {
                int currentCount = CountAliveOfType(enemy);
                if (currentCount >= enemy.maxAtOnce) continue;
            }
            
            // Лимит за волну
            if (enemy.maxPerWave > 0)
            {
                int spawned = spawnedThisWave.ContainsKey(enemy) ? spawnedThisWave[enemy] : 0;
                if (spawned >= enemy.maxPerWave) continue;
            }
            
            // Кулдаун
            if (lastSpawnTime.ContainsKey(enemy))
            {
                float timeSince = Time.time - lastSpawnTime[enemy];
                if (timeSince < enemy.spawnCooldown) continue;
            }
            
            affordable.Add(enemy);
            weights.Add(enemy.spawnWeight);
        }
        
        if (affordable.Count == 0) return null;
        
        return PickWeighted(affordable, weights);
    }
    
    private EnemyData PickWeighted(List<EnemyData> options, List<float> weights)
    {
        float total = weights.Sum();
        float random = Random.Range(0f, total);
        float cumulative = 0f;
        
        for (int i = 0; i < options.Count; i++)
        {
            cumulative += weights[i];
            if (random <= cumulative)
                return options[i];
        }
        
        return options[options.Count - 1];
    }
    
    private int CountAliveOfType(EnemyData data)
    {
        int count = 0;
        foreach (GameObject enemy in aliveEnemies)
        {
            if (enemy != null && enemy.name.StartsWith(data.prefab.name))
                count++;
        }
        return count;
    }
    
    private void UpdateInProgress()
    {
        if (aliveEnemies.Count == 0 && !isSpawning)
            WaveCleared();
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
        
        currentState = WaveState.Completed;
        stateTimer = timeBetweenWaves;
        
        MageDialogue dialogue = FindObjectOfType<MageDialogue>();
        if (dialogue != null)
            dialogue.OnWaveEnd();
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
        Debug.Log("=== VICTORY! ===");
        
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerVictory();
    }
}