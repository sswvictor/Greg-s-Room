using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    public Button ContinueBtn;
    public Button NewGameBtn;
    public Button SettingsBtn;
    
    private void Start()
    {
        // Check if save file exists and set continue button state
        // TODO: Implement proper save check
        ContinueBtn.interactable = false;
        BGMManager.Instance?.PlayRoomBGM(0);  // 约定 0 代表 StartMenu

        SetupButtonListeners();
    }
    
    private void SetupButtonListeners()
    {
        if (ContinueBtn != null)
            ContinueBtn.onClick.AddListener(OnContinueClick);
        
        if (NewGameBtn != null)
            NewGameBtn.onClick.AddListener(OnNewGameClick);
        
        if (SettingsBtn != null)
            SettingsBtn.onClick.AddListener(OnSettingsClick);
    }
    
    private void OnContinueClick()
    {
        // TODO: Load saved game scene
        SceneManager.LoadScene("MainScene3D"); // Load the main game scene
    }
    
    private void OnNewGameClick()
    {
        // Load first game scene
        var oldBGM = GameObject.Find("BGMManager");
        if (oldBGM != null)
        {
            Destroy(oldBGM);
            Debug.Log("[StartMenu] 🎵 Destroyed StartMenuBGM before scene switch");
        }
        SceneManager.LoadScene("MainScene3D");
    }
    
    private void OnSettingsClick()
    {
        // TODO: Implement settings functionality
        Debug.Log("Settings clicked - To be implemented");
    }
}