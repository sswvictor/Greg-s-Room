using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadCutscene()
    {
        StartCoroutine(LoadCutsceneAsync());
    }

    private IEnumerator LoadCutsceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("CutsceneScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene cutsceneScene = SceneManager.GetSceneByName("CutsceneScene");
        SceneManager.SetActiveScene(cutsceneScene);

        // Find and play the cutscene
        var director = FindObjectOfType<CutsceneDirector>();
        if (director != null)
        {
            director.PlayParallax();
        }
    }

    public void UnloadCutscene()
    {
        StartCoroutine(UnloadCutsceneAsync());
    }

    private IEnumerator UnloadCutsceneAsync()
    {
        Scene cutsceneScene = SceneManager.GetSceneByName("CutsceneScene");
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(cutsceneScene);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // Return to main scene
        Scene mainScene = SceneManager.GetSceneByName("MainScene3D");
        SceneManager.SetActiveScene(mainScene);

        // Continue with room transition
        RoomManager.Instance.ContinueToNextRoom();
    }
}
