using UnityEngine;

public class MageGirl : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectileDamage = 15;
    [SerializeField] private float attackRange = 20f;
    
    [Header("Forced Attack")]
    [SerializeField] private float forcedAttackDelay = 0.3f;
    [SerializeField] private int forcedAttackDamage = 50;
    [SerializeField] private float forcedProjectileSpeed = 15f;
    
    [Header("Multi Shot")]
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float burstDelay = 0.15f;
    
    // State
    private float shootTimer;
    
    private void Start()
    {
        shootTimer = shootInterval;
    }
    
    private void Update()
    {
        float speedMult = GetSpeedMultiplier();
        
        shootTimer -= Time.deltaTime * speedMult;
        
        if (shootTimer <= 0)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
                Shoot(target, projectileDamage, projectileSpeed);
            
            shootTimer = shootInterval;
        }
    }
    
    private float GetSpeedMultiplier()
    {
        Slowable slowable = GetComponent<Slowable>();
        return slowable != null ? slowable.CurrentSpeedMultiplier : 1f;
    }

    
    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        Transform closest = null;
        float closestDistance = attackRange;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }
        
        return closest;
    }
    
    private void Shoot(Transform target, int damage, float speed)
    {
        if (projectilePrefab == null) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        
        // Поворот к цели
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        
        // Создаём снаряд
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(direction, speed, damage);
    }
    
    // Форсированная атака по цели (вызывается слаймом при парри)
    public void ForcedAttack(Transform target)
    {
        StartCoroutine(ForcedAttackCoroutine(target));
    }
    
    private System.Collections.IEnumerator ForcedAttackCoroutine(Transform target)
    {
        // Небольшая задержка для драматизма
        yield return new WaitForSeconds(forcedAttackDelay);
        
        if (target != null)
        {
            Debug.Log("Mage FORCED ATTACK on " + target.name + "!");
            Shoot(target, forcedAttackDamage, forcedProjectileSpeed);
        }
    }
    
    public void MultiplyShootInterval(float multiplier)
    {
        shootInterval *= multiplier;
        shootInterval = Mathf.Max(0.2f, shootInterval); // Минимум 0.2 сек
    }
    
    public void AddDamage(int amount)
    {
        projectileDamage += amount;
    }
}