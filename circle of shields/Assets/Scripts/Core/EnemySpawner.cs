using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    public GameObject SpawnEnemy(GameObject prefab, int pointIndex = -1)
    {
        SpawnPoint point = ChoosePoint(pointIndex);
        
        if (point == null)
        {
            Debug.LogError("No spawn points configured!");
            return null;
        }
        
        // Спавним врага в точке
        GameObject enemy = Instantiate(prefab, point.SpawnPosition, Quaternion.identity);
        
        // Получаем СЛУЧАЙНУЮ точку прибытия в области
        Vector2 targetPosition = point.GetRandomEntryTarget();
        
        // Добавляем компонент входа
        EnemiesEntry entry = enemy.AddComponent<EnemiesEntry>();
        entry.StartEntry(targetPosition, point.EntrySpeed);
        
        Debug.Log("Spawned " + prefab.name + " at " + point.PointName + " → target " + targetPosition);
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