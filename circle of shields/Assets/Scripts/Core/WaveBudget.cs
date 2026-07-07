using UnityEngine;

[CreateAssetMenu(fileName = "Wave_", menuName = "CircleOfShields/Wave Budget")]
public class WaveBudgetData : ScriptableObject
{
    [Header("Identity")]
    public string waveName = "Wave 1";
    
    [Header("Budget")]
    [Tooltip("Бюджет очков для закупки врагов")]
    public int budget = 50;
    
    [Header("Timing")]
    [Tooltip("Интервал между спавнами врагов")]
    public float spawnInterval = 1.5f;
    
    [Tooltip("Случайный разброс интервала")]
    public float spawnIntervalVariance = 0.5f;
    
    [Header("Available Enemies")]
    [Tooltip("Пул врагов доступных в этой волне (если пусто — используется глобальный)")]
    public EnemyData[] availableEnemies;
    
    [Header("Golden Enemies")]
    [Tooltip("Сколько золотых врагов гарантированно заспавнить")]
    public int guaranteedGoldenCount = 0;
    
    [Tooltip("Шанс что каждый обычный враг станет золотым (0-1)")]
    [Range(0f, 1f)]
    public float randomGoldenChance = 0f;
    
    [Header("Directional Limits")]
    [Tooltip("Максимум активных точек спавна одновременно")]
    public int maxActiveSpawnPoints = 2;
}