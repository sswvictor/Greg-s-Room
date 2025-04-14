using UnityEngine;

public class IsometricCameraSwitcher : MonoBehaviour
{
    public CameraMapper mapper;  // ✅ 引用 CameraMapper（场景中需拖入）

    public void SwitchCamera()
    {
        mapper.SwitchNext();  // 切换到下一个相机，同时更新映射
    }
}
