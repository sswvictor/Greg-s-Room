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
    public GameObject lifeMenuPanel;  // 👉 只保留一个引用，面板本体

    private BranchOverviewPanel branchPanel; // 👉 不暴露在 Inspector，自动取

    void Start()
    {
        Debug.Log("[DEBUG] Start() begin.");

        pauseMenuPanel.SetActive(false);
        lifeMenuPanel.SetActive(false);  // ✅ 保证它开局永远是关闭状态

        Debug.Log("[DEBUG] pauseMenuPanel OK.");

        pauseButton.onClick.AddListener(ShowPause);
        resumeButton.onClick.AddListener(HidePause);
        closeButton.onClick.AddListener(HideLifePanel);
        Debug.Log("[DEBUG] Pause/resume buttons OK.");

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
        Debug.Log("[DEBUG] QuickSave/load buttons OK.");

        quickLoadButton.interactable = QuickSaveSystem.HasSave();

        Debug.Log("[DEBUG] lifeButton = " + (lifeButton != null));
        Debug.Log("[DEBUG] lifeMenuPanel = " + (lifeMenuPanel != null));

        lifeButton.onClick.AddListener(() =>
        {
            Debug.Log("[DEBUG] LifeButton clicked. panel=" + lifeMenuPanel);

            lifeMenuPanel.SetActive(true);
            // ✅ 强制刷新当前房间的历史记录（非常关键）
            RoomManager.Instance.UpdateRoomHistoryIfNeeded();

            if (branchPanel == null)
            {
                Debug.Log("[DEBUG] branchPanel is null, trying GetComponent...");
                branchPanel = lifeMenuPanel.GetComponent<BranchOverviewPanel>();
                Debug.Log("[DEBUG] branchPanel result: " + branchPanel);
                if (branchPanel == null)
                {
                    Debug.LogError("[PauseMenuController] ❌ branchPanel 获取失败！");
                    return;
                }
            }

            Debug.Log("[DEBUG] Calling RefreshButtons...");
            branchPanel.RefreshButtons();
        });

        Debug.Log("[DEBUG] Start() end.");

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
