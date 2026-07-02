using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Dash")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 3f;
    
    [Header("References")]
    [SerializeField] private Transform shieldTransform;
    [SerializeField] private ShieldController shieldController;
    
    // Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Input
    private Vector2 moveInput;
    private Vector2 mouseWorldPosition;
    
    // Rotation lock
    private bool isRotationLocked = false;
    private float lockedAngle = 0f;
    
    // Dash
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");
        
        // Автоматически найти ShieldController если не привязан
        if (shieldController == null && shieldTransform != null)
            shieldController = shieldTransform.GetComponent<ShieldController>();
    }
    
    private void Update()
    {
        if (Mouse.current != null && mainCamera != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane)
            );
            
            // ДЕБАГ — линия от рыцаря к курсору
            Debug.DrawLine(transform.position, mouseWorldPosition, Color.cyan);
        }
        
        UpdateRotationLock();
        
        if (isRotationLocked)
            ApplyLockedRotation();
        else
            RotateTowardsMouse();
        
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
            
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                isDashing = false;
        }
    }
    
    private Vector2 GetMouseWorldOnPlane(Vector2 mouseScreenPos)
    {
        // Луч из камеры через мышь
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
        
        // Плоскость игры — XY на Z=0
        // Нормаль (0, 0, -1) потому что камера смотрит в +Z
        Plane gamePlane = new Plane(Vector3.back, Vector3.zero);
        
        if (gamePlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            return new Vector2(worldPoint.x, worldPoint.y);
        }
        
        return transform.position;
    }
    
    private void FixedUpdate()
    {
        float speedMult = GetSpeedMultiplier();
        
        if (isDashing)
            rb.linearVelocity = dashDirection * (dashDistance / dashDuration) * speedMult;
        else
            rb.linearVelocity = moveInput * moveSpeed * speedMult;
    }
    
    private float GetSpeedMultiplier()
    {
        Slowable slowable = GetComponent<Slowable>();
        return slowable != null ? slowable.CurrentSpeedMultiplier : 1f;
    }
    
    private void UpdateRotationLock()
    {
        if (shieldController == null) return;
        
        bool shouldLock = shieldController.IsBlocking || shieldController.IsParryActive;
        
        if (shouldLock && !isRotationLocked)
        {
            // Только что поднял щит → замораживаем текущий угол
            isRotationLocked = true;
            lockedAngle = transform.eulerAngles.z;
            Debug.Log("Rotation LOCKED");
        }
        else if (!shouldLock && isRotationLocked)
        {
            // Опустил щит → разблокируем
            isRotationLocked = false;
            Debug.Log("Rotation UNLOCKED");
        }
    }
    
    private void RotateTowardsMouse()
    {
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        
        if (shieldTransform != null)
            shieldTransform.localPosition = new Vector3(0, 0.5f, 0);
    }
    
    private void ApplyLockedRotation()
    {
        // Держим замороженный угол
        transform.rotation = Quaternion.Euler(0, 0, lockedAngle);
        
        if (shieldTransform != null)
            shieldTransform.localPosition = new Vector3(0, 0.5f, 0);
    }
    
    // === INPUT CALLBACKS ===
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && dashCooldownTimer <= 0)
            StartDash();
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        if (moveInput.magnitude > 0.1f)
            dashDirection = moveInput.normalized;
        else
            dashDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;
        
        Debug.Log("Dash!");
    }
}