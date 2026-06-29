using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 20;
    
    [Header("Target")]
    [SerializeField] private Transform target;
    
    // State
    private float shootTimer;
    
    private void Start()
    {
        shootTimer = shootInterval;
        
        // Если нет цели — ищем игрока
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }
    
    private void Update()
    {
        if (target == null) return;
        
        // Таймер стрельбы
        shootTimer -= Time.deltaTime;
        
        if (shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootInterval;
        }
        
        // Поворачиваемся к цели
        Vector2 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
    
    private void Shoot()
    {
        if (projectilePrefab == null || target == null) return;
        
        // Направление к цели
        Vector2 direction = (target.position - transform.position).normalized;
        
        // Создаём снаряд
        GameObject proj = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );
        
        // Инициализируем снаряд
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(direction, projectileSpeed, projectileDamage);
        
        Debug.Log("Enemy shoots!");
    }
}