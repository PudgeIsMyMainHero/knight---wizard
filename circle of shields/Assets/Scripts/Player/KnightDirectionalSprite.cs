using UnityEngine;

public class KnightDirectionalSprite : MonoBehaviour
{
    [Header("Sprites — Shield Down (all facing LEFT for side/diag)")]
    [SerializeField] private Sprite upSD;
    [SerializeField] private Sprite downSD;
    [SerializeField] private Sprite sideSD;       // смотрит влево
    [SerializeField] private Sprite diagUpSD;     // смотрит влево-вверх (NW)
    [SerializeField] private Sprite diagDownSD;   // смотрит влево-вниз (SW)
    
    [Header("Sprites — Shield Up (all facing LEFT for side/diag)")]
    [SerializeField] private Sprite upSU;
    [SerializeField] private Sprite downSU;
    [SerializeField] private Sprite sideSU;
    [SerializeField] private Sprite diagUpSU;
    [SerializeField] private Sprite diagDownSU;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private ShieldController shieldController;
    
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    
    [SerializeField] private float minMovementThreshold = 0.1f;
    
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    
    private Camera cam;
    
    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (shieldController == null)
            shieldController = GetComponent<ShieldController>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        cam = Camera.main;
    }
    
    private void LateUpdate()
    {
        UpdateSprite();
    }
    
    private void UpdateSprite()
    {
        if (targetRenderer == null) return;
        
        float angle = GetAimAngle();
        bool shieldUp = IsShieldRaised();
        bool isMoving = IsMoving();
        
        bool shouldPlayWalkAnim = isMoving && !shieldUp;
        
        if (animator != null)
            animator.SetBool(IsMovingHash, shouldPlayWalkAnim);
        
        Sprite selectedSprite;
        bool flipByCursor;
        SelectSprite(angle, shieldUp, out selectedSprite, out flipByCursor);
        
        if (shouldPlayWalkAnim)
        {
            // Флип по направлению движения (не курсора)
            bool flipByMovement = GetFlipByMovement();
            targetRenderer.flipX = flipByMovement;
        }
        else
        {
            if (selectedSprite != null)
                targetRenderer.sprite = selectedSprite;
            targetRenderer.flipX = flipByCursor;
        }
    }
    
    private bool GetFlipByMovement()
    {
        if (rb == null) return false;
        
        Vector2 vel = rb.linearVelocity;
        
        // Если движение по X больше — флип по X
        if (Mathf.Abs(vel.x) > 0.1f)
            return vel.x > 0;   // движется вправо → флип
        
        // Если только по Y — флип по курсору (сохраняем предыдущий)
        return targetRenderer.flipX;
    }
    
    private bool IsMoving()
    {
        if (rb == null) return false;
        return rb.linearVelocity.magnitude > minMovementThreshold;
    }

    
    private float GetAimAngle()
    {
        if (cam == null || UnityEngine.InputSystem.Mouse.current == null)
            return 90f;
        
        Vector2 mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector2 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane)
        );
        
        Vector2 direction = (mouseWorld - (Vector2)transform.position).normalized;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    
    private bool IsShieldRaised()
    {
        if (shieldController == null) return false;
        return shieldController.IsBlocking || shieldController.IsParryActive;
    }
    
    private void SelectSprite(float angle, bool shieldUp, out Sprite sprite, out bool flip)
    {
        // Нормализуем угол в 0-360
        angle = ((angle % 360f) + 360f) % 360f;
        
        // 0°=E, 90°=N, 180°=W, 270°=S
        
        if (angle >= 337.5f || angle < 22.5f)          // E (восток)
        {
            sprite = shieldUp ? sideSU : sideSD;
            flip = true;   // side смотрит влево → флип
        }
        else if (angle < 67.5f)                          // NE (диагональ вверх-вправо)
        {
            sprite = shieldUp ? diagUpSU : diagUpSD;
            flip = false;  // diagUp уже смотрит вправо
        }
        else if (angle < 112.5f)                         // N (вверх)
        {
            sprite = shieldUp ? upSU : upSD;
            flip = false;
        }
        else if (angle < 157.5f)                         // NW (диагональ вверх-влево)
        {
            sprite = shieldUp ? diagUpSU : diagUpSD;
            flip = true;   // diagUp смотрит вправо, флипаем для влево
        }
        else if (angle < 202.5f)                         // W (запад)
        {
            sprite = shieldUp ? sideSU : sideSD;
            flip = false;  // side уже смотрит влево
        }
        else if (angle < 247.5f)                         // SW (диагональ вниз-влево)
        {
            sprite = shieldUp ? diagDownSU : diagDownSD;
            flip = false;  // diagDown уже смотрит влево
        }
        else if (angle < 292.5f)                         // S (вниз)
        {
            sprite = shieldUp ? downSU : downSD;
            flip = false;
        }
        else                                              // SE (диагональ вниз-вправо)
        {
            sprite = shieldUp ? diagDownSU : diagDownSD;
            flip = true;   // diagDown смотрит влево, флипаем для вправо
        }
    }
}