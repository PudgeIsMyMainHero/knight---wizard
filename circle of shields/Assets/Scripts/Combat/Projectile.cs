using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    public enum ProjectileOwner { Enemy, Player }
    
    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private ProjectileOwner owner = ProjectileOwner.Enemy;
    
    // State
    private Vector2 direction;
    private bool isReflected = false;
    private Rigidbody2D rb;
    
    // Properties
    public ProjectileOwner Owner => owner;
    public bool IsReflected => isReflected;
    
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
    
    public void SetOwner(ProjectileOwner newOwner)
    {
        owner = newOwner;
    }
    
    // Отражение к курсору (без урона, только для эффектов)
    public void Reflect(Vector2 newDirection)
    {
        isReflected = true;
        direction = newDirection.normalized;
        
        rb.linearVelocity = direction * speed * 1.5f;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Визуал — яркий голубой (эффект-снаряд)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.3f, 1f, 1f, 1f);
    }
    
        private void OnTriggerEnter2D(Collider2D other)
    {
        // Снаряд девочки → урон врагам
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
        
        // Отражённый снаряд → попадает во врага БЕЗ урона
        if (isReflected)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("Reflected hit: " + other.name + " (effect trigger!)");
                Destroy(gameObject);
            }
            return;
        }
        
        // Снаряд врага
        if (owner == ProjectileOwner.Enemy)
        {
            if (other.CompareTag("Player"))
            {
                ShieldController shield = other.GetComponentInChildren<ShieldController>();
                
                if (shield != null && shield.IsInBlockCone(direction))
                {
                    if (shield.IsParryActive)
                    {
                        Vector2 cursorDir = GetCursorDirection(other.transform.position);
                        Reflect(cursorDir);
                        
                        ParryEffect.SpawnFlash(transform.position);
                        
                        Debug.Log(">>> PARRY! Redirected to cursor! <<<");
                        return;
                    }
                    else if (shield.IsBlocking)
                    {
                        Debug.Log("Blocked!");
                        Destroy(gameObject);
                        return;
                    }
                }
                
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
    
    private Vector2 GetCursorDirection(Vector2 fromPosition)
    {
        Camera cam = Camera.main;
        if (cam == null || Mouse.current == null)
            return -direction; // Fallback: обратно
        
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane)
        );
        
        Vector2 cursorDirection = (mouseWorld - fromPosition).normalized;
        return cursorDirection;
    }
}