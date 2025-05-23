using UnityEngine;
using System.Collections.Generic;

public class BranchOverviewPanel : MonoBehaviour
{
    public List<BranchButtonController> buttons;

    public void RefreshButtons()
    {
        if (RoomManager.Instance == null || RoomManager.Instance.roomHistories == null)
        {
            Debug.LogWarning("[BranchPanel] RoomManager Not prepared");
            return;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            int index = i;

            RoomHistory history = RoomManager.Instance.roomHistories.ContainsKey(index)
                ? RoomManager.Instance.roomHistories[index]
                : new RoomHistory { unlocked = false, finished = false, placedItemNames = new() };

            bool isCurrent = RoomManager.Instance.CurrentRoomIndex == index;

            btn.roomIndex = index;
            btn.Setup(history, isCurrent);
        }
    }
}
