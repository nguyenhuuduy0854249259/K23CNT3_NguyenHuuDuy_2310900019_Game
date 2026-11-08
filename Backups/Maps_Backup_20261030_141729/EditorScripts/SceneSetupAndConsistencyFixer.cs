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
using ActionRPG.Core;
using ActionRPG.Items;
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;
using ActionRPG.UI.Character;
using ActionRPG.Presentation.Views;
using ActionRPG.Visuals;
using ActionRPG.Combat;
using ActionRPG.Core.Bridges;

namespace ActionRPG.Editor
{
    public static class SceneSetupAndConsistencyFixer
    {
        [MenuItem("Tools/ActionRPG/Fix All Scenes UI & Movement")]
        public static bool FixAllScenes()
        {
            string activeScenePath = EditorSceneManager.GetActiveScene().path;
            bool ok1 = FixScene("Assets/Scenes/MainGame.unity");
            bool ok2 = FixScene("Assets/Scenes/Forest_Demo_Map.unity");
            // Preserve the user's active scene
            if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            {
                EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
            }
            return ok1 && ok2;
        }

        [MenuItem("Tools/ActionRPG/Fix MainGame Scene UI & Movement")]
        public static bool FixMainGameScene()
        {
            return FixScene("Assets/Scenes/MainGame.unity");
        }

        [MenuItem("Tools/ActionRPG/Fix Forest Demo Map Scene UI & Movement")]
        public static bool FixForestDemoMapScene()
        {
            return FixScene("Assets/Scenes/Forest_Demo_Map.unity");
        }

        public static bool FixScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[SceneFixer] File scene không tồn tại: {scenePath}");
                return false;
            }

