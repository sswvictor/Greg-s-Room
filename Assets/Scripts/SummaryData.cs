using UnityEngine;

public class SummaryData : MonoBehaviour
{
    public static SummaryData Instance;

    public int totalCHI;
    public string characterName;
    public Sprite characterSprite;
    public int nextRoomIndex;
    


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
