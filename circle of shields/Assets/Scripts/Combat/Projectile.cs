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
        
        // Отражённый снаряд → врагам БЕЗ урона
        if (isReflected)
        {
            if (other.CompareTag("Enemy"))
            {
                // Применяем эффект парри в зависимости от типа
                ApplyParryEffect(other.gameObject, transform.position);
                Destroy(gameObject);
            }
            return;
        }
        
        // Вражеский снаряд → попадает в щит? Нет — ShieldHitbox ловит раньше
        // Если долетел сюда — значит мимо щита
        if (owner == ProjectileOwner.Enemy)
        {
            if (other.CompareTag("Player"))
            {
                Health playerHealth = other.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage, "Enemy Projectile");
                
                Destroy(gameObject);
            }
            else if (other.CompareTag("Mage"))
            {
                Health mageHealth = other.GetComponent<Health>();
                if (mageHealth != null)
                    mageHealth.TakeDamage(damage, "Enemy Projectile");
                
                Destroy(gameObject);
            }
        }
    }
    
    private void ApplyParryEffect(GameObject target, Vector2 hitPosition)
    {
        switch (parryEffect)
        {
            case ParryEffectType.DeathMark:
                DeathMarkEffect.Trigger(target, hitPosition);
                break;
            
            case ParryEffectType.None:
            default:
                Debug.Log("Reflected hit: " + target.name + " (no effect)");
                break;
        }
    }
    
}