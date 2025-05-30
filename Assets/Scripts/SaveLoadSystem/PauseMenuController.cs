using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button quickSaveButton;
    public Button quickLoadButton;

    public Button closeButton;
    public Button lifeButton;
    public GameObject lifeMenuPanel;  

    private BranchOverviewPanel branchPanel; 

    void Start()
    {

        pauseMenuPanel.SetActive(false);
        lifeMenuPanel.SetActive(false);  


        pauseButton.onClick.AddListener(ShowPause);
        resumeButton.onClick.AddListener(HidePause);
        closeButton.onClick.AddListener(HideLifePanel);

        quickSaveButton.onClick.AddListener(() =>
        {
            QuickSaveSystem.Save();
            quickLoadButton.interactable = true;
        });
        quickLoadButton.onClick.AddListener(() =>
        {
            QuickSaveSystem.Load();
            HidePause();
        });

        quickLoadButton.interactable = QuickSaveSystem.HasSave();


        lifeButton.onClick.AddListener(() =>
        {

            lifeMenuPanel.SetActive(true);
            RoomManager.Instance.UpdateRoomHistoryIfNeeded();

            if (branchPanel == null)
            {
                branchPanel = lifeMenuPanel.GetComponent<BranchOverviewPanel>();
                if (branchPanel == null)
                {
                    return;
                }
            }

            branchPanel.RefreshButtons();
        });


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

    public void HideLifePanel()
    {
        lifeMenuPanel.SetActive(false);
    }
    
}
