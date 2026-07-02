using UnityEngine;

public class FrostMark : MonoBehaviour
{
    [SerializeField] private float freezeRadius = 5f;
    [SerializeField] private float freezeDuration = 3f;
    
    private GameObject markVisual;
    private bool isMarked = false;
    
    public bool IsMarked => isMarked;
    
    public void ApplyMark()
    {
        if (isMarked) return;
        
        isMarked = true;
        SpawnVisual();
        
        // Подписываемся на смерть
        Health health = GetComponent<Health>();
        if (health != null)
            health.OnDeath.AddListener(OnDeath);
        
        Debug.Log(gameObject.name + " marked with FROST!");
    }
    
    private void OnDeath()
    {
        Debug.Log(gameObject.name + " died with frost mark — FREEZING AREA!");
        FreezeArea();
    }
    
    private void FreezeArea()
    {
        // Визуал зоны заморозки
        SpawnFreezeVisual(transform.position, freezeRadius, freezeDuration);
        
        // Замораживаем всех врагов в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, freezeRadius);
        
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (hit.gameObject == gameObject) continue;
            
            Slowable slowable = hit.GetComponent<Slowable>();
            if (slowable == null)
                slowable = hit.gameObject.AddComponent<Slowable>();
            
            slowable.Freeze(freezeDuration);
            Debug.Log("Frozen: " + hit.name);
        }
    }
    
    private void SpawnVisual()
    {
        if (markVisual != null) return;
        
        markVisual = new GameObject("FrostMarkIcon");
        markVisual.transform.SetParent(transform);
        markVisual.transform.localPosition = Vector3.up * 0.8f;
        
        SpriteRenderer sr = markVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateMarkSprite();
        sr.color = new Color(0.5f, 0.9f, 1f, 1f);
        sr.sortingOrder = 50;
        
        markVisual.AddComponent<MarkPulse>();
    }
    
    private void SpawnFreezeVisual(Vector2 pos, float radius, float duration)
    {
        GameObject visual = new GameObject("FrostExplosion");
        visual.transform.position = pos;
        
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.5f, 0.9f, 1f, 0.6f);
        sr.sortingOrder = 90;
        
        visual.transform.localScale = Vector3.one * (radius * 2f);
        
        FrostZoneAnimation anim = visual.AddComponent<FrostZoneAnimation>();
        anim.duration = duration;
    }
    
    private Sprite CreateMarkSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;
        
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = dist < radius ? 1f : 0f;
                if (dist > radius - 4f && dist < radius)
                    alpha = (radius - dist) / 4f;
                
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}