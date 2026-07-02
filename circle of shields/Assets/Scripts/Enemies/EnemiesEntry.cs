using UnityEngine;

public class EnemiesEntry : MonoBehaviour
{
    private Vector2 targetPosition;
    private float moveSpeed = 3f;
    private bool isEntering = false;
    
    // Ссылки на компоненты которые надо временно отключить
    private MonoBehaviour[] enemyBehaviours;
    private Rigidbody2D rb;
    
    public bool IsEntering => isEntering;
    
    public void StartEntry(Vector2 target, float speed)
    {
        targetPosition = target;
        moveSpeed = speed;
        isEntering = true;
        
        // Временно отключаем ИИ врага
        DisableEnemyAI();
        
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void DisableEnemyAI()
    {
        // Отключаем все скрипты врагов (EnemyArcher, EnemySlime, EnemyFrostArcher)
        MonoBehaviour[] all = GetComponents<MonoBehaviour>();
        
        System.Collections.Generic.List<MonoBehaviour> toDisable = new System.Collections.Generic.List<MonoBehaviour>();
        
        foreach (var b in all)
        {
            // Отключаем всё что содержит "Enemy" в имени, кроме EnemyEntry
            if (b == this) continue;
            if (b == null) continue;
            
            string typeName = b.GetType().Name;
            if (typeName.StartsWith("Enemy"))
            {
                b.enabled = false;
                toDisable.Add(b);
            }
        }
        
        enemyBehaviours = toDisable.ToArray();
    }
    
    private void EnableEnemyAI()
    {
        if (enemyBehaviours == null) return;
        
        foreach (var b in enemyBehaviours)
            if (b != null) b.enabled = true;
    }
    
    private void Update()
    {
        if (!isEntering) return;
        
        Vector2 currentPos = transform.position;
        float distance = Vector2.Distance(currentPos, targetPosition);
        
        if (distance < 0.3f)
        {
            FinishEntry();
            return;
        }
        
        // Двигаемся к цели
        Vector2 direction = (targetPosition - currentPos).normalized;
        
        if (rb != null)
            rb.linearVelocity = direction * moveSpeed;
        else
            transform.position = Vector2.MoveTowards(currentPos, targetPosition, moveSpeed * Time.deltaTime);
        
        // Поворот в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
    
    private void FinishEntry()
    {
        isEntering = false;
        
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        EnableEnemyAI();
        
        Debug.Log(gameObject.name + " reached arena!");
        
        // Удаляем компонент — больше не нужен
        Destroy(this);
    }
}