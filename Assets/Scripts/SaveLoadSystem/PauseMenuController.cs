using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;     // 指向 PauseMenuPanel（在 MainCanvas 内部）
    public Button pauseButton;
    public Button resumeButton;

    public Button quickSaveButton;
    public Button quickLoadButton;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        pauseButton.onClick.AddListener(ShowPause);
        resumeButton.onClick.AddListener(HidePause);
        quickSaveButton.onClick.AddListener(() => {
            QuickSaveSystem.Save();
            quickLoadButton.interactable = true;
        });
        quickLoadButton.onClick.AddListener(() => {
            QuickSaveSystem.Load();
            HidePause();
        });

        quickLoadButton.interactable = QuickSaveSystem.HasSave();
    }


    void ShowPause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        GamePauseState.IsPaused = true;
    }

    void HidePause()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        GamePauseState.IsPaused = false;
    }

}
