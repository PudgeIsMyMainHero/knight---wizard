using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_", menuName = "CircleOfShields/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public GameObject prefab;
    
    [Header("Cost")]
    [Tooltip("Стоимость в бюджете волны")]
    public int cost = 10;
    
    [Header("Availability")]
    [Tooltip("С какой волны может спавниться")]
    public int minWaveNumber = 1;
    
    [Tooltip("Максимум одновременно на арене (0 = без лимита)")]
    public int maxAtOnce = 0;
    
    [Tooltip("Максимум за волну (0 = без лимита)")]
    public int maxPerWave = 0;
    
    [Header("Spawn Weight")]
    [Tooltip("Вес для случайного выбора (выше = чаще выбирается)")]
    public float spawnWeight = 1f;
    
    [Header("Spawn Constraints")]
    [Tooltip("Задержка между спавнами этого типа")]
    public float spawnCooldown = 0.5f;
    
    [Header("Golden Support")]
    [Tooltip("Может ли этот враг быть золотым")]
    public bool canBeGolden = false;
}