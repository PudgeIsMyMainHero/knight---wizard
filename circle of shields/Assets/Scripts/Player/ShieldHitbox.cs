using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ShieldHitbox : MonoBehaviour
{
    [Header("Aim Assist")]
    [SerializeField] private float aimAssistRadius = 3f;
    [SerializeField] private float aimAssistStrength = 0.4f;
    [SerializeField] private bool showDebug = true;
    
    private ShieldController shieldController;
    
    private void Awake()
    {
        shieldController = GetComponent<ShieldController>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (shieldController == null) return;
        
        Projectile proj = other.GetComponent<Projectile>();
        if (proj != null && proj.Owner == Projectile.ProjectileOwner.Enemy && !proj.IsReflected)
        {
            if (shieldController.IsParryActive)
            {
                Vector2 reflectDir = GetAssistedDirection();
                proj.Reflect(reflectDir);
        
                ParryEffect.SpawnFlash(other.transform.position);
                Debug.Log(">>> SHIELD PARRY! <<<");
        
                // Уведомляем волшебницу о парри
                MageGirl mage = FindObjectOfType<MageGirl>();
                if (mage != null)
                    mage.OnParryHappened();
                if (PerfectDuetManager.Instance != null)
                    PerfectDuetManager.Instance.OnParry();
            }
            else if (shieldController.IsBlocking)
            {
                Destroy(other.gameObject);
                Debug.Log("Shield blocked!");
            }
            return;
        }
        
        EnemySlime slime = other.GetComponent<EnemySlime>();
        if (slime != null && slime.CurrentState == EnemySlime.SlimeState.Dashing)
        {
            if (shieldController.IsParryActive)
            {
                slime.OnShieldParry(transform.parent);
                Debug.Log(">>> SHIELD PARRY SLIME! <<<");
            }
            else if (shieldController.IsBlocking)
            {
                slime.OnShieldBlock();
                Debug.Log("Shield blocked slime!");
            }
            return;
        }
    }
    
    private Vector2 GetAssistedDirection()
    {
        Vector2 knightPos = transform.parent.position;
        Vector2 cursorDir = GetCursorDirection();
        Vector2 cursorWorldPos = GetCursorWorldPosition();
        
        // Ищем ближайшего врага к линии курсора
        GameObject bestTarget = null;
        float bestScore = float.MaxValue;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            
            Vector2 enemyPos = enemy.transform.position;
            Vector2 toEnemy = enemyPos - knightPos;
            
            // Расстояние от врага до линии полёта (курсор)
            float distanceToLine = DistanceToRay(knightPos, cursorDir, enemyPos);
            
            // Враг слишком далеко от траектории — пропускаем
            if (distanceToLine > aimAssistRadius) continue;
            
            // Враг позади рыцаря — пропускаем
            float dotProduct = Vector2.Dot(cursorDir, toEnemy.normalized);
            if (dotProduct < 0.3f) continue;
            
            // Счёт: чем ближе к траектории — тем лучше
            float score = distanceToLine;
            
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }
        
        // Нет подходящего врага — летим строго к курсору
        if (bestTarget == null)
        {
            if (showDebug) Debug.Log("Aim assist: no target, pure cursor");
            return cursorDir;
        }
        
        // Смешиваем направление к курсору и к врагу
        Vector2 toTarget = ((Vector2)bestTarget.transform.position - knightPos).normalized;
        Vector2 assistedDir = Vector2.Lerp(cursorDir, toTarget, aimAssistStrength).normalized;
        
        if (showDebug)
        {
            Debug.Log("Aim assist: targeting " + bestTarget.name + 
                      " (distance to line: " + bestScore.ToString("F1") + ")");
            Debug.DrawLine(knightPos, knightPos + cursorDir * 10f, Color.yellow, 1f);
            Debug.DrawLine(knightPos, knightPos + assistedDir * 10f, Color.green, 1f);
            Debug.DrawLine(knightPos, bestTarget.transform.position, Color.red, 1f);
        }
        
        return assistedDir;
    }
    
    // Расстояние от точки до луча
    private float DistanceToRay(Vector2 rayOrigin, Vector2 rayDir, Vector2 point)
    {
        Vector2 toPoint = point - rayOrigin;
        float projection = Vector2.Dot(toPoint, rayDir);
        Vector2 closestPoint = rayOrigin + rayDir * projection;
        return Vector2.Distance(point, closestPoint);
    }
    
    private Vector2 GetCursorDirection()
    {
        Vector2 knightPos = transform.parent.position;
        Vector2 cursorPos = GetCursorWorldPosition();
        return (cursorPos - knightPos).normalized;
    }
    
    private Vector2 GetCursorWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null || Mouse.current == null)
            return (Vector2)transform.parent.position + (Vector2)transform.parent.up;
        
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        return cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane)
        );
    }
}