using UnityEngine;

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
    [SerializeField] private Animator animator;
    
    [Header("Direction Source")]
    [SerializeField] private DirectionSource defaultSource = DirectionSource.Velocity;
    [SerializeField] private Transform customTarget;
    [SerializeField] private bool useVelocityDuringEntry = true;
    
    [Header("Settings")]
    [SerializeField] private float minMovementThreshold = 0.1f;
    [SerializeField] private string targetTag = "Mage";
    
    [Header("Sprite Orientation")]
    [SerializeField] private bool sideFacesLeft = true;
    
    private enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;
    private Transform cachedTarget;
    private EnemiesEntry enemyEntry;
    
    private Vector2 lastPosition;
    private bool wasMoving = false;
    
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    
    private void Awake() 
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        enemyEntry = GetComponent<EnemiesEntry>();
        lastPosition = transform.position;
    }
    
    private void Start()
    {
        if (defaultSource == DirectionSource.TargetDirection && customTarget == null)
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
                cachedTarget = targetObj.transform;
        }
        else
        {
            cachedTarget = customTarget;
        }
        
        // Инициализация flip
        InitializeFlip();
    }
    
    private void InitializeFlip()
    {
        Vector2 dir = GetDirectionVector();
        if (Mathf.Abs(dir.x) > 0.1f && targetRenderer != null)
        {
            bool movingRight = dir.x > 0;
            lastFlipState = movingRight ? sideFacesLeft : !sideFacesLeft;
            targetRenderer.flipX = lastFlipState;
        }
    }
    
    private void LateUpdate()
    {
        if (targetRenderer == null) return;
        
        UpdateDirection();
        
        bool isMoving = IsMoving();
        bool isAttacking = IsInAttackAnimation();
        
        if (animator != null)
            animator.SetBool(IsMovingHash, isMoving);
        
        // Меняем спрайт ТОЛЬКО когда стоит И не атакует
        if (!isMoving && !isAttacking)
        {
            ApplyStaticSprite();
        }
        else
        {
            // Во время движения/атаки — только флип
            ApplyFlipForDirection();
        }
    }
    
    private bool IsInAttackAnimation()
    {
        if (animator == null) return false;
        
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        
        if (currentState.IsTag("Attack"))
            return true;
        
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (nextState.IsTag("Attack"))
                return true;
        }
        
        return false;
    }
    
    private bool lastFlipState = false;
    
    private void ApplyFlipForDirection()
    {
        if (rb == null || targetRenderer == null) return;
        
        Vector2 velocity = rb.linearVelocity;
        
        // Обновляем flip при заметном движении по X
        if (Mathf.Abs(velocity.x) > 0.3f)
        {
            bool movingRight = velocity.x > 0;
            lastFlipState = movingRight ? sideFacesLeft : !sideFacesLeft;
        }
        
        // Применяем всегда (сохраняет flip при движении только по Y)
        targetRenderer.flipX = lastFlipState;
    }
    
    private bool IsMoving()
    {
        if (rb == null) return false;
        return rb.linearVelocity.magnitude > minMovementThreshold;
    }
    
    private void UpdateDirection()
    {
        Vector2 dir = GetDirectionVector();
        
        if (dir.magnitude < minMovementThreshold)
            return;
        
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            currentDirection = dir.x < 0 ? Direction.Left : Direction.Right;
        else
            currentDirection = dir.y > 0 ? Direction.Up : Direction.Down;
    }
    
    private Vector2 GetDirectionVector()
    {
        DirectionSource currentSource = defaultSource;
        
        if (useVelocityDuringEntry && enemyEntry != null && enemyEntry.IsEntering)
            currentSource = DirectionSource.Velocity;
        
        if (currentSource == DirectionSource.Velocity)
        {
            return rb != null ? rb.linearVelocity : Vector2.zero;
        }
        else
        {
            if (cachedTarget == null) return Vector2.zero;
            return ((Vector2)cachedTarget.position - (Vector2)transform.position).normalized;
        }
    }
    
    private void ApplyStaticSprite()
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
                targetRenderer.flipX = !sideFacesLeft;
                break;
                
            case Direction.Right:
                targetRenderer.sprite = spriteSide;
                targetRenderer.flipX = sideFacesLeft;
                break;
        }
    }
    
    private void ApplyFlipForWalking()
    {
        // Animator играет анимацию (нарисована для "left")
        // Мы только флипаем для правого движения
        
        switch (currentDirection)
        {
            case Direction.Right:
                targetRenderer.flipX = sideFacesLeft;
                break;
                
            case Direction.Left:
            case Direction.Up:
            case Direction.Down:
                targetRenderer.flipX = !sideFacesLeft;
                break;
        }
    }
}