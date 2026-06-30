using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    public float maxRadius = 3f;
    public float duration = 0.4f;
    
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
        float scale = Mathf.Lerp(0.5f, maxRadius * 2f, progress);
        transform.localScale = Vector3.one * scale;
        
        // Исчезает
        if (sr != null)
        {
            Color c = sr.color;
            c.a = (1f - progress) * 0.8f;
            sr.color = c;
        }
        
        if (timer <= 0)
            Destroy(gameObject);
    }
}