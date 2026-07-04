using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    public enum ProjectileOwner { Enemy, Player }
    public enum ParryEffectType
    {
        None,
        DeathMark,    // Лучник
        Frost,        // Ледяной стрелок
    }
    
    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private ProjectileOwner owner = ProjectileOwner.Enemy;
    [SerializeField] private ParryEffectType parryEffect = ParryEffectType.None;
    
    // State
    private Vector2 direction;
    private bool isReflected = false;
    private Rigidbody2D rb;
    
    // Properties
    public ProjectileOwner Owner => owner;
    public bool IsReflected => isReflected;
    public ParryEffectType ParryEffect => parryEffect;
    
    private GameObject originalOwner;
    public GameObject OriginalOwner => originalOwner;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector2 shootDirection, float projectileSpeed, int projectileDamage)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        
        rb.linearVelocity = direction * speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public void SetParryEffect(ParryEffectType effect)
    {
        parryEffect = effect;
    }
    
    public void SetOwner(ProjectileOwner newOwner)
    {
        owner = newOwner;
    }
    
    public void Reflect(Vector2 newDirection)
    {
        isReflected = true;
        direction = newDirection.normalized;
        
        rb.linearVelocity = direction * speed * 1.5f;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.3f, 1f, 1f, 1f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Снаряд девочки → врагам
        if (owner == ProjectileOwner.Player && !isReflected)
        {
            if (other.CompareTag("Enemy"))
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage, "Mage Projectile");
                
                Destroy(gameObject);
            }
            return;
        }

        if (isReflected)
        {
            if (other.CompareTag("Enemy"))
            {
                // Проверяем — это ЗОЛОТОЙ враг + попали В НЕГО САМОГО?
                if (other.gameObject == originalOwner)
                {
                    EnemyGolden golden = other.GetComponent<EnemyGolden>();
                    if (golden != null)
                    {
                        golden.OnGoldenHit();
                        Destroy(gameObject);
                        return;
                    }
                }
                
                // Обычный эффект парри
                ApplyParryEffect(other.gameObject, transform.position);
                Destroy(gameObject);
            }
            return;
        }
    }
    
    public void SetOriginalOwner(GameObject owner)
    {
        originalOwner = owner;
    }
    
    private void ApplyFrostToAlly(GameObject ally)
    {
        Slowable slowable = ally.GetComponent<Slowable>();
        if (slowable == null)
            slowable = ally.AddComponent<Slowable>();
        
        // 1-й hit = замедление, 2-й в течение stackWindow = заморозка
        slowable.ApplyStackingFrost(0.5f, 3f);
    }
    
    private void ApplyParryEffect(GameObject target, Vector2 hitPosition)
    {
        switch (parryEffect)
        {
            case ParryEffectType.DeathMark:
                DeathMarkEffect.Trigger(target, hitPosition);
                break;
                
            case ParryEffectType.Frost:
                ApplyFrostParryEffect(target, hitPosition);
                break;
                
            case ParryEffectType.None:
            default:
                Debug.Log("Reflected hit: " + target.name + " (no effect)");
                break;
        }
    }
    
    private void ApplyFrostParryEffect(GameObject target, Vector2 hitPosition)
    {
        // Проверяем — это ЛЕДЯНОЙ СТРЕЛОК?
        EnemyFrostArcher frostArcher = target.GetComponent<EnemyFrostArcher>();
        
        if (frostArcher != null)
        {
            // Ледяной иммунен к своему эффекту — накладываем метку
            FrostMark mark = target.GetComponent<FrostMark>();
            if (mark == null)
                mark = target.AddComponent<FrostMark>();
            
            mark.ApplyMark();
            Debug.Log("FROST MARK applied to frost archer!");
        }
        else
        {
            // Обычный враг — замедляется
            Slowable slowable = target.GetComponent<Slowable>();
            if (slowable == null)
                slowable = target.AddComponent<Slowable>();
            
            slowable.ApplySlow(0.4f, 3f);
            Debug.Log("Frost slow applied to " + target.name);
        }
    }
}