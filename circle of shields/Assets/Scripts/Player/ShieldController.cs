using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldController : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private float blockAngle = 120f;
    
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
    
    [Header("Debug")]
    [SerializeField] private bool showDebugCone = true;
    
    // State
    private bool isBlocking = false;
    private bool isParryActive = false;
    private bool isOnCooldown = false;
    private bool wantsToBlock = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;
    
    // Properties
    public bool IsBlocking => isBlocking;
    public bool IsParryActive => isParryActive;
    public bool IsOnCooldown => isOnCooldown;
    public float BlockAngle => blockAngle;
    
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
                    UpdateShieldColor(normalColor);
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
            UpdateShieldColor(normalColor);
    }
    
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
        UpdateShieldColor(parryColor);
    }
    
    private void LowerShield()
    {
        isBlocking = false;
        isParryActive = false;
        isOnCooldown = true;
        cooldownTimer = shieldCooldown;
        UpdateShieldColor(cooldownColor);
    }
    
    // Проверка: попал ли снаряд в зону блока
    public bool IsInBlockCone(Vector2 projectileDirection)
    {
        if (!isBlocking && !isParryActive) return false;
        
        // Направление "вперёд" рыцаря (куда смотрит)
        Vector2 knightForward = transform.parent.up;
        
        // Направление ОТКУДА летит снаряд (инвертируем)
        Vector2 incomingDirection = -projectileDirection.normalized;
        
        // Угол между направлением взгляда и откуда летит снаряд
        float angle = Vector2.Angle(knightForward, incomingDirection);
        
        bool isInCone = angle <= blockAngle / 2f;
        
        if (showDebugCone)
        {
            Color debugColor = isInCone ? Color.green : Color.red;
            Debug.DrawRay(transform.parent.position, knightForward * 2f, Color.blue, 0.5f);
            Debug.DrawRay(transform.parent.position, incomingDirection * 2f, debugColor, 0.5f);
            Debug.Log("Shield angle check: " + angle + "° (max: " + blockAngle / 2f + "°) = " + (isInCone ? "BLOCKED" : "MISSED"));
        }
        
        return isInCone;
    }
    
    private void UpdateShieldColor(Color color)
    {
        if (shieldSprite != null)
            shieldSprite.color = color;
    }
    
    // Визуализация конуса в редакторе
    private void OnDrawGizmos()
    {
        if (!showDebugCone) return;
        
        Transform knight = transform.parent;
        if (knight == null) return;
        
        Vector2 forward = knight.up;
        float halfAngle = blockAngle / 2f;
        
        // Цвет конуса
        if (isParryActive)
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        else if (isBlocking)
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        else if (isOnCooldown)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        else
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        
        // Рисуем конус
        float range = 2f;
        Vector2 leftBound = RotateVector(forward, halfAngle);
        Vector2 rightBound = RotateVector(forward, -halfAngle);
        
        Vector3 origin = knight.position;
        Gizmos.DrawLine(origin, origin + (Vector3)(leftBound * range));
        Gizmos.DrawLine(origin, origin + (Vector3)(rightBound * range));
        
        // Дуга
        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float a1 = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
            float a2 = Mathf.Lerp(-halfAngle, halfAngle, (float)(i + 1) / segments);
            
            Vector2 p1 = RotateVector(forward, a1) * range;
            Vector2 p2 = RotateVector(forward, a2) * range;
            
            Gizmos.DrawLine(origin + (Vector3)p1, origin + (Vector3)p2);
        }
    }
    
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}