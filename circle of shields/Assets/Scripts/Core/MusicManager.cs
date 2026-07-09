using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float fadeInDuration = 2f;
    
    private AudioSource musicSource;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Создаём AudioSource для музыки
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;
        musicSource.spatialBlend = 0f;   // 2D
        musicSource.priority = 0;         // высокий приоритет
    }
    
    private void Start()
    {
        if (playOnStart)
            PlayMusic(backgroundMusic);
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        
        musicSource.clip = clip;
        musicSource.Play();
        
        StartCoroutine(FadeIn(fadeInDuration));
    }
    
    public void StopMusic()
    {
        StartCoroutine(FadeOutAndStop(1f));
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (musicSource != null)
            musicSource.volume = volume;
    }
    
    private System.Collections.IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, volume, elapsed / duration);
            yield return null;
        }
        
        musicSource.volume = volume;
    }
    
    private System.Collections.IEnumerator FadeOutAndStop(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        musicSource.Stop();
    }
}