using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject prefab;
    public int count = 1;
    public float spawnDelay = 0.5f;
    
    [Tooltip("Индекс точки спавна из массива в WaveManager (-1 = случайная)")]
    public int spawnPointIndex = -1;
}

[CreateAssetMenu(fileName = "Wave", menuName = "CircleOfShields/Wave Data")]
public class WaveData : ScriptableObject
{
    public string waveName = "Wave 1";
    public EnemySpawnInfo[] enemies;
    public float timeBetweenGroups = 1f;
}