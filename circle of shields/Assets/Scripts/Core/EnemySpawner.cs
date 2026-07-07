using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    public GameObject SpawnEnemy(GameObject prefab, int pointIndex = -1, bool makeGolden = false)
    {
        SpawnPoint point = ChoosePoint(pointIndex);
        
        if (point == null)
        {
            Debug.LogError("No spawn points configured!");
            return null;
        }
        
        // Спавним врага
        GameObject enemy = Instantiate(prefab, point.SpawnPosition, Quaternion.identity);
        
        // Целевая позиция входа
        Vector2 targetPosition = point.GetRandomEntryTarget();
        
        // Добавляем компонент входа на арену
        EnemiesEntry entry = enemy.AddComponent<EnemiesEntry>();
        entry.StartEntry(targetPosition, point.EntrySpeed);
        
        // Делаем золотым если нужно
        if (makeGolden)
        {
            enemy.AddComponent<EnemyGolden>();
            MageDialogue dialogue = FindObjectOfType<MageDialogue>();
            if (dialogue != null)
                dialogue.OnGoldenAppears();
        }
        
        Debug.Log("Spawned " + prefab.name + " at " + point.PointName + 
                  (makeGolden ? " ★GOLDEN" : ""));
        return enemy;
    }
    
    private SpawnPoint ChoosePoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        
        // Конкретная точка
        if (index >= 0 && index < spawnPoints.Length)
            return spawnPoints[index];
        
        // Случайная точка
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    // Получить количество точек (для UI/дебага)
    public int SpawnPointCount => spawnPoints != null ? spawnPoints.Length : 0;
}