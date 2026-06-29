using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;
    
    // State
    private Vector2 direction;
    private bool isReflected = false;
    private int reflectedDamage;
    private Rigidbody2D rb;
    
    // Properties
    public bool IsReflected => isReflected;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        // Уничтожить через lifetime секунд
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector2 shootDirection, float projectileSpeed, int projectileDamage)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        
        rb.linearVelocity = direction * speed;
        
        // Поворачиваем спрайт в направлении полёта
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // Отражение снаряда (при парри)
    public void Reflect(Vector2 newDirection, float damageMultiplier = 2f)
    {
        isReflected = true;
        direction = newDirection.normalized;
        reflectedDamage = Mathf.RoundToInt(damage * damageMultiplier);
        
        rb.linearVelocity = direction * speed * 1.5f;
        
        // Поворот
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Меняем цвет на жёлтый (отражённый)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.yellow;
        
        // Меняем слой чтобы попадал по врагам
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        
        Debug.Log("REFLECTED! Damage: " + reflectedDamage);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isReflected)
        {
            // Отражённый снаряд попадает по врагам
            if (other.CompareTag("Enemy"))
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(reflectedDamage);
                
                Destroy(gameObject);
            }
        }
        else
        {
            // Обычный снаряд попадает по игроку
            if (other.CompareTag("Player"))
            {
                // Проверяем блок/парри
                ShieldController shield = other.GetComponentInChildren<ShieldController>();
                
                if (shield != null && shield.IsInBlockCone(direction))
                {
                    if (shield.IsParryActive)
                    {
                        // ПАРРИ — отражаем снаряд
                        Vector2 reflectDir = -direction;
                        Reflect(reflectDir);
                        Debug.Log(">>> PERFECT PARRY! <<<");
                        return;
                    }
                    else if (shield.IsBlocking)
                    {
                        // БЛОК — снаряд уничтожается
                        Debug.Log("Blocked!");
                        Destroy(gameObject);
                        return;
                    }
                }
                
                // Попадание — урон
                Health playerHealth = other.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);
                
                Destroy(gameObject);
            }
        }
    }
}