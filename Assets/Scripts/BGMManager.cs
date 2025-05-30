using UnityEngine;
using System.Collections.Generic;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;

    [System.Serializable]
    public class RoomBGM
    {
        public int roomIndex;
        public AudioClip bgmClip;
    }

    public List<RoomBGM> roomBGMs;

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

    public void PlayRoomBGM(int roomIndex)
    {
        AudioClip clip = null;
        foreach (var entry in roomBGMs)
        {
            if (entry.roomIndex == roomIndex)
            {
                clip = entry.bgmClip;
                break;
            }
        }

        if (clip == null)
        {
            return;
        }

        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
}