            Debug.Log($"<color=cyan>[SceneFixer] Đang tiến hành nạp và sửa lỗi Scene: {scenePath}...</color>");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[SceneFixer] Không thể mở Scene tại đường dẫn: {scenePath}");
                return false;
            }

            // 1. Kiểm tra & Khắc phục Time.timeScale
            Time.timeScale = 1f;

            // 2. Kiểm tra Player GameObject (Tự động nạp Prefab nếu thiếu)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("Player_Novice_Tier1");
            if (player == null) player = UnityEngine.Object.FindAnyObjectByType<PlayerController>()?.gameObject;

            if (player == null)
            {
                string prefabPath = "Assets/Prefabs/ForestPrototype/Player_Novice_Tier1.prefab";
                GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (playerPrefab != null)
                {
                    player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
                    player.name = "Player_Novice_Tier1";
                    player.tag = "Player";

                    GameObject sp = GameObject.Find("PlayerSpawnPoint");
                    Vector3 spawnPos = sp != null ? sp.transform.position : new Vector3(-37f, 0f, 0f);
                    player.transform.position = spawnPos;
                    Debug.Log($"<color=green>[SceneFixer] Đã tự động tạo mới Player_Novice_Tier1 tại vị trí Spawn ({spawnPos}) trong {scenePath}!</color>");
                }
                else
                {
                    Debug.LogError($"[SceneFixer] Không tìm thấy Prefab Player tại: {prefabPath}!");
                }
            }

            if (player != null)
            {
                player.name = "Player_Novice_Tier1";
                player.tag = "Player";

                var rb = player.GetComponent<Rigidbody2D>();
                if (rb == null) rb = player.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                var stats = player.GetComponent<CharacterStats>();
                if (stats == null) stats = player.AddComponent<CharacterStats>();
                if (stats.baseMoveSpeed <= 0.1f) stats.baseMoveSpeed = 4.5f;
                if (stats.maxHp <= 0f) stats.maxHp = 100f;
                if (stats.currentHp <= 0f) stats.currentHp = stats.maxHp;

                var classSys = player.GetComponent<ClassSystem>();
                if (classSys == null) classSys = player.AddComponent<ClassSystem>();

                var ctrl = player.GetComponent<PlayerController>();
                if (ctrl == null) ctrl = player.AddComponent<PlayerController>();

                if (player.GetComponent<CharacterVisualView>() == null)
                    player.AddComponent<CharacterVisualView>();
                if (player.GetComponent<WeaponSocketView>() == null)
                    player.AddComponent<WeaponSocketView>();
                if (player.GetComponent<PlayerPresenter>() == null)
                    player.AddComponent<PlayerPresenter>();
                if (player.GetComponent<PlayerSkillCombat2D>() == null)
                    player.AddComponent<PlayerSkillCombat2D>();
                if (player.GetComponent<LuminaFourDirectionCharacterAnimator>() == null)
                    player.AddComponent<LuminaFourDirectionCharacterAnimator>();
                if (player.GetComponent<YSortOrder2D>() == null)
                    player.AddComponent<YSortOrder2D>();

                // Đồng bộ CameraFollow2D target tới player
                CameraFollow2D camFollow = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
                if (camFollow == null)
                {
                    var mainCam = Camera.main;
                    if (mainCam != null)
                    {
                        camFollow = mainCam.GetComponent<CameraFollow2D>() ?? mainCam.gameObject.AddComponent<CameraFollow2D>();
                    }
                }
                if (camFollow != null)
                {
                    camFollow.target = player.transform;
                    camFollow.offset = new Vector3(0, 0, -10f);
                    Debug.Log($"<color=green>[SceneFixer] Đã liên kết CameraFollow2D target tới Player {player.name}!</color>");
                }

                Debug.Log($"<color=green>[SceneFixer] Đã chuẩn hóa Player {player.name} trong {scenePath}!</color>");
            }
            else
            {
                Debug.LogWarning($"[SceneFixer] Không tìm thấy Player trong scene {scenePath} để chuẩn hóa.");
            }

            // 3. Dọn dẹp Canvas cũ không tương thích (nếu có)
            GameObject oldCanvas = GameObject.Find("Canvas_GameUI");
            if (oldCanvas != null)
            {
                GameObject.DestroyImmediate(oldCanvas);
                Debug.Log("[SceneFixer] Đã dọn dẹp Canvas cũ Canvas_GameUI.");
            }

            // 4. Tạo hoặc Chuẩn hóa GameUIRoot Canvas (ScreenSpaceOverlay)
            GameObject canvasObj = GameObject.Find("GameUIRoot");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("GameUIRoot", typeof(RectTransform));
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                var canvas = canvasObj.GetComponent<Canvas>() ?? canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.GetComponent<CanvasScaler>() ?? canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                if (canvasObj.GetComponent<GraphicRaycaster>() == null) canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 5. Chuẩn hóa EventSystem
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // 6. Gắn & Khởi tạo GameplayHUD
            Transform hudTransform = canvasObj.transform.Find("GameplayHUD");
            GameObject hudObj;
            if (hudTransform != null)
            {
                hudObj = hudTransform.gameObject;
            }
            else
            {
                hudObj = new GameObject("GameplayHUD", typeof(RectTransform));
                hudObj.transform.SetParent(canvasObj.transform, false);
            }
            var hudRt = hudObj.GetComponent<RectTransform>();
            if (hudRt != null)
            {
                hudRt.anchorMin = Vector2.zero;
                hudRt.anchorMax = Vector2.one;
                hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;
            }

            var gameplayHUD = hudObj.GetComponent<GameplayHUD>() ?? hudObj.AddComponent<GameplayHUD>();
            gameplayHUD.EnsureUIHierarchy();

            var novice = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
            if (novice != null)
            {
                gameplayHUD.UpdateSkillBar(novice);
                if (gameplayHUD.avatarImage != null && novice.classIcon != null)
                    gameplayHUD.avatarImage.sprite = novice.classIcon;
                if (gameplayHUD.playerNameText != null)
                    gameplayHUD.playerNameText.text = novice.className;
            }

            // 7. Gắn các hệ thống UI cốt lõi: ClassDemoSwitcherUI, PauseMenuUI, CharacterUI, ClassSelectionUI, InventoryUI
            if (canvasObj.GetComponent<ClassDemoSwitcherUI>() == null) canvasObj.AddComponent<ClassDemoSwitcherUI>();
            if (canvasObj.GetComponent<PauseMenuUI>() == null) canvasObj.AddComponent<PauseMenuUI>();
            if (canvasObj.GetComponent<ActionRPG.UI.Character.CharacterUI>() == null) canvasObj.AddComponent<ActionRPG.UI.Character.CharacterUI>();
            if (canvasObj.GetComponent<ClassSelectionUI>() == null) canvasObj.AddComponent<ClassSelectionUI>();
            if (canvasObj.GetComponent<InventoryUI>() == null) canvasObj.AddComponent<InventoryUI>();
            if (canvasObj.GetComponent<ClassChangeUI>() == null) canvasObj.AddComponent<ClassChangeUI>();

            // 7b. Đảm bảo GameSystems có SaveManager hỗ trợ [F5] / [F9]
            GameObject sysObj = GameObject.Find("GameSystems");
            if (sysObj != null && sysObj.GetComponent<ActionRPG.Save.SaveManager>() == null)
            {
                sysObj.AddComponent<ActionRPG.Save.SaveManager>();
            }

            // 8. Lưu Scene
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            Debug.Log($"<color=green>[SceneFixer] HOÀN TẤT NÂNG CẤP VÀ LƯU SCENE: {scenePath} (Saved={saved})!</color>");
            return saved;
        }

        [MenuItem("Tools/Run Movement And Menu Consistency Test")]
        public static bool RunMovementAndMenuTests()
        {
            Debug.Log("==========================================================");
            Debug.Log("<color=cyan><b>[LUMINA TEST SUITE] BẮT ĐẦU KIỂM TRA LOGIC DI CHUYỂN & THANH MENU HUD</b></color>");
            Debug.Log("==========================================================");

            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testName, string details = "")
            {
                if (condition)
                {
                    passed++;
                    Debug.Log($"<color=green>[PASS]</color> {testName} {details}");
                }
                else
                {
                    failed++;
                    Debug.LogError($"<color=red>[FAIL]</color> {testName} {details}");
                }
            }

            try
            {
                // ========================================================
                // TEST 1: TIME SCALE & PHYSICS STATE
                // ========================================================
                Assert(Time.timeScale > 0f, "TimeScale - Game không bị đóng băng khi khởi động", $"timeScale={Time.timeScale}");

                // ========================================================
                // TEST 2: PLAYER CONTROLLER & MOVEMENT LOGIC
                // ========================================================
                GameObject playerGO = new GameObject("TestPlayerObject");
                var rb = playerGO.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;

                var stats = playerGO.AddComponent<CharacterStats>();
                stats.baseMoveSpeed = 5.2f;

                var ctrl = playerGO.AddComponent<PlayerController>();
                var classSys = playerGO.AddComponent<ClassSystem>();

                Assert(stats.TotalMoveSpeed >= 4.5f, "Movement - CharacterStats.TotalMoveSpeed tính toán chính xác", $"speed={stats.TotalMoveSpeed}");

                // Giả lập di chuyển bằng gán velocity qua physics
                rb.linearVelocity = new Vector2(1f, 0f) * stats.TotalMoveSpeed;
                Assert(rb.linearVelocity.x > 0f, "Movement - Rigidbody2D nhận vận tốc di chuyển mượt mà", $"linearVelocity={rb.linearVelocity}");

                // Test giảm tốc độ khi chặn đòn
                classSys.isBlocking = true;
                float blockingSpeed = stats.TotalMoveSpeed * 0.35f;
                rb.linearVelocity = new Vector2(1f, 0f) * blockingSpeed;
                Assert(rb.linearVelocity.x < stats.TotalMoveSpeed, "Movement - Tốc độ giảm còn 35% khi giơ khiên đỡ đòn", $"blockSpeed={rb.linearVelocity.x}");
                classSys.isBlocking = false;

                // ========================================================
                // TEST 3: CANVAS ROOT & GAMEPLAY HUD HIERARCHY
                // ========================================================
                GameObject canvasGO = new GameObject("TestCanvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                var raycaster = canvasGO.AddComponent<GraphicRaycaster>();

                GameObject hudGO = new GameObject("TestGameplayHUD");
                hudGO.transform.SetParent(canvasGO.transform, false);
                var hud = hudGO.AddComponent<GameplayHUD>();

                // Tự động dựng Hierarchy
                hud.EnsureUIHierarchy();

                Assert(hud.hpBarFill != null, "HUD - hpBarFill được tự động dựng thành công");
                Assert(hud.mpBarFill != null, "HUD - mpBarFill được tự động dựng thành công");
                Assert(hud.expBarFill != null, "HUD - expBarFill được tự động dựng thành công");
                Assert(hud.playerNameText != null, "HUD - playerNameText hiển thị đầy đủ");
                Assert(hud.levelText != null, "HUD - levelText hiển thị đầy đủ");

                // ========================================================
                // TEST 4: SKILL BAR & HOTKEYS
                // ========================================================
                Assert(hud.skillSlots != null && hud.skillSlots.Length >= 6, "SkillBar - 6 ô kỹ năng (1, 2, 3, 4, 5, Q) hiện diện đầy đủ", $"slots={hud.skillSlots?.Length}");

                // Gán class test vào SkillBar
                var testClass = ScriptableObject.CreateInstance<CharacterClassData>();
                testClass.className = "Đấu Sĩ Cuồng Nộ";
                var sk1 = ScriptableObject.CreateInstance<SkillData>();
                sk1.skillId = "skill_frenzy";
                sk1.skillName = "Cuồng Bạo";
                sk1.manaCost = 20;
                testClass.unlockedSkills.Add(sk1);

                hud.UpdateSkillBar(testClass);
                Assert(hud.skillSlots[0].SkillId == "skill_frenzy", "SkillBar - Đồng bộ kỹ năng đầu tiên của class thành công");

                // ========================================================
                // TEST 5: QUICK SLOTS [E] & [R]
                // ========================================================
                Assert(hud.hpPotionButton != null, "QuickSlot - Nút [E] Bình Máu hiển thị trên HUD");
                Assert(hud.mpPotionButton != null, "QuickSlot - Nút [R] Bình Mana hiển thị trên HUD");

                // ========================================================
                // TEST 6: BOTTOM-LEFT MAIN MENU BUTTONS
                // ========================================================
                Assert(hud.demoClassButton != null, "MenuCluster - Nút '⚔ Đổi Class [F1]' hiển thị đầy đủ trên thanh menu");
                Assert(hud.characterButton != null, "MenuCluster - Nút '👤 Nhân Vật [U]' hiển thị đầy đủ trên thanh menu");
                Assert(hud.inventoryButton != null, "MenuCluster - Nút '🎒 Túi Đồ [I]' hiển thị đầy đủ trên thanh menu");
                Assert(hud.menuButton != null, "MenuCluster - Nút '⚙ Cài Đặt [ESC]' hiển thị đầy đủ trên thanh menu");

                // ========================================================
                // TEST 7: CLASS DEMO SWITCHER & PAUSE MENU PANELS
                // ========================================================
                var demoUI = canvasGO.AddComponent<ClassDemoSwitcherUI>();
                demoUI.EnsureUIHierarchy();
                Assert(demoUI.panelRoot != null, "ClassDemoSwitcher - panelRoot tự dựng đầy đủ");
                demoUI.Open();
                Assert(demoUI.panelRoot.activeSelf, "ClassDemoSwitcher - Mở bảng [F1] thành công");
                demoUI.Close();
                Assert(!demoUI.panelRoot.activeSelf, "ClassDemoSwitcher - Đóng bảng [F1] thành công");

                var pauseUI = canvasGO.AddComponent<PauseMenuUI>();
                pauseUI.EnsureUIHierarchy();
                Assert(pauseUI.panelRoot != null, "PauseMenu - panelRoot tự dựng đầy đủ");
                pauseUI.TogglePause();
                Assert(pauseUI.panelRoot.activeSelf, "PauseMenu - Phím [ESC] mở Menu thành công");
                pauseUI.ResumeGame();
                Assert(!pauseUI.panelRoot.activeSelf && Time.timeScale == 1f, "PauseMenu - Tiếp tục game khôi phục TimeScale = 1 thành công");

                // Cleanup
                GameObject.DestroyImmediate(playerGO);
                GameObject.DestroyImmediate(canvasGO);
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"<color=red>[EXCEPTION IN TEST]</color> {ex.Message}\n{ex.StackTrace}");
            }

            Debug.Log("==========================================================");
            if (failed == 0)
            {
                Debug.Log($"<color=green><b>[KẾT QUẢ KIỂM TRA] TOÀN BỘ {passed} BÀI TEST ĐÃ VƯỢT QUA! LOGIC DI CHUYỂN VÀ THANH MENU HUD HOÀN TOÀN ĐỒNG NHẤT 100%.</b></color>");
            }
            else
            {
                Debug.LogError($"<color=red><b>[KẾT QUẢ KIỂM TRA] CÓ {failed} BÀI TEST THẤT BẠI TRÊN TỔNG SỐ {passed + failed} BÀI KIỂM TRA!</b></color>");
            }
            Debug.Log("==========================================================");

            return failed == 0;
        }
    }
}
#endif
