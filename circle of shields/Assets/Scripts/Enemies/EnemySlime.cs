using UnityEngine;

public class EnemySlime : MonoBehaviour
{
    public enum SlimeState { Walking, Charging, Dashing, Flying, WaitingForHit, Dead }
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashTriggerDistance = 5f;
    
    [Header("Dash")]
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private float dashDuration = 0.4f;
    
    [Header("Knockback")]
    [SerializeField] private float flyTime = 0.4f;
    
    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int explosionDamage = 30;
    [SerializeField] private float waitForHitTimeout = 3f;
    
    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 25;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color chargeColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color dashColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private Color stunnedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    // State
    private SlimeState currentState = SlimeState.Walking;
    private Transform target;
    private Rigidbody2D rb;
    private float stateTimer = 0f;
    private Vector2 dashDirection;
    private Vector2 flyTarget;
    private bool hasDealtDamage = false;
    
    public SlimeState CurrentState => currentState;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        GameObject mage = GameObject.FindGameObjectWithTag("Mage");
        if (mage != null)
            target = mage.transform;
            
        UpdateColor();
        
        // Подписка на смерть
        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDeath.AddListener(OnDied);
    }
    
    private void OnDied()
    {
        if (currentState == SlimeState.Dead) return;
        Debug.Log("Slime destroyed by damage!");
        SafeExplode();
    }
    
    private void Update()
    {
        if (currentState == SlimeState.Dead) return;
        
        stateTimer -= Time.deltaTime;
        
        switch (currentState)
        {
            case SlimeState.Walking:
                UpdateWalking();
                break;
            case SlimeState.Charging:
                UpdateCharging();
                break;
            case SlimeState.Dashing:
                UpdateDashing();
                break;
            case SlimeState.Flying:
                UpdateFlying();
                break;
            case SlimeState.WaitingForHit:
                UpdateWaitingForHit();
                break;
        }
    }
    
    // === STATES ===
    
    private void UpdateWalking()
    {
        if (target == null) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * walkSpeed;
        
        
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= dashTriggerDistance)
            StartCharging();
    }
    
    private void StartCharging()
    {
        currentState = SlimeState.Charging;
        stateTimer = chargeTime;
        rb.linearVelocity = Vector2.zero;
        dashDirection = (target.position - transform.position).normalized;
        UpdateColor();
    }
    
    private void UpdateCharging()
    {
        rb.linearVelocity = Vector2.zero;
        
        float pulse = 1f + Mathf.Sin(Time.time * 20f) * 0.1f;
        transform.localScale = Vector3.one * pulse * 0.8f;
        
        if (stateTimer <= 0)
            StartDashing();
    }
    
    private void StartDashing()
    {
        currentState = SlimeState.Dashing;
        stateTimer = dashDuration;
        hasDealtDamage = false;
        rb.linearVelocity = dashDirection * dashSpeed;
        UpdateColor();
    }
    
    private void UpdateDashing()
    {
        rb.linearVelocity = dashDirection * dashSpeed;
        
        if (stateTimer <= 0)
        {
            currentState = SlimeState.Walking;
            transform.localScale = Vector3.one * 0.8f;
            UpdateColor();
        }
    }
    
    private void StartFlying(Vector2 targetPos)
    {
        currentState = SlimeState.Flying;
        flyTarget = targetPos;
        
        float distance = Vector2.Distance(transform.position, flyTarget);
        float speed = Mathf.Max(distance / flyTime, 10f);
        
        Vector2 flyDir = (flyTarget - (Vector2)transform.position).normalized;
        rb.linearDamping = 0f;
        rb.linearVelocity = flyDir * speed;
        
        stateTimer = flyTime + 0.3f; // Запас на случай если не долетит
        
        transform.localScale = Vector3.one * 0.8f;
        UpdateColor();
        
        Debug.Log("Slime flying to cursor!");
    }
    
    private void UpdateFlying()
    {
        float dist = Vector2.Distance(transform.position, flyTarget);
        
        // Долетели или время вышло
        if (dist < 0.5f || stateTimer <= 0)
        {
            StartWaitingForHit();
        }
    }
    
    private void StartWaitingForHit()
    {
        currentState = SlimeState.WaitingForHit;
        stateTimer = waitForHitTimeout;
        
        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = 5f;
        
        // Девочка стреляет
        MageGirl mage = FindObjectOfType<MageGirl>();
        if (mage != null)
            mage.ForcedAttack(transform);
        
        Debug.Log("Slime waiting for mage hit!");
    }
    
    private void UpdateWaitingForHit()
    {
        // Таймаут — взрываемся если снаряд не попал
        if (stateTimer <= 0)
        {
            Debug.Log("Timeout! Exploding anyway.");
            ExplodeWithDamage();
        }
    }
    
    // === СТОЛКНОВЕНИЯ ===
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ждём снаряд девочки
        if (currentState == SlimeState.WaitingForHit)
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj != null && proj.Owner == Projectile.ProjectileOwner.Player)
            {
                Debug.Log("Mage hit! BOOM!");
                Destroy(other.gameObject);
                ExplodeWithDamage();
                return;
            }
        }
        
        // Рывок
        if (currentState == SlimeState.Dashing && !hasDealtDamage)
            CheckDashHit(other);
    }
    
    // === ВЫЗЫВАЕТСЯ ИЗ ShieldHitbox ===
    
    public void OnShieldParry(Transform knight)
    {
        if (currentState != SlimeState.Dashing) return;
        if (hasDealtDamage) return;
        
        Debug.Log("Slime PARRIED by shield!");
        hasDealtDamage = true;
        
        ParryEffect.SpawnFlash(transform.position);
        
        Health health = GetComponent<Health>();
        if (health != null)
            health.SetInvulnerable(true);
        
        Vector2 cursorPos = GetCursorWorldPosition();
        StartFlying(cursorPos);
        // Perfect Duet + Guardian Bond
        if (PerfectDuetManager.Instance != null)
            PerfectDuetManager.Instance.OnParry();
        
        MageGirl mage = FindObjectOfType<MageGirl>();
        if (mage != null)
            mage.OnParryHappened();
    }
    
    public void OnShieldBlock()
    {
        if (currentState != SlimeState.Dashing) return;
        if (hasDealtDamage) return;
        
        Debug.Log("Slime BLOCKED by shield!");
        hasDealtDamage = true;
        
        ParryEffect.SpawnFlash(transform.position);
        SafeExplode();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == SlimeState.Dashing && !hasDealtDamage)
            CheckDashHit(collision.collider);
    }
    
    private void CheckDashHit(Collider2D other)
    {
        // Щит теперь ловит сам через ShieldHitbox
        // Сюда попадаем только если мимо щита
        
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.TakeDamage(contactDamage, "Slime Dash");
            
            hasDealtDamage = true;
            SafeExplode();
        }
        else if (other.CompareTag("Mage"))
        {
            Health mageHealth = other.GetComponent<Health>();
            if (mageHealth != null)
                mageHealth.TakeDamage(contactDamage, "Slime Dash");
            
            hasDealtDamage = true;
            SafeExplode();
        }
    }
    
    // === ВЗРЫВЫ ===
    
    private void SafeExplode()
    {
        currentState = SlimeState.Dead;
        SpawnExplosionVisual(false);
        Destroy(gameObject);
    }
    
    private void ExplodeWithDamage()
    {
        if (currentState == SlimeState.Dead) return;
        currentState = SlimeState.Dead;
        
        Debug.Log("EXPLOSION! Radius: " + explosionRadius);
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != gameObject)
            {
                Health enemyHealth = hit.GetComponent<Health>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(explosionDamage, "Slime Explosion");
            }
        }
        
        SpawnExplosionVisual(true);
        Destroy(gameObject);
    }
    
    // === ВИЗУАЛ ===
    
    private void SpawnExplosionVisual(bool isDamaging)
    {
        GameObject explosion = new GameObject("SlimeExplosion");
        explosion.transform.position = transform.position;
        
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingOrder = 99;
        
        if (isDamaging)
            sr.color = new Color(1f, 0.3f, 0f, 0.8f);
        else
            sr.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        
        explosion.transform.localScale = Vector3.one * 0.5f;
        
        ExplosionAnimation anim = explosion.AddComponent<ExplosionAnimation>();
        anim.maxRadius = isDamaging ? explosionRadius : explosionRadius * 0.5f;
        anim.duration = 0.4f;
    }
    
    private void UpdateColor()
    {
        if (spriteRenderer == null) return;
        
        switch (currentState)
        {
            case SlimeState.Walking:       spriteRenderer.color = normalColor; break;
            case SlimeState.Charging:      spriteRenderer.color = chargeColor; break;
            case SlimeState.Dashing:       spriteRenderer.color = dashColor; break;
            case SlimeState.Flying:        spriteRenderer.color = stunnedColor; break;
            case SlimeState.WaitingForHit: spriteRenderer.color = stunnedColor; break;
        }
    }
    
    private Vector2 GetCursorWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null || UnityEngine.InputSystem.Mouse.current == null)
            return (Vector2)transform.position + Vector2.up * 3f;
        
        Vector2 mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        return cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane)
        );
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashTriggerDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}