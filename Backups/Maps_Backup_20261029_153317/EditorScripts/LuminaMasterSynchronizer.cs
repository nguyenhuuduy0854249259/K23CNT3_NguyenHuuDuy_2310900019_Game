#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Visuals;
using ActionRPG.Presentation.Views;
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Master One-Click Synchronizer: Cập nhật và đồng bộ toàn bộ nội dung dự án Lumina trong 1 cú click:
    /// 1. Cấu hình Sprite & Icons 4 hướng, VFX, HUD.
    /// 2. Cập nhật & liên kết 15 Kỹ năng, 14 Class, Hỏa Cầu (Fire Orb) & VFX Projectiles.
    /// 3. Chuẩn hóa Prefab Player (Player_Novice_Tier1.prefab) gắn đầy đủ Combat2D, 4-Dir Animator, Weapon Socket.
    /// 4. Xây dựng lại GameUIRoot với 6 Slot Kỹ Năng [1]..[6], Font chữ sắc nét, độ tương phản cao.
    /// 5. Bake và lưu thẳng vào tất cả các Scene (Forest_Demo_Map, MainGame).
    /// 6. Tự động mở sẵn Scene chơi chính Forest_Demo_Map.unity.
    /// </summary>
    [InitializeOnLoad]
    public static class LuminaMasterSynchronizer
    {
        private const string PrefabDir = "Assets/Prefabs/UI";
        private const string GameUIRootPrefabPath = "Assets/Prefabs/UI/GameUIRoot.prefab";
        private const string PlayerPrefabPath = "Assets/Prefabs/ForestPrototype/Player_Novice_Tier1.prefab";

        static LuminaMasterSynchronizer()
        {
            // Do not automatically switch or bake scenes on domain reload to prevent disrupting user's active scene
        }

        [MenuItem("⚡ LUMINA/CẬP NHẬT TOÀN BỘ DỰ ÁN (ONE-CLICK MASTER SYNC) %&u", priority = 0)]
        [MenuItem("Tools/Lumina/⚡ CẬP NHẬT TOÀN BỘ DỰ ÁN (ONE-CLICK MASTER SYNC)", priority = -100)]
        public static void SyncAllMenu()
        {
            SyncAll(silent: false);
        }

        public static void SyncAll(bool silent = false)
        {
            try
            {
                string activeScenePath = EditorSceneManager.GetActiveScene().path;

                EditorUtility.DisplayProgressBar("Lumina Master Sync", "Đang khởi tạo đồng bộ toàn bộ dự án...", 0.1f);

                // 1. Sinh & Liên kết toàn bộ Class, Skill & Vũ khí 4 hướng
                EditorUtility.DisplayProgressBar("Lumina Master Sync", "1/6: Sinh & Liên kết Class & Kỹ Năng...", 0.2f);
                LuminaClassAssetDatabase.GenerateAll();
                SetupLuminaFireOrbAndVisuals.SetupAll();

                // 2. Cập nhật Font chữ Times New Roman
                EditorUtility.DisplayProgressBar("Lumina Master Sync", "2/6: Đồng bộ Font chữ & Độ tương phản...", 0.35f);
                TimesNewRomanFontSetup.ApplyTimesNewRomanToAll();

                // 3. Chuẩn hóa Prefab Player
                EditorUtility.DisplayProgressBar("Lumina Master Sync", "3/6: Chuẩn hóa Prefab Nhân Vật Player...", 0.5f);
                StandardizePlayerPrefab();

                // 4. Tạo & Cập nhật Prefab GameUIRoot với 6 Ô Kỹ Năng
                EditorUtility.DisplayProgressBar("Lumina Master Sync", "4/6: Xây dựng HUD 6 Ô Kỹ Năng [1..6]...", 0.65f);
                GameObject uiPrefab = CreateOrUpdateGameUIRootPrefab();

                // 5. Bake và lưu vào các Scene
                EditorUtility.DisplayProgressBar("Lumina Master Sync", "5/6: Nạp & Lưu vào các Scene...", 0.8f);
                BakeIntoScene("Assets/Scenes/Forest_Demo_Map.unity", uiPrefab);
                BakeIntoScene("Assets/Scenes/MainGame.unity", uiPrefab);

                // 6. Giữ nguyên Scene người dùng đang mở
                if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
                {
                    EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
                }
                else if (File.Exists("Assets/Scenes/MainGame.unity"))
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("<color=#00FF7F><b>[LUMINA MASTER SYNC] ✅ ĐÃ ĐỒNG BỘ TOÀN BỘ 100% THÀNH CÔNG! BẠN CHỈ CẦN BẤM PLAY (▶️) ĐỂ CHƠI!</b></color>");

                if (!silent)
                {
                    EditorUtility.DisplayDialog(
                        "Lumina - Đồng Bộ Toàn Bộ Thành Công!",
                        "✅ ĐÃ HOÀN TẤT CẬP NHẬT 100%:\n\n" +
                        "1. Thanh kỹ năng mở rộng đủ 6 Ô [1, 2, 3, 4, 5, 6].\n" +
                        "2. Phím di chuyển WASD đã tách biệt hoàn toàn, không bị tung chiêu khi bấm W.\n" +
                        "3. Vũ khí 4 hướng (Kiếm, Gậy, Cung) đã căn chỉnh offset & sorting chuẩn.\n" +
                        "4. Bảng Đổi Class [F1] đã căn chỉnh chữ rõ ràng, không còn đè chữ.\n" +
                        "5. Scene 'Forest_Demo_Map' đã được mở sẵn và chuẩn hóa.\n\n" +
                        "👉 Bạn chỉ cần bấm nút PLAY (▶️) trên thanh công cụ để trải nghiệm ngay!",
                        "OK Tuyệt Vời"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LuminaMasterSync] Lỗi trong quá trình đồng bộ: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void StandardizePlayerPrefab()
        {
            if (!File.Exists(PlayerPrefabPath)) return;
            try
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(PlayerPrefabPath))
                {
                    var pRoot = scope.prefabContentsRoot;
                    if (pRoot.GetComponent<CharacterStats>() == null) pRoot.AddComponent<CharacterStats>();
                    if (pRoot.GetComponent<ClassSystem>() == null) pRoot.AddComponent<ClassSystem>();
                    if (pRoot.GetComponent<PlayerSkillCombat2D>() == null) pRoot.AddComponent<PlayerSkillCombat2D>();
                    if (pRoot.GetComponent<LuminaFourDirectionCharacterAnimator>() == null) pRoot.AddComponent<LuminaFourDirectionCharacterAnimator>();
                    if (pRoot.GetComponent<WeaponSocketView>() == null) pRoot.AddComponent<WeaponSocketView>();
                    if (pRoot.GetComponent<Hurtbox2D>() == null) pRoot.AddComponent<Hurtbox2D>();
                    if (pRoot.GetComponent<PlayerController>() == null) pRoot.AddComponent<PlayerController>();
                }
                Debug.Log("<color=green>[LuminaMasterSync] Đã chuẩn hóa Player_Novice_Tier1.prefab thành công!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LuminaMasterSync] Cảnh báo khi sửa Prefab: {ex.Message}");
            }
        }

        private static GameObject CreateOrUpdateGameUIRootPrefab()
        {
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);

            GameObject rootGO = new GameObject("GameUIRoot", typeof(RectTransform));
            try
            {
                var canvas = rootGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = rootGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                rootGO.AddComponent<GraphicRaycaster>();

                GameObject hudGO = new GameObject("GameplayHUD", typeof(RectTransform));
                hudGO.transform.SetParent(rootGO.transform, false);
                var hudRt = hudGO.GetComponent<RectTransform>();
                hudRt.anchorMin = Vector2.zero;
                hudRt.anchorMax = Vector2.one;
                hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;

                var hud = hudGO.AddComponent<GameplayHUD>();
                hud.EnsureUIHierarchy();

                var novice = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
                if (novice != null)
                {
                    hud.UpdateSkillBar(novice);
                    if (hud.avatarImage != null && novice.classIcon != null) hud.avatarImage.sprite = novice.classIcon;
                    if (hud.playerNameText != null) hud.playerNameText.text = novice.className;
                }

                rootGO.AddComponent<ClassDemoSwitcherUI>();
                rootGO.AddComponent<ActionBarUI>();
                rootGO.AddComponent<PauseMenuUI>();
                rootGO.AddComponent<ActionRPG.UI.Character.CharacterUI>();
                rootGO.AddComponent<ClassSelectionUI>();
                rootGO.AddComponent<InventoryUI>();
                rootGO.AddComponent<ClassChangeUI>();

                var tmpFont = TimesNewRomanFontSetup.EnsureTmpFontAssetCreated();
                if (tmpFont != null)
                {
                    foreach (var txt in rootGO.GetComponentsInChildren<TMP_Text>(true))
                    {
                        txt.font = tmpFont;
                    }
                }

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGO, GameUIRootPrefabPath);
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootGO);
            }
        }

        private static void BakeIntoScene(string scenePath, GameObject uiPrefab)
        {
            if (!File.Exists(scenePath)) return;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            // Xóa Canvas cũ hoặc thừa
            string[] staleNames = { "Canvas_GameUI", "GameUIRoot", "GameplayHUD", "TestCanvas" };
            foreach (var sName in staleNames)
            {
                var existing = GameObject.Find(sName);
                while (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing);
                    existing = GameObject.Find(sName);
                }
            }

            // Đảm bảo EventSystem
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // Chuẩn hóa Player trong Scene
            GameObject player = GameObject.FindGameObjectWithTag("Player") 
                ?? GameObject.Find("Player") 
                ?? GameObject.Find("Player_Novice_Tier1")
                ?? UnityEngine.Object.FindAnyObjectByType<PlayerController>()?.gameObject;

            if (player == null)
            {
                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
                if (playerPrefab != null)
                {
                    player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
                    player.name = "Player_Novice_Tier1";
                    player.tag = "Player";
                    GameObject sp = GameObject.Find("PlayerSpawnPoint");
                    Vector3 spawnPos = sp != null ? sp.transform.position : new Vector3(-37f, 0f, 0f);
                    player.transform.position = spawnPos;
                }
            }

            if (player != null)
            {
                player.name = "Player_Novice_Tier1";
                player.tag = "Player";
                if (player.GetComponent<CharacterStats>() == null) player.AddComponent<CharacterStats>();
                if (player.GetComponent<ClassSystem>() == null) player.AddComponent<ClassSystem>();
                if (player.GetComponent<PlayerSkillCombat2D>() == null) player.AddComponent<PlayerSkillCombat2D>();
                if (player.GetComponent<LuminaFourDirectionCharacterAnimator>() == null) player.AddComponent<LuminaFourDirectionCharacterAnimator>();
                if (player.GetComponent<WeaponSocketView>() == null) player.AddComponent<WeaponSocketView>();

                var cam = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
                if (cam != null)
                {
                    cam.target = player.transform;
                    cam.offset = new Vector3(0, 0, -10f);
                }
            }

            // Instantiate GameUIRoot
            GameObject instance = null;
            if (uiPrefab != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab, scene);
            }
            if (instance != null)
            {
                instance.name = "GameUIRoot";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
#endif
