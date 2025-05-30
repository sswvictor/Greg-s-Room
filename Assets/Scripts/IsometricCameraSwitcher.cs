using UnityEngine;

public class IsometricCameraSwitcher : MonoBehaviour
{
    public CameraMapper mapper;  

    public void SwitchCamera()
    {
        mapper.SwitchNext();  

        Camera currentCam = mapper.GetCurrentCamera();
        GameObject currentRoom = RoomManager.Instance?.currentRoom;

        if (currentRoom == null)
        {
            return;
        }

        WallVisibilityController wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl == null)
        {
            return;
        }

        if (currentCam.name.Contains("Main"))
        {
            wallCtrl.ShowMainView(); 
        }
        else if (currentCam.name.Contains("Side"))
        {
            wallCtrl.ShowSideView(); 
        }
    }
}
