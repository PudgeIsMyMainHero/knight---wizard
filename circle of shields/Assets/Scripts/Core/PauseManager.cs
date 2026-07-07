using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    
    private bool isPaused = false;
    
    public bool IsPaused => isPaused;
    
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
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        Time.timeScale = 1f;
    }
    
    private void Update()
    {
        // ESC — переключение паузы
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }
    
    public void Pause()
    {
        // Не ставим паузу если Game Over активен
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;
        
        // Не ставим паузу если открыт выбор апгрейда
        // (там уже своя пауза через Time.timeScale = 0)
        
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        Debug.Log("Game paused");
    }
    
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        Debug.Log("Game resumed");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}