using UnityEngine;
using UnityEditor;

public class VRUICustomMenu : MonoBehaviour
{
    [MenuItem("GameObject/VRUI Component/VRUIPanel", false, 10)]
    private static void CreateVRUIPanel()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUIPanel.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUIPanel";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUIPanel");
    }

    [MenuItem("GameObject/VRUI Component/VRUIScrollPanel", false, 10)]
    private static void CreateVRUIScrollPanel()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUIScrollPanel.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUIScrollPanel";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUIScrollPanel");
    }

    [MenuItem("GameObject/VRUI Component/VRUIButton", false, 10)]
    private static void CreateVRUIButton()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUIButton.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUIButton";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUIButton");
    }

    [MenuItem("GameObject/VRUI Component/VRUIToggle", false, 10)]
    private static void CreateVRUIToggle()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUIToggle.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUIToggle";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUIToggle");
    }

    [MenuItem("GameObject/VRUI Component/VRUISlider", false, 10)]
    private static void CreateVRUISlider()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUISlider.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUISlider";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUISlider");
    }

    [MenuItem("GameObject/VRUI Component/VRUITextcontainer", false, 10)]
    private static void CreateVRUITextcontainer()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/VRUI/VRUITextcontainer.prefab", typeof(GameObject)) as GameObject;
        GameObject instance = Instantiate(prefab);
        instance.name = "VRUITextcontainer";
        Undo.RegisterCreatedObjectUndo(instance, "Create VRUITextcontainer");
    }
}
