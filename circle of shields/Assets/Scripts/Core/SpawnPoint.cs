using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum EntryAreaMode { Point, Circle, Polygon }
    
    [Header("Entry Area Mode")]
    [SerializeField] private EntryAreaMode areaMode = EntryAreaMode.Circle;
    
    [Header("Point / Circle")]
    [Tooltip("Целевая точка (или центр круга)")]
    [SerializeField] private Transform entryTarget;
    [SerializeField] private Vector2 entryTargetOffset = new Vector2(0, -5);
    
    [Tooltip("Радиус разброса для Circle mode")]
    [SerializeField] private float entrySpreadRadius = 2f;
    
    [Header("Polygon")]
    [Tooltip("PolygonCollider2D задающий форму зоны прибытия")]
    [SerializeField] private PolygonCollider2D entryPolygon;
    
    [Header("Speed")]
    [SerializeField] private float entrySpeed = 3f;
    
    [Header("Identity")]
    [SerializeField] private string pointName = "Spawn Point";
    [SerializeField] private Color gizmoColor = Color.cyan;
    
    public Vector2 SpawnPosition => transform.position;
    public float EntrySpeed => entrySpeed;
    public string PointName => pointName;
    
    // Центр зоны прибытия
    public Vector2 EntryTargetCenter
    {
        get
        {
            if (entryTarget != null)
                return entryTarget.position;
            return (Vector2)transform.position + entryTargetOffset;
        }
    }
    
    // Получить случайную точку прибытия
    public Vector2 GetRandomEntryTarget()
    {
        switch (areaMode)
        {
            case EntryAreaMode.Point:
                return EntryTargetCenter;
                
            case EntryAreaMode.Circle:
                return EntryTargetCenter + Random.insideUnitCircle * entrySpreadRadius;
                
            case EntryAreaMode.Polygon:
                return GetRandomPointInPolygon();
                
            default:
                return EntryTargetCenter;
        }
    }
    
    private Vector2 GetRandomPointInPolygon()
    {
        if (entryPolygon == null)
        {
            Debug.LogWarning(pointName + ": Polygon mode selected but polygon not set!");
            return EntryTargetCenter;
        }
        
        Bounds bounds = entryPolygon.bounds;
        
        // Пытаемся найти точку внутри полигона
        for (int i = 0; i < 50; i++)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            
            if (entryPolygon.OverlapPoint(randomPoint))
                return randomPoint;
        }
        
        Debug.LogWarning(pointName + ": Failed to find point in polygon, using center");
        return entryPolygon.bounds.center;
    }
    
    // === GIZMOS ===
    
    private void OnDrawGizmos()
    {
        // Точка спавна
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.15f);
        
        // Визуализация зоны прибытия
        switch (areaMode)
        {
            case EntryAreaMode.Point:
                DrawPointGizmo();
                break;
            case EntryAreaMode.Circle:
                DrawCircleGizmo();
                break;
            case EntryAreaMode.Polygon:
                DrawPolygonGizmo();
                break;
        }
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, pointName);
        #endif
    }
    
    private void DrawPointGizmo()
    {
        Vector3 target = (Vector3)EntryTargetCenter;
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.7f);
        Gizmos.DrawWireSphere(target, 0.3f);
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(transform.position, target);
        DrawArrow(transform.position, target);
    }
    
    private void DrawCircleGizmo()
    {
        Vector3 target = (Vector3)EntryTargetCenter;
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireSphere(target, entrySpreadRadius);
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.7f);
        Gizmos.DrawWireSphere(target, 0.3f);
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(transform.position, target);
        DrawArrow(transform.position, target);
    }
    
    private void DrawPolygonGizmo()
    {
        if (entryPolygon == null) return;
        
        // Контур полигона
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);
        Vector3 offset = entryPolygon.transform.position;
        
        for (int p = 0; p < entryPolygon.pathCount; p++)
        {
            Vector2[] path = entryPolygon.GetPath(p);
            
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 start = (Vector3)path[i] + offset;
                Vector3 end = (Vector3)path[(i + 1) % path.Length] + offset;
                Gizmos.DrawLine(start, end);
            }
        }
        
        // Линия от точки спавна к центру полигона
        Vector3 polygonCenter = entryPolygon.bounds.center;
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        Gizmos.DrawLine(transform.position, polygonCenter);
        DrawArrow(transform.position, polygonCenter);
    }
    
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        if (direction.magnitude < 0.01f) return;
        
        Vector3 right = Quaternion.Euler(0, 0, 135) * direction * 0.3f;
        Vector3 left = Quaternion.Euler(0, 0, -135) * direction * 0.3f;
        
        Gizmos.DrawLine(to, to + right);
        Gizmos.DrawLine(to, to + left);
    }
}