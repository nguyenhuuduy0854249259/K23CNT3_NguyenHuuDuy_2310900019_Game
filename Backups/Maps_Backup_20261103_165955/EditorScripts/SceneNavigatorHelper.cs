#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ hỗ trợ điều hướng nhanh giữa các Scene và tự động khôi phục về MainGame.unity.
    /// Giúp người dùng luôn ở đúng Scene làm việc chính chứa đầy đủ các chỉnh sửa Map 1 và Map 2.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneNavigatorHelper
    {
        private const string MainGameScenePath = "Assets/Scenes/MainGame.unity";
        private const string DemoScenePath = "Assets/Scenes/Forest_Demo_Map.unity";
        private const string AutoSwitchSessionKey = "SceneNavigator_AutoSwitchedToMainGame_v1";

        static SceneNavigatorHelper()
        {
            EditorApplication.delayCall += AutoSwitchToMainGameOnce;
        }

        private static void AutoSwitchToMainGameOnce()
        {
            if (Application.isPlaying) return;

            // Chỉ chạy tự động 1 lần trong phiên làm việc để đưa người dùng về Scene chính
            if (SessionState.GetBool(AutoSwitchSessionKey, false)) return;
            SessionState.SetBool(AutoSwitchSessionKey, true);

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "MainGame" && File.Exists(MainGameScenePath))
            {
                Debug.Log($"<color=cyan>[SceneNavigator] Đang tự động mở lại Scene làm việc chính: {MainGameScenePath}...</color>");
                EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);
                Debug.Log("<color=green><b>[SceneNavigator] ✅ ĐÃ MỞ THÀNH CÔNG SCENE 'MainGame'! Toàn bộ chỉnh sửa Map 1 và Map 2 đã hiển thị đầy đủ trên màn hình!</b></color>");
            }
        }

        [MenuItem("Tools/Map/🌟 Mở Scene Chính MainGame (Chứa Đầy Đủ Map 1 & Map 2) %&m", priority = -200)]
        public static void OpenMainGameScene()
        {
            if (!File.Exists(MainGameScenePath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy Scene tại đường dẫn: {MainGameScenePath}", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);
                Debug.Log("<color=green><b>[SceneNavigator] ✅ Đã mở Scene chính MainGame.unity (Map 1 & Map 2 đầy đủ).</b></color>");
            }
        }

        [MenuItem("Tools/Map/Mở Scene Demo (Forest_Demo_Map)", priority = -199)]
        public static void OpenDemoScene()
        {
            if (!File.Exists(DemoScenePath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy Scene tại đường dẫn: {DemoScenePath}", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
                Debug.Log("<color=yellow>[SceneNavigator] Đã mở Scene demo Forest_Demo_Map.unity.</color>");
            }
        }
    }
}
#endif
