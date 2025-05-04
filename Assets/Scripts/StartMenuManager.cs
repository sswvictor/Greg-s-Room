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
        SceneManager.LoadScene("MainScene3D");
    }
    
    private void OnSettingsClick()
    {
        // TODO: Implement settings functionality
        Debug.Log("Settings clicked - To be implemented");
    }
}