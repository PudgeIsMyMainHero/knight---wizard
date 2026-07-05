using UnityEngine;

/// Универсальный компонент для смены спрайта врага по направлению движения.
/// Поддерживает 3 базовых направления (up, down, side) + флип для right.
public class EnemyDirectionalSprite : MonoBehaviour
{
    public enum DirectionSource { Velocity, TargetDirection }
    
    [Header("Sprites (side смотрит ВЛЕВО)")]
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteSide;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Direction Source")]
    [SerializeField] private DirectionSource source = DirectionSource.Velocity;
    
    [Tooltip("Опционально: если направление берётся через таргет (не Velocity)")]
    [SerializeField] private Transform customTarget;
    
    [Header("Settings")]
    [Tooltip("Минимальная скорость для смены направления")]
    [SerializeField] private float minMovementThreshold = 0.1f;
    
    [Tooltip("Использовать target по тегу если customTarget не задан")]
    [SerializeField] private string targetTag = "Mage";
    
    private enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;
    private Transform cachedTarget;
    
    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        // Находим таргет по тегу если нужен
        if (source == DirectionSource.TargetDirection && customTarget == null)
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
                cachedTarget = targetObj.transform;
        }
        else
        {
            cachedTarget = customTarget;
        }
    }
    
    private void LateUpdate()
    {
        if (targetRenderer == null) return;
        
        UpdateDirection();
        ApplySprite();
    }
    
    private void UpdateDirection()
    {
        Vector2 dir = GetDirectionVector();
        
        // Если движения нет — не меняем направление
        if (dir.magnitude < minMovementThreshold)
            return;
        
        // Определяем главную ось
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            currentDirection = dir.x < 0 ? Direction.Left : Direction.Right;
        }
        else
        {
            currentDirection = dir.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    
    private Vector2 GetDirectionVector()
    {
        if (source == DirectionSource.Velocity)
        {
            return rb != null ? rb.linearVelocity : Vector2.zero;
        }
        else // TargetDirection
        {
            if (cachedTarget == null) return Vector2.zero;
            return ((Vector2)cachedTarget.position - (Vector2)transform.position).normalized;
        }
    }
    
    private void ApplySprite()
    {
        switch (currentDirection)
        {
            case Direction.Up:
                targetRenderer.sprite = spriteUp;
                targetRenderer.flipX = false;
                break;
                
            case Direction.Down:
                targetRenderer.sprite = spriteDown;
                targetRenderer.flipX = false;
                break;
                
            case Direction.Left:
                targetRenderer.sprite = spriteSide;
                targetRenderer.flipX = false;
                break;
                
            case Direction.Right:
                targetRenderer.sprite = spriteSide;
                targetRenderer.flipX = true;
                break;
        }
    }
}