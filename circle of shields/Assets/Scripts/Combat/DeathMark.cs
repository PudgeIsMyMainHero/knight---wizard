using UnityEngine;

public class DeathMark : MonoBehaviour
{
    public bool IsMarked { get; private set; }
    public float BonusDamageMultiplier { get; private set; } = 1.5f;
    
    private GameObject markVisual;
    
    [Header("Visual")]
    [SerializeField] private Sprite markSpriteFallback;
    
    private static Sprite globalMarkSprite;
    
    public void ApplyMark(float multiplier = 1.5f)
    {
        if (IsMarked) return; // Уже помечен — не дублируем
        
        IsMarked = true;
        BonusDamageMultiplier = multiplier;
        
        SpawnVisual();
        Debug.Log(gameObject.name + " is MARKED FOR DEATH!");
    }
    
    public void ConsumeMark()
    {
        if (!IsMarked) return;
        
        IsMarked = false;
        RemoveVisual();
    }
    
    private void SpawnVisual()
    {
        if (markVisual != null) return;
        
        markVisual = new GameObject("DeathMarkIcon");
        markVisual.transform.SetParent(transform);
        markVisual.transform.localPosition = new Vector3(0, 0.8f, 0);
        markVisual.transform.localScale = Vector3.one * 0.3f;
        
        SpriteRenderer sr = markVisual.AddComponent<SpriteRenderer>();
        
        // Загружаем спрайт если ещё не
        if (globalMarkSprite == null)
            globalMarkSprite = Resources.Load<Sprite>("deathmark");
        
        sr.sprite = globalMarkSprite != null ? globalMarkSprite : CreateMarkSprite();
        sr.color = new Color(1f, 0.1f, 0.1f, 1f);
        sr.sortingOrder = 50;
        
        markVisual.AddComponent<MarkPulse>();
    }
    
    private void RemoveVisual()
    {
        if (markVisual != null)
        {
            Destroy(markVisual);
            markVisual = null;
        }
    }
    
    private void OnDestroy()
    {
        RemoveVisual();
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
}

// Пульсация значка метки
public class MarkPulse : MonoBehaviour
{
    private float t;
    
    private void Update()
    {
        t += Time.deltaTime * 4f;
        float scale = 1f + Mathf.Sin(t) * 0.25f;
        transform.localScale = Vector3.one * scale;
    }
}