using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EndSceneController : MonoBehaviour
{
    [Header("StartMenu 场景名")]
    public string startMenuSceneName = "StartMenu"; 
    public void QuitGame()
    {

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  
#else
        Application.Quit();  
#endif
    }


    public void RestartGame()
    {
        var oldBGM = GameObject.Find("BGMManager");
        if (oldBGM != null)
        {
            Destroy(oldBGM);
        }
        SceneManager.LoadScene(startMenuSceneName);
    }
}
