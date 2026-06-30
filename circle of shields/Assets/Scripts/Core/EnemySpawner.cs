using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float arenaRadius = 12f;
    [SerializeField] private float minDistanceFromMage = 8f;
    [SerializeField] private Transform mageTransform;
    
    public GameObject SpawnEnemy(GameObject prefab)
    {
        Vector2 spawnPos = GetSpawnPosition();
        
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        Debug.Log("Spawned: " + prefab.name + " at " + spawnPos);
        return enemy;
    }
    
    private Vector2 GetSpawnPosition()
    {
        // Спавним по краю арены в случайном направлении
        for (int i = 0; i < 30; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromMage, arenaRadius);
            
            Vector2 pos = new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
            
            return pos;
        }
        
        return new Vector2(arenaRadius, 0);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Зона спавна
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, arenaRadius);
        
        // Минимальная дистанция от мага
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, minDistanceFromMage);
    }
}