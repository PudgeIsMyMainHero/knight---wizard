using UnityEngine;

public class ParryEffect : MonoBehaviour
{
    public static GameObject ParticlePrefab;
    
    public static void SpawnFlash(Vector2 position)
    {
        SpawnParticles(position);
    }
    
    public static void SpawnParticles(Vector2 position)
    {
        if (ParticlePrefab == null)
        {
            Debug.LogWarning("ParryEffect.ParticlePrefab not assigned!");
            return;
        }
        
        GameObject particles = Instantiate(ParticlePrefab, position, Quaternion.identity);
        
        // Уничтожаем через 3 сек (подстраховка)
        Destroy(particles, 3f);
    }
}