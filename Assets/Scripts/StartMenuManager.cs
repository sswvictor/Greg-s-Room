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
        ContinueBtn.interactable = false;
        BGMManager.Instance?.PlayRoomBGM(0);  

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
        SceneManager.LoadScene("MainScene3D"); 
    }
    
    private void OnNewGameClick()
    {
        var oldBGM = GameObject.Find("BGMManager");
        if (oldBGM != null)
        {
            Destroy(oldBGM);
        }
        SceneManager.LoadScene("MainScene3D");
    }
    
    private void OnSettingsClick()
    {
        // TODO: Implement settings functionality
    }
}