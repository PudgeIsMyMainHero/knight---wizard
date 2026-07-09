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
    
    private Animator animator;
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        animator = GetComponent<Animator>();
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
        
        Health health = GetComponent<Health>();
        if (health != null && health.IsDead)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Shoot()
    {
        if (projectilePrefab == null || target == null) return;
        
        // Только триггерим анимацию
        // Снаряд заспавнится через Animation Event
        if (animator != null)
            animator.SetTrigger(AttackTriggerHash);
    }
    
    // ВЫЗЫВАЕТСЯ ИЗ ANIMATION EVENT
    public void SpawnProjectileEvent()
    {
        if (target == null) return;
        if (projectilePrefab == null) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed, projectileDamage);
            projectile.SetParryEffect(Projectile.ParryEffectType.Frost);
            projectile.SetOriginalOwner(gameObject);
        }
    }
    
    private void OnDied()
    {
        Debug.Log("Frost Archer destroyed!");
        Destroy(gameObject);
    }
}