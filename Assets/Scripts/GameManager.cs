using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    
    public Player player;
    public Waves waves;

    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public Key restartKey = Key.R;

    private bool _gameOver;

    private void Awake()
    {
        
        if (player == null) player = FindFirstObjectByType<Player>();
        if (waves == null) waves = FindFirstObjectByType<Waves>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

    }

    private void OnEnable()
    {
        if (player != null) player.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (player != null) player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        _gameOver = true;

        
        if (waves != null) waves.enabled = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = "GAME OVER";

        Debug.Log("GAME OVER. Press R to restart.");

    }

    private void Update()
    {
        if (!_gameOver) return;

        var kb = Keyboard.current;
        if (kb != null && kb[restartKey].wasPressedThisFrame)
        {
            Restart();
        }
    }

    private void Restart()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
