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
    
    [Header("Animator")]
    [SerializeField] private Animator animator;
    
    [Header("Guardian Bond")]
    private bool guardianBondActive = false;
    private float guardianBondMultiplier = 1f;
    private bool empoweredNextShot = false;
    
    private Transform pendingTarget;
    private int pendingDamage;
    private float pendingSpeed;
    
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    
    // State
    private float shootTimer;
    
    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
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
    
    public void EnableGuardianBond(float multiplier)
    {
        guardianBondActive = true;
        guardianBondMultiplier = Mathf.Max(guardianBondMultiplier, multiplier);
    }
    
    public void OnParryHappened()
    {
        if (guardianBondActive)
        {
            empoweredNextShot = true;
            Debug.Log("Next Mage shot empowered by Guardian Bond!");
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
        if (target == null) return;
        
        if (projectilePrefab == null || target == null) return;
        
        // Применяем Guardian Bond
        int finalDamage = damage;
        if (empoweredNextShot)
        {
            finalDamage = Mathf.RoundToInt(damage * guardianBondMultiplier);
            empoweredNextShot = false;
            Debug.Log("Mage empowered shot! Damage: " + finalDamage);
        }
        
        // Запоминаем параметры для Animation Event
        pendingTarget = target;
        pendingDamage = finalDamage;
        pendingSpeed = speed;
        
        // Триггерим анимацию — снаряд появится через SpawnProjectileEvent
        if (animator != null)
            animator.SetTrigger(AttackTriggerHash);
    }
    
    public void SpawnProjectileEvent()
    {
        if (pendingTarget == null || projectilePrefab == null) return;
        
        Vector2 direction = (pendingTarget.position - transform.position).normalized;
        SpawnProjectile(direction, pendingDamage, pendingSpeed);
    }
    
    private void SpawnProjectile(Vector2 direction, int damage, float speed)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(direction, speed, damage);
    }
    
    // Форсированная атака по цели (вызывается слаймом при парри)
    public void ForcedAttack(Transform target)
    {
        if (target == null || projectilePrefab == null) return;
        
        // Forced attack не использует Guardian Bond
        empoweredNextShot = false;   // сбрасываем без применения
        
        Vector2 direction = (target.position - transform.position).normalized;
        SpawnProjectile(direction, forcedAttackDamage, forcedProjectileSpeed);
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