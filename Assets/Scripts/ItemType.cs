using UnityEngine;

public enum PlacementType
{
    Floor,
    Wall
}

public class ItemType : MonoBehaviour
{
    [Tooltip("该物体应吸附的平面类型")]
    public PlacementType type = PlacementType.Floor;


}
