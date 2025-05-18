using UnityEngine;

public class DebugInstanceLogger : MonoBehaviour
{
    void Awake()
    {
        string parent = transform.parent != null ? transform.parent.name : "Scene Root";
        Debug.Log($"[🛠️ INSTANCE] Created {name}, parent = {parent}, time = {Time.time}");

        Debug.Log($"[🧠 STACK TRACE]\n{System.Environment.StackTrace}");
    }
}
