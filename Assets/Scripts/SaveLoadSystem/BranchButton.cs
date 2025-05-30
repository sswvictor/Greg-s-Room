using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class BranchButtonController : MonoBehaviour
{
    public Image thumbnail;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI summaryText;
    public Button button;
    public int roomIndex;

    public GameObject lifeMenuPanel;

    public GameObject pauseMenuPanel;

    public void Setup(RoomHistory history, bool isCurrent)
    {
        titleText.text = $"Room {roomIndex}";

        if (isCurrent)
        {
            statusText.text = "Ongoing";
            statusText.color = Color.cyan;
            button.interactable = true;  
        }
        else if (history.unlocked)
        {
            statusText.text = "Finished";
            statusText.color = new Color(0.2f, 0.8f, 0.2f);
            button.interactable = true;  
        }
        else
        {
            statusText.text = "Locked";
            statusText.color = Color.red;
            button.interactable = false;
            return;
        }


        summaryText.text = GenerateSummary(history);

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (RoomManager.Instance.roomHistories.TryGetValue(roomIndex, out var history))
            {
                history.placedItemNames.Clear();
                history.finished = false;
            }

            RoomManager.Instance.LoadRoomByIndex(roomIndex, true);
        });

;


    }



    private string GenerateSummary(RoomHistory history)
    {


        string[] keyObjects = null;

        if (roomIndex == 0)
            keyObjects = new[] { "DeskComputer_Prefab", "Basketball_Prefab", "Poster_Prefab" };
        else if (roomIndex == 1)
            keyObjects = new[] { "Weeds_Prefab", "Weights_Prefab", "Couch_Prefab" };
        else if (roomIndex == 2)
            keyObjects = new[] { "DeskComputer_Prefab", "Kallax_Prefab", "TV_Prefab" };
        else
            keyObjects = new string[0]; 
 

        HashSet<string> placed = new(history.placedItemNames);

        List<string> lines = new();
        foreach (var item in keyObjects)
        {
            bool isPlaced = placed.Contains(item);
            string itemN = item.Replace("_Prefab", "");
            string color = isPlaced ? "#2ECC71" : "#AAAAAA";
            lines.Add($"<color={color}>• {itemN}</color>");
        }

        return string.Join("\n", lines);
    }

    public void ExitLifeAndResume()
    {
        // lifeMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;

    }

}
