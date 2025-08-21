using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public bool isGamePaused = false;
    public GameObject pauseMenu;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (gameManager.gameOver)
            return;

        isGamePaused = !isGamePaused;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isGamePaused);
            if (SoundManager.instance)
            {
                SoundManager.instance.PlaySFX(0);
                Time.timeScale = isGamePaused ? 0f : 1f;
            }
        }
    }
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
