using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Shooting")] [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 20;

    // State
    private float shootTimer;
    private Transform target;

    private Animator animator;
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
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

        // Подписка на смерть
        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDeath.AddListener(OnDied);
    }

    private void OnDied()
    {
        Debug.Log("Archer destroyed!");
        Destroy(gameObject);
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

        if (MageThreatMonitor.Instance != null && !MageThreatMonitor.Instance.CanShootMage)
            return; // ждём, снарядов уже много

        // Только триггерим анимацию
        // Снаряд заспавнится через Animation Event на нужном кадре
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
            projectile.SetParryEffect(Projectile.ParryEffectType.DeathMark);
            projectile.SetOriginalOwner(gameObject);

            // Регистрируем снаряд ЗДЕСЬ (когда он создан)
            MageThreatMonitor.Instance?.RegisterIncoming(projectile);
        }
    }
}
    