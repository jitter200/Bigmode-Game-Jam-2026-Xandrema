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

    
    public GameObject youWinPanel;
    public TMP_Text youWinText;

    
    public Key restartKey = Key.R;

    private bool _gameEnded; 

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
        if (waves == null) waves = FindFirstObjectByType<Waves>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (youWinPanel != null) youWinPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (player != null) player.Died += OnPlayerDied;
        if (waves != null) waves.AllWavesCompleted += OnYouWin;
    }

    private void OnDisable()
    {
        if (player != null) player.Died -= OnPlayerDied;
        if (waves != null) waves.AllWavesCompleted -= OnYouWin;
    }

    private void Update()
    {
        if (!_gameEnded) return;

        var kb = Keyboard.current;
        if (kb != null && kb[restartKey].wasPressedThisFrame)
            Restart();
    }

    private void OnPlayerDied()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (waves != null) waves.enabled = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = "GAME OVER\nPress R to Restart";

        Time.timeScale = 0f;
    }

    private void OnYouWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (waves != null) waves.enabled = false;

        if (youWinPanel != null) youWinPanel.SetActive(true);
        if (youWinText != null) youWinText.text = "YOU WIN\nPress R to Restart";

        Time.timeScale = 0f;
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
