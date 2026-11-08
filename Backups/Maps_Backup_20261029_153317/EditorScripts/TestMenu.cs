using UnityEngine;
using UnityEditor;

public class TestMenu : EditorWindow
{
    [MenuItem("Tools/Lumina/Test Menu")]
    public static void Test()
    {
        Debug.Log("Test Menu Works!");
    }
}
