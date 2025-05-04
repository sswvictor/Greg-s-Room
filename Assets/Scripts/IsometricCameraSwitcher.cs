using UnityEngine;

public class IsometricCameraSwitcher : MonoBehaviour
{
    public CameraMapper mapper;  // 拖入 CameraMapper 组件

    public void SwitchCamera()
    {
        mapper.SwitchNext();  // 切换到下一个相机，同时更新映射

        Camera currentCam = mapper.GetCurrentCamera();
        GameObject currentRoom = RoomManager.Instance?.currentRoom;

        if (currentRoom == null)
        {
            Debug.LogWarning("[CameraSwitcher] currentRoom is null.");
            return;
        }

        WallVisibilityController wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl == null)
        {
            Debug.LogWarning("[CameraSwitcher] WallVisibilityController not found on currentRoom.");
            return;
        }

        // 根据相机名称判断调用哪个墙体显隐方法
        if (currentCam.name.Contains("Main"))
        {
            wallCtrl.ShowMainView(); // 主视角：显示墙0/1，隐藏墙2/3
        }
        else if (currentCam.name.Contains("Side"))
        {
            wallCtrl.ShowSideView(); // 侧视角：显示墙2/3，隐藏墙0/1
        }
    }
}
