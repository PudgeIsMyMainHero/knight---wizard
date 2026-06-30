using UnityEngine;

public class EnemyFrostArcher: MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2.5f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private int projectileDamage = 15;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // State
    private float shootTimer;
    private Transform target;
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        shootTimer = shootInterval;
        
        GameObject mage = GameObject.FindGameObjectWithTag("Mage");
        if (mage != null)
            target = mage.transform;
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDeath.AddListener(OnDied);
    }
    
    private void Update()
    {
        if (target == null) return;
        
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootInterval;
        }
        
        Vector2 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
    
    private void Shoot()
    {
        if (projectilePrefab == null || target == null) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed, projectileDamage);
            projectile.SetParryEffect(Projectile.ParryEffectType.Frost);
        }
    }
    
    private void OnDied()
    {
        Debug.Log("Frost Archer destroyed!");
        Destroy(gameObject);
    }
}