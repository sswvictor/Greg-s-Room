using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;
    public GameObject pauseMenuRoot;
    public Button resumeButton;
    public Button timelinesButton;
    public Button saveQuitButton;

    private bool isPaused = false;
    private PlayerInput playerInput;

    private void Awake()
    {
        Instance = this;

        // Setup button listeners
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (timelinesButton != null) timelinesButton.onClick.AddListener(OpenTimelines);
        if (saveQuitButton != null) saveQuitButton.onClick.AddListener(SaveAndQuit);

        // Ensure menu starts hidden
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }
    }

    private void Start()
    {
        // Get or add PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.defaultActionMap = "UI";
        }
    }

    public void OnCancel(InputValue value)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(isPaused);
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void OpenTimelines()
    {
        Debug.Log("Timelines feature not yet implemented");
    }

    public void SaveAndQuit()
    {
        // TODO: Implement save functionality
        Debug.Log("Saving game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
