using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    
    [Header("Bounds (мир фона)")]
    [SerializeField] private Vector2 backgroundCenter = Vector2.zero;
    [SerializeField] private Vector2 backgroundSize = new Vector2(20f, 20f);
    
    [Header("Debug")]
    [SerializeField] private bool showBounds = true;
    
    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }
    
    private void LateUpdate()
    {
        if (target == null || cam == null) return;
        
        // Целевая позиция камеры
        Vector3 targetPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );
        
        // Ограничиваем чтобы не показывать край фона
        targetPos = ClampToBounds(targetPos);
        
        // Плавное движение
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
    
    private Vector3 ClampToBounds(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        float minX = backgroundCenter.x - backgroundSize.x / 2f + camWidth;
        float maxX = backgroundCenter.x + backgroundSize.x / 2f - camWidth;
        float minY = backgroundCenter.y - backgroundSize.y / 2f + camHeight;
        float maxY = backgroundCenter.y + backgroundSize.y / 2f - camHeight;
        
        // Если фон меньше камеры — центрируем
        if (minX > maxX)
            position.x = backgroundCenter.x;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);
        
        if (minY > maxY)
            position.y = backgroundCenter.y;
        else
            position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    private void OnDrawGizmos()
    {
        if (!showBounds) return;
        
        // Границы фона
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector3)backgroundCenter, (Vector3)backgroundSize);
        
        // Область движения камеры
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            
            Vector2 clampSize = new Vector2(
                Mathf.Max(0, backgroundSize.x - camWidth * 2),
                Mathf.Max(0, backgroundSize.y - camHeight * 2)
            );
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((Vector3)backgroundCenter, (Vector3)clampSize);
        }
    }
}