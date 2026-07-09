using UnityEngine;

public class Slowable : MonoBehaviour
{
    [Header("Freeze on Second Hit")]
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private float stackWindow = 3f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    private float slowTimer = 0f;
    private float freezeTimer = 0f;
    private float lastHitTime = -999f;
    private bool isFrozen = false;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool colorSaved = false;
    
    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    public bool IsFrozen => isFrozen;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
                Unfreeze();
            return;
        }
        
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
                RemoveSlow();
        }
    }
    
    public void ApplySlow(float multiplier, float duration)
    {
        if (isFrozen) return;
        
        SaveColor();
        
        CurrentSpeedMultiplier = Mathf.Min(CurrentSpeedMultiplier == 1f ? multiplier : CurrentSpeedMultiplier, multiplier);
        slowTimer = Mathf.Max(slowTimer, duration);
        
        UpdateVisual();
        UpdateAnimatorSpeed();
    }
    
    public void ApplyStackingFrost(float slowMultiplier, float slowDuration)
    {
        if (isFrozen) return;
        
        SaveColor();
        
        float timeSinceLastHit = Time.time - lastHitTime;
        
        if (timeSinceLastHit < stackWindow)
        {
            Freeze(freezeDuration);
        }
        else
        {
            ApplySlow(slowMultiplier, slowDuration);
        }
        
        lastHitTime = Time.time;
    }
    
    public void Freeze(float duration)
    {
        SaveColor();
        
        isFrozen = true;
        freezeTimer = duration;
        CurrentSpeedMultiplier = 0f;
        
        UpdateVisual();
        UpdateAnimatorSpeed();
    }
    
    private void Unfreeze()
    {
        isFrozen = false;
        CurrentSpeedMultiplier = 1f;
        slowTimer = 0f;
        
        RestoreColor();
        UpdateAnimatorSpeed();
    }
    
    private void RemoveSlow()
    {
        CurrentSpeedMultiplier = 1f;
        RestoreColor();
        UpdateAnimatorSpeed();
    }
    
    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;
        
        animator.speed = CurrentSpeedMultiplier;
    }
    
    private void SaveColor()
    {
        if (colorSaved) return;
        if (spriteRenderer == null) return;
        
        originalColor = spriteRenderer.color;
        colorSaved = true;
    }
    
    private void RestoreColor()
    {
        if (spriteRenderer != null && colorSaved)
            spriteRenderer.color = originalColor;
    }
    
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        if (isFrozen)
            spriteRenderer.color = new Color(0.6f, 0.9f, 1f, 1f);
        else
            spriteRenderer.color = Color.Lerp(originalColor, new Color(0.5f, 0.8f, 1f, 1f), 0.6f);
    }
}