using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ShieldController : MonoBehaviour
{
    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.25f;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float shieldCooldown = 0.5f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer shieldSprite;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color blockColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private Color parryColor = new Color(1f, 1f, 0f, 1f);
    [SerializeField] private Color cooldownColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    
    // State
    private bool isBlocking = false;
    private bool isParryActive = false;
    private bool isOnCooldown = false;
    private bool wantsToBlock = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;
    
    // Collider
    private Collider2D shieldCollider;
    
    // Properties
    public bool IsBlocking => isBlocking;
    public bool IsParryActive => isParryActive;
    public bool IsOnCooldown => isOnCooldown;
    
    private void Awake()
    {
        shieldCollider = GetComponent<Collider2D>();
        shieldCollider.enabled = false; // Щит опущен по умолчанию
        
        if (shieldSprite == null)
            shieldSprite = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                if (wantsToBlock)
                    RaiseShield();
                else
                    SetShieldState(false, normalColor);
            }
            return;
        }
        
        if (isParryActive)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
                isParryActive = false;
                if (isBlocking)
                    UpdateShieldColor(blockColor);
            }
        }
        
        if (!isBlocking && !isParryActive && !isOnCooldown)
            SetShieldState(false, normalColor);
    }
    
    // === INPUT ===
    
    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            wantsToBlock = true;
            if (!isOnCooldown)
                RaiseShield();
        }
        else if (context.canceled)
        {
            wantsToBlock = false;
            if (isBlocking || isParryActive)
                LowerShield();
        }
    }
    
    private void RaiseShield()
    {
        isBlocking = true;
        isParryActive = true;
        parryTimer = parryWindow;
        
        SetShieldState(true, parryColor);
        Debug.Log("SHIELD UP! Parry window!");
    }
    
    private void LowerShield()
    {
        isBlocking = false;
        isParryActive = false;
        isOnCooldown = true;
        cooldownTimer = shieldCooldown;
        
        SetShieldState(false, cooldownColor);
    }
    
    private void SetShieldState(bool active, Color color)
    {
        shieldCollider.enabled = active;
        UpdateShieldColor(color);
    }
    
    private void UpdateShieldColor(Color color)
    {
        if (shieldSprite != null)
            shieldSprite.color = color;
    }
    
    public void IncreaseParryWindow(float amount)
    {
        parryWindow += amount;
    }
    
}