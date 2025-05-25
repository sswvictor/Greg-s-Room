using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EndSceneController : MonoBehaviour
{
    [Header("StartMenu 场景名")]
    public string startMenuSceneName = "StartMenu";  // ⚠️ 请确保拼写与场景一致，并已加入 Build Settings

    // 👉 退出游戏
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  // 编辑器中退出播放模式
#else
        Application.Quit();  // 打包后退出游戏
#endif
    }

    // 👉 重启游戏，跳转回 StartMenu
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(startMenuSceneName);
    }
}
