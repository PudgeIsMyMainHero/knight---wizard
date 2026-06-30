using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlowEffect : MonoBehaviour
{
    public static void ApplyAreaSlow(Vector2 center, float radius, float slowMultiplier, float duration)
    {
        Debug.Log("Frost effect! Slowing enemies in radius " + radius);
        
        // Визуал зоны
        SpawnFrostVisual(center, radius, duration);
        
        // Находим всех врагов в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            
            Slowable slowable = hit.GetComponent<Slowable>();
            if (slowable == null)
                slowable = hit.gameObject.AddComponent<Slowable>();
            
            slowable.ApplySlow(slowMultiplier, duration);
            
            Debug.Log("Slowed: " + hit.name);
        }
    }
    
    private static void SpawnFrostVisual(Vector2 center, float radius, float duration)
    {
        GameObject visual = new GameObject("FrostZone");
        visual.transform.position = center;
        
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.3f, 0.7f, 1f, 0.35f);
        sr.sortingOrder = 2;
        
        visual.transform.localScale = Vector3.one * (radius * 2f);
        
        FrostZoneAnimation anim = visual.AddComponent<FrostZoneAnimation>();
        anim.duration = duration;
    }
    
    private static Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = dist < radius ? 1f : 0f;
                
                // Мягкий край
                if (dist > radius - 4f && dist < radius)
                    alpha = (radius - dist) / 4f;
                
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}

// Анимация визуальной зоны льда
public class FrostZoneAnimation : MonoBehaviour
{
    public float duration = 3f;
    private float timer;
    private SpriteRenderer sr;
    
    private void Start()
    {
        timer = duration;
        sr = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (sr != null)
        {
            Color c = sr.color;
            // Затухание в последние 20% времени
            if (timer < duration * 0.2f)
                c.a = Mathf.Lerp(0f, 0.35f, timer / (duration * 0.2f));
            sr.color = c;
        }
        
        if (timer <= 0)
            Destroy(gameObject);
    }
}