using UnityEngine;

public class DeviceTypeChanger : MonoBehaviour
{
    public static DeviceLevel type = DeviceLevel.HIGH;
    
    [SerializeField]
    private string typeName = "高端机";

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("高端机", GUILayout.Height(50), GUILayout.Width(120)))
        {
            type = DeviceLevel.HIGH;
            typeName = "高端机";
        }
        
        if (GUILayout.Button("中端机", GUILayout.Height(50), GUILayout.Width(120)))
        {
            type = DeviceLevel.MIDDLE;
            typeName = "中端机";
        }
        
        if (GUILayout.Button("低端机", GUILayout.Height(50), GUILayout.Width(120)))
        {
            type = DeviceLevel.LOW;
            typeName = "低端机";
        }
    }
}
