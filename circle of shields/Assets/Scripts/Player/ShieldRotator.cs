using UnityEngine;
using UnityEngine.InputSystem;

/// Поворачивает объект (ShieldPivot) к курсору на 360°
public class ShieldRotator : MonoBehaviour
{
    private Camera cam;
    
    private void Awake()
    {
        cam = Camera.main;
    }
    
    private void LateUpdate()
    {
        if (cam == null || Mouse.current == null) return;
        
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane)
        );
        
        Vector2 direction = (mouseWorld - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Поворачиваем так, чтобы "вверх" объекта (Y+) смотрел к курсору
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
}