using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Sound Library")]
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip blockSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip playerHitSound;
    
    [Header("Settings")]
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private float pitchVariation = 0.1f;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayParry(Vector2 position)
    {
        PlaySoundAtPoint(parrySound, position);
    }
    
    public void PlayBlock(Vector2 position)
    {
        PlaySoundAtPoint(blockSound, position);
    }
    
    public void PlayEnemyHit(Vector2 position)
    {
        PlaySoundAtPoint(enemyHitSound, position);
    }
    
    public void PlayPlayerHit(Vector2 position)
    {
        PlaySoundAtPoint(playerHitSound, position);
    }
    
    private void PlaySoundAtPoint(AudioClip clip, Vector2 position)
    {
        if (clip == null) return;
        
        // Создаём временный AudioSource
        GameObject soundObj = new GameObject("Sound");
        soundObj.transform.position = position;
        
        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = defaultVolume;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.spatialBlend = 0f;   // 0 = 2D звук
        source.Play();
        
        Destroy(soundObj, clip.length + 0.1f);
    }
}