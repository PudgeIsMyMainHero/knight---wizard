using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject prefab;
    public int count = 1;
    public float spawnDelay = 0.5f; // Задержка между спавном каждого врага
}

[CreateAssetMenu(fileName = "Wave", menuName = "CircleOfShields/Wave Data")]
public class WaveData : ScriptableObject
{
    public string waveName = "Wave 1";
    public EnemySpawnInfo[] enemies;
    public float timeBetweenGroups = 1f; // Пауза между разными типами врагов
}