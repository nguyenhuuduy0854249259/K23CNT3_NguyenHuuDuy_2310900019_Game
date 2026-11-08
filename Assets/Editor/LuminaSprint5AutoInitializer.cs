#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ActionRPG.Editor;
using ActionRPG.Tests;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Tự động kích hoạt khi Unity nạp hoặc biên dịch lại script:
    /// 1. Tự động sinh trọn bộ Asset Sprite HUD Pixel Art 16-bit và Icons.
    /// 2. Tự động sinh 5 Kỹ Năng Tân Thủ ScriptableObjects và đồng bộ vào Class_Novice.asset.
    /// 3. Tự động chạy bộ kiểm thử Sprint 5 để đảm bảo toàn bộ hệ thống hoạt động 100%.
    /// </summary>
    [InitializeOnLoad]
    public static class LuminaSprint5AutoInitializer
    {
        static LuminaSprint5AutoInitializer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("LuminaSprint5AutoInitializer_Done", false))
                {
                    SessionState.SetBool("LuminaSprint5AutoInitializer_Done", true);
                    string skill1 = "Assets/ScriptableObjects/Skills/Skill_novice_fan_slash.asset";
                    if (!System.IO.File.Exists(skill1))
                    {
                        InitializeSprint5();
                    }
                }
            };
        }

        public static void InitializeSprint5()
        {
            try
            {
                // Kiểm tra xem đã có đủ 5 asset kỹ năng chưa, nếu chưa hoặc cần cập nhật thì chạy sinh toàn bộ
                string skill1 = "Assets/ScriptableObjects/Skills/Skill_novice_fan_slash.asset";
                bool needsGeneration = !System.IO.File.Exists(skill1);

                // 1. Sinh trọn bộ Sprite Pixel Art HUD
                LuminaHUDAssetGenerator.GenerateAllHUDAssets();

                // 2. Sinh trọn bộ Class và 5 Kỹ Năng Tân Thủ
                LuminaClassAssetDatabase.GenerateAll();

                // 3. Chuẩn hóa và Bake UI HUD vào các Scene (Forest_Demo_Map & MainGame)
                SceneSetupAndConsistencyFixer.FixAllScenes();

                // 4. Lưu và nạp AssetDatabase
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 5. Chạy bộ kiểm thử tự động Sprint 5
                Sprint5HUDAndNoviceSkillsTest.RunAllTests();

                Debug.Log("<color=#00FF7F><b>[LUMINA SPRINT 5] ✅ ĐÃ ĐỒNG BỘ THÀNH CÔNG HỆ THỐNG PIXEL ART HUD & 5 KỸ NĂNG TÂN THỦ CẤP 1 VÀO TẤT CẢ SCENE!</b></color>");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LuminaSprint5AutoInitializer] Lỗi khi tự động khởi tạo: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
#endif
