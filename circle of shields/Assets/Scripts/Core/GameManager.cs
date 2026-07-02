using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Health knightHealth;
    [SerializeField] private Health mageHealth;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Settings")]
    [SerializeField] private float gameOverSlowdown = 0.3f;
    [SerializeField] private float restartDelay = 3f;
    
    private bool isGameOver = false;
    
    public bool IsGameOver => isGameOver;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        Time.timeScale = 1f;
        
        if (knightHealth != null)
            knightHealth.OnDeath.AddListener(OnKnightDied);
        
        if (mageHealth != null)
            mageHealth.OnDeath.AddListener(OnMageDied);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    private void OnKnightDied()
    {
        TriggerGameOver("The Knight has fallen!");
    }
    
    private void OnMageDied()
    {
        TriggerGameOver("The Mage has been slain!");
    }
    
    private void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("GAME OVER: " + reason);
        
        Time.timeScale = gameOverSlowdown;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            GameOverUI ui = gameOverPanel.GetComponent<GameOverUI>();
            if (ui != null)
                ui.SetReason(reason);
        }
        
        // Автоперезапуск через delay
        StartCoroutine(RestartAfterDelay());
    }
    
    private IEnumerator RestartAfterDelay()
    {
        // Используем unscaled time чтобы не зависеть от Time.timeScale
        yield return new WaitForSecondsRealtime(restartDelay);
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}