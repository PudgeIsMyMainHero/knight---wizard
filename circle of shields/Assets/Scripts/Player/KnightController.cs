using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float shieldMoveSpeedMultiplier = 0.5f;
    
    [Header("References")]
    [SerializeField] private ShieldController shieldController;
    
    // Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Input
    private Vector2 moveInput;
    private Vector2 mouseWorldPosition;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");
    }
    
    private void Update()
    {
        if (Mouse.current != null && mainCamera != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane)
            );
        }
        
        RotateTowardsMouse();
    }
    
    private void FixedUpdate()
    {
        float speedMult = GetSpeedMultiplier();
        
        // Замедление при поднятом щите
        if (shieldController != null && (shieldController.IsBlocking || shieldController.IsParryActive))
            speedMult *= shieldMoveSpeedMultiplier;
        
        rb.linearVelocity = moveInput * moveSpeed * speedMult;
    }
    
    private void RotateTowardsMouse()
    {
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
    
    private float GetSpeedMultiplier()
    {
        Slowable slowable = GetComponent<Slowable>();
        return slowable != null ? slowable.CurrentSpeedMultiplier : 1f;
    }
    
    // === INPUT CALLBACKS ===
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }
    
}