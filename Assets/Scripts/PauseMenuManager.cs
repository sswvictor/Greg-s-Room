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
    private CanvasGroup overlayGroup;

    private void Awake()
    {
        Instance = this;



        // Get CanvasGroup from NewOverlayPanel
        if (pauseMenuRoot != null)
        {
            Transform overlayPanel = pauseMenuRoot.transform.Find("NewOverlayPanel");
            if (overlayPanel != null)
            {
                overlayGroup = overlayPanel.GetComponent<CanvasGroup>();
            }
        }


        // Setup button listeners
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (timelinesButton != null) timelinesButton.onClick.AddListener(OpenTimelines);
        if (saveQuitButton != null) saveQuitButton.onClick.AddListener(SaveAndQuit);

        // Ensure menu starts hidden and non-interactive
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
            SetPauseMenuInteractivity(false);
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

    private void SetPauseMenuInteractivity(bool interactive)
    {
        if (overlayGroup != null)
        {
            overlayGroup.interactable = interactive;
            overlayGroup.blocksRaycasts = interactive;
            overlayGroup.alpha = interactive ? 1f : 0f;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;


        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(isPaused);
            pauseMenuRoot.transform.localScale = Vector3.one;
            pauseMenuRoot.transform.localPosition = Vector3.zero;

            SetPauseMenuInteractivity(isPaused);

            // When paused, move PauseMenuRoot to be the last child of Canvas for proper UI layering
            if (isPaused && pauseMenuRoot.transform.parent != null)
            {
                pauseMenuRoot.transform.SetAsLastSibling();
            }
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
