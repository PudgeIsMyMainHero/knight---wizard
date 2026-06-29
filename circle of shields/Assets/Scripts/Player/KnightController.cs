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
    
    // Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Input
    private Vector2 moveInput;
    private Vector2 mouseWorldPosition;
    
    // Dash
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        // Проверка наличия камеры
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Make sure your camera has tag 'MainCamera'");
        }
    }
    
    private void Update()
    {
        // Проверка наличия мыши (защита от null)
        if (Mouse.current != null && mainCamera != null)
        {
            // Получаем позицию мыши через НОВЫЙ Input System
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));
            
            // Поворот к курсору
            RotateTowardsMouse();
        }
        
        // Обновляем таймеры
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
            
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                isDashing = false;
        }
    }
    
    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Движение во время рывка
            rb.linearVelocity = dashDirection * (dashDistance / dashDuration);
        }
        else
        {
            // Обычное движение
            Vector2 movement = moveInput * moveSpeed;
            rb.linearVelocity = movement;
        }
    }
    
    private void RotateTowardsMouse()
    {
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Поворачиваем рыцаря
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 чтобы "вперёд" был вверх спрайта
        
        // Поворачиваем щит (он всегда впереди)
        if (shieldTransform != null)
        {
            shieldTransform.localPosition = new Vector3(0, 0.5f, 0); // Впереди рыцаря
        }
    }
    
    // === INPUT SYSTEM CALLBACKS ===
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && dashCooldownTimer <= 0)
        {
            StartDash();
        }
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        // Направление рывка = направление движения или к курсору
        if (moveInput.magnitude > 0.1f)
            dashDirection = moveInput.normalized;
        else
            dashDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;
        
        Debug.Log("Dash!");
    }
}