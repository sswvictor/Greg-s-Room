using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("通用按钮点击音效")]
    public AudioClip buttonClickSound;

    [Header("ItemBox 按钮点击音效")]
    public AudioClip itemClickSound;

    [Header("物体放置成功音效")]
    public AudioClip placeSound;

    [Header("播放音效的 AudioSource")]
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() => PlayButtonClick());
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound != null && audioSource != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    public void PlayItemClick()
    {
        if (itemClickSound != null && audioSource != null)
            audioSource.PlayOneShot(itemClickSound);
    }

    public void PlayPlaceSound()
    {
        if (placeSound != null && audioSource != null)
            audioSource.PlayOneShot(placeSound);
    }
}
