using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldController : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private float blockAngle = 90f;
    
    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.25f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer shieldSprite;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color blockColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private Color parryColor = new Color(1f, 1f, 0f, 1f);
    
    // State
    private bool isBlocking = false;
    private bool isParryActive = false;
    private float parryTimer = 0f;
    private float blockStartTime = 0f;
    
    // Properties (другие скрипты могут читать)
    public bool IsBlocking => isBlocking;
    public bool IsParryActive => isParryActive;
    public float BlockAngle => blockAngle;
    
    private void Update()
    {
        // Обновляем парри таймер
        if (isParryActive)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
                isParryActive = false;
                
                // После парри остаёмся в блоке если кнопка зажата
                if (isBlocking)
                    UpdateShieldColor(blockColor);
            }
        }
        
        // Обновляем цвет щита
        if (!isBlocking && !isParryActive)
            UpdateShieldColor(normalColor);
    }
    
    // === INPUT CALLBACK ===
    
    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Кнопка нажата — начинаем блок + парри окно
            isBlocking = true;
            isParryActive = true;
            parryTimer = parryWindow;
            blockStartTime = Time.time;
            
            UpdateShieldColor(parryColor);
            Debug.Log("PARRY WINDOW OPEN!");
        }
        else if (context.canceled)
        {
            // Кнопка отпущена — заканчиваем блок
            isBlocking = false;
            isParryActive = false;
            
            UpdateShieldColor(normalColor);
        }
    }
    
    // Проверка: попал ли снаряд в зону блока
    public bool IsInBlockCone(Vector2 projectileDirection)
    {
        if (!isBlocking && !isParryActive) return false;
        
        // Направление "вперёд" щита (куда смотрит рыцарь)
        Vector2 shieldForward = transform.parent.up;
        
        // Угол между направлением снаряда и щитом
        float angle = Vector2.Angle(-projectileDirection, shieldForward);
        
        return angle <= blockAngle / 2f;
    }
    
    private void UpdateShieldColor(Color color)
    {
        if (shieldSprite != null)
            shieldSprite.color = color;
    }
}