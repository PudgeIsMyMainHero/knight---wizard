using UnityEngine;

/// Вычисляет текущий "уровень угрозы" на арене
/// Используется для умного контроля спавна
public static class ThreatCalculator
{
    // Веса для разных источников угрозы (настройки)
    private const float PROJECTILE_WEIGHT = 2f;
    private const float FROST_PROJECTILE_WEIGHT = 3f;   // лёд опаснее
    private const float SLIME_DASHING_WEIGHT = 5f;
    private const float SLIME_CHARGING_WEIGHT = 3f;
    private const float ALIVE_ENEMY_WEIGHT = 1f;
    private const float SLOWED_ALLY_WEIGHT = 3f;
    private const float FROZEN_ALLY_WEIGHT = 8f;
    private const float LOW_HP_MAGE_WEIGHT = 10f;
    private const float LOW_HP_KNIGHT_WEIGHT = 5f;
    
    /// Основной метод расчёта
    public static float CalculateCurrentThreat()
    {
        float threat = 0f;
        
        threat += CalculateProjectileThreat();
        threat += CalculateEnemyThreat();
        threat += CalculateAllyStatusThreat();
        
        return threat;
    }
    
    /// Угроза от снарядов
    private static float CalculateProjectileThreat()
    {
        float threat = 0f;
        
        Projectile[] projectiles = Object.FindObjectsOfType<Projectile>();
        foreach (var p in projectiles)
        {
            if (p == null) continue;
            if (p.Owner != Projectile.ProjectileOwner.Enemy) continue;
            if (p.IsReflected) continue;
            
            // Разный вес по типу эффекта
            switch (p.ParryEffect)
            {
                case Projectile.ParryEffectType.Frost:
                    threat += FROST_PROJECTILE_WEIGHT;
                    break;
                default:
                    threat += PROJECTILE_WEIGHT;
                    break;
            }
        }
        
        return threat;
    }
    
    /// Угроза от врагов на арене
    private static float CalculateEnemyThreat()
    {
        float threat = 0f;
        
        // Слаймы в опасных состояниях
        EnemySlime[] slimes = Object.FindObjectsOfType<EnemySlime>();
        foreach (var s in slimes)
        {
            if (s == null) continue;
            
            switch (s.CurrentState)
            {
                case EnemySlime.SlimeState.Dashing:
                case EnemySlime.SlimeState.Flying:
                    threat += SLIME_DASHING_WEIGHT;
                    break;
                    
                case EnemySlime.SlimeState.Charging:
                    threat += SLIME_CHARGING_WEIGHT;
                    break;
                    
                case EnemySlime.SlimeState.Walking:
                    threat += ALIVE_ENEMY_WEIGHT;
                    break;
            }
        }
        
        // Обычные враги
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (e == null) continue;
            if (e.GetComponent<EnemySlime>() != null) continue;   // слаймы уже посчитаны
            
            threat += ALIVE_ENEMY_WEIGHT;
        }
        
        return threat;
    }
    
    /// Угроза от статусов союзников
    private static float CalculateAllyStatusThreat()
    {
        float threat = 0f;
        
        threat += CalculateAllyThreat("Player", LOW_HP_KNIGHT_WEIGHT);
        threat += CalculateAllyThreat("Mage", LOW_HP_MAGE_WEIGHT);
        
        return threat;
    }
    
    private static float CalculateAllyThreat(string tag, float lowHpWeight)
    {
        float threat = 0f;
        
        GameObject ally = GameObject.FindGameObjectWithTag(tag);
        if (ally == null) return 0f;
        
        // Слабое HP
        Health health = ally.GetComponent<Health>();
        if (health != null)
        {
            float hpRatio = (float)health.CurrentHealth / health.MaxHealth;
            
            if (hpRatio < 0.3f)
                threat += lowHpWeight;
            else if (hpRatio < 0.5f)
                threat += lowHpWeight * 0.5f;
        }
        
        // Замороженный
        Slowable slowable = ally.GetComponent<Slowable>();
        if (slowable != null)
        {
            if (slowable.IsFrozen)
                threat += FROZEN_ALLY_WEIGHT;
            else if (slowable.CurrentSpeedMultiplier < 0.7f)
                threat += SLOWED_ALLY_WEIGHT;
        }
        
        return threat;
    }
    
    /// Категория угрозы для дебага
    public static string GetThreatCategory(float threat)
    {
        if (threat < 5f) return "SAFE";
        if (threat < 15f) return "MODERATE";
        if (threat < 25f) return "DANGEROUS";
        return "CRITICAL";
    }
}