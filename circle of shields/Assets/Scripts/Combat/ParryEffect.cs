using UnityEngine;

public class ParryEffect : MonoBehaviour
{
    public static void SpawnFlash(Vector2 position)
    {
        GameObject flash = new GameObject("ParryFlash");
        flash.transform.position = position;
        
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 1f, 0.5f, 1f);
        sr.sortingOrder = 100;
        
        flash.transform.localScale = Vector3.one * 0.5f;
        
        FlashAnimation anim = flash.AddComponent<FlashAnimation>();
        anim.duration = 0.3f;
        anim.maxScale = 2f;
    }
    
    private static Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}

public class FlashAnimation : MonoBehaviour
{
    public float duration = 0.3f;
    public float maxScale = 2f;
    
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
        float progress = 1f - (timer / duration);
        
        // Растёт
        float scale = Mathf.Lerp(0.5f, maxScale, progress);
        transform.localScale = Vector3.one * scale;
        
        // Исчезает
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - progress;
            sr.color = c;
        }
        
        if (timer <= 0)
            Destroy(gameObject);
    }
}