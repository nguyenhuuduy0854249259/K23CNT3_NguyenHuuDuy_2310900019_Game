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
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;
using ActionRPG.UI.Character;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ Editor tự động nạp và bake hệ thống Fantasy RPG HUD mới vào toàn bộ Scenes và Prefab của dự án.
    /// Thay thế triệt để canvas đồ họa cũ bằng bộ đồ họa mới sắc nét.
    /// Menu: Tools -> Lumina -> Bake New Fantasy HUD To Scenes & Prefabs
    /// </summary>
    [InitializeOnLoad]
    public static class BakeNewFantasyHUD
    {
        private const string PrefabDir = "Assets/Prefabs/UI";
        private const string PrefabPath = "Assets/Prefabs/UI/GameUIRoot.prefab";
        private static bool hasExecutedThisSession = false;

        static BakeNewFantasyHUD()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && !SessionState.GetBool("BakeNewFantasyHUD_Executed", false))
                {
                    SessionState.SetBool("BakeNewFantasyHUD_Executed", true);
                    hasExecutedThisSession = true;
                }
            };
        }

        [MenuItem("Tools/Lumina/Bake New Fantasy HUD To Scenes & Prefabs")]
        public static void BakeAll()
        {
            string activeScenePath = EditorSceneManager.GetActiveScene().path;
            Debug.Log("<color=cyan><b>[BakeNewFantasyHUD] BẮT ĐẦU CẬP NHẬT ĐỒ HỌA MỚI VÀO DỰ ÁN THAY THẾ CHO CANVAS CŨ...</b></color>");

            // 1. Cấu hình lại Texture Importer cho toàn bộ Sprite HUD mới
            ConfigureAllHUDSprites();

            // 2. Liên kết Icon vào ScriptableObjects kỹ năng và Class_Novice
            LinkIconsToScriptableObjects();

            // 2.5. Cập nhật font Times New Roman cho toàn bộ giao diện
            TimesNewRomanFontSetup.ApplyTimesNewRomanToAll();

            // 3. Chuẩn hóa Player_Novice_Tier1 Prefab
            StandardizePlayerPrefab();

            // 4. Tạo Prefab GameUIRoot với 6 ô kỹ năng mới và toàn bộ đồ họa chuẩn
            GameObject prefabAsset = CreateOrUpdateUIPrefab();

            // 5. Bake trực tiếp vào các Scene của dự án
            BakeIntoScene("Assets/Scenes/Forest_Demo_Map.unity", prefabAsset);
            BakeIntoScene("Assets/Scenes/MainGame.unity", prefabAsset);

            // Giữ nguyên Scene người dùng đang mở
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

            Debug.Log("<color=green><b>[BakeNewFantasyHUD] HOÀN TẤT 100%! Đồ họa mới đã được cập nhật thành công vào toàn bộ Scene và Prefab dự án!</b></color>");
        }

        private static void ConfigureAllHUDSprites()
        {
            string hudPath = "Assets/Sprites/UI/HUD";
            if (!Directory.Exists(hudPath)) return;

            string[] pngs = Directory.GetFiles(hudPath, "*.png", SearchOption.AllDirectories);
            foreach (var file in pngs)
            {
                string uPath = file.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(uPath) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        changed = true;
                    }

                    Vector4 targetBorder = Vector4.zero;
                    if (uPath.Contains("frame_skill_slot")) targetBorder = new Vector4(8, 8, 8, 8);
                    else if (uPath.Contains("frame_hud_bar_bg")) targetBorder = new Vector4(6, 6, 6, 6);
                    else if (uPath.Contains("frame_portrait")) targetBorder = new Vector4(8, 8, 8, 8);
                    else if (uPath.Contains("frame_boss_bar")) targetBorder = new Vector4(8, 8, 8, 8);

                    if (targetBorder != Vector4.zero && importer.spriteBorder != targetBorder)
                    {
                        importer.spriteBorder = targetBorder;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static void LinkIconsToScriptableObjects()
        {
            // Class Novice icon
            var novice = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
            var knightIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Icons/icon_character.png");
            if (novice != null && knightIcon != null)
            {
                novice.classIcon = knightIcon;
                EditorUtility.SetDirty(novice);
            }

            // 5 Novice Skills
            void Link(string skillId, string spriteFile)
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillData>($"Assets/ScriptableObjects/Skills/Skill_{skillId}.asset");
                var spr = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/HUD/Skills/{spriteFile}");
                if (skill != null && spr != null)
                {
                    skill.icon = spr;
                    EditorUtility.SetDirty(skill);
                }
            }

            Link("novice_fan_slash", "icon_skill_fan_slash.png");
            Link("novice_flurry_slash", "icon_skill_flurry_slash.png");
            Link("novice_aegis_barrier", "icon_skill_aegis_barrier.png");
            Link("novice_quad_dash", "icon_skill_quad_dash.png");
            Link("novice_holy_heal", "icon_skill_holy_heal.png");

            // 5 Warrior Skills
            Link("warrior_cleave", "icon_skill_warrior_cleave.png");
            Link("warrior_vanguard_charge", "icon_skill_warrior_vanguard_charge.png");
            Link("warrior_iron_bastion", "icon_skill_warrior_iron_bastion.png");
            Link("warrior_tremor_slam", "icon_skill_warrior_tremor_slam.png");
            Link("warrior_frenzied_strike", "icon_skill_warrior_frenzied_strike.png");

            // 5 Mage Skills
            Link("mage_fire_orb", "icon_skill_mage_fire_orb.png");
            Link("mage_frost_spikes", "icon_skill_mage_frost_spikes.png");
            Link("mage_blink", "icon_skill_mage_blink.png");
            Link("mage_chain_lightning", "icon_skill_mage_chain_lightning.png");
            Link("mage_mana_shield", "icon_skill_mage_mana_shield.png");

            // 5 Archer Skills
            Link("archer_piercing_shot", "icon_skill_archer_piercing_shot.png");
            Link("archer_volley", "icon_skill_archer_volley.png");
            Link("archer_tumble", "icon_skill_archer_tumble.png");
            Link("archer_snare_trap", "icon_skill_archer_snare_trap.png");
            Link("archer_deadeye", "icon_skill_archer_deadeye.png");

            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateOrUpdateUIPrefab()
        {
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);

            // Tạo GameObject tạm trong scene để build cấu trúc
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

                // Thêm GameplayHUD
                GameObject hudGO = new GameObject("GameplayHUD", typeof(RectTransform));
                hudGO.transform.SetParent(rootGO.transform, false);
                var hudRt = hudGO.GetComponent<RectTransform>();
                hudRt.anchorMin = Vector2.zero;
                hudRt.anchorMax = Vector2.one;
                hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;

                var hud = hudGO.AddComponent<GameplayHUD>();
                hud.EnsureUIHierarchy();

                // Nạp Class_Novice vào HUD để các slot hiển thị ngay icon mới
                var novice = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
                if (novice != null)
                {
                    hud.UpdateSkillBar(novice);
                    if (hud.avatarImage != null && novice.classIcon != null)
                    {
                        hud.avatarImage.sprite = novice.classIcon;
                    }
                    if (hud.playerNameText != null)
                    {
                        hud.playerNameText.text = novice.className;
                    }
                }

                // Gắn các controller UI khác vào root
                rootGO.AddComponent<ClassDemoSwitcherUI>();
                rootGO.AddComponent<PauseMenuUI>();
                rootGO.AddComponent<ActionRPG.UI.Character.CharacterUI>();
                rootGO.AddComponent<ClassSelectionUI>();
                rootGO.AddComponent<InventoryUI>();
                rootGO.AddComponent<ClassChangeUI>();

                // Gán font Times New Roman cho toàn bộ Text trong Prefab
                var tmpFont = TimesNewRomanFontSetup.EnsureTmpFontAssetCreated();
                if (tmpFont != null)
                {
                    foreach (var txt in rootGO.GetComponentsInChildren<TMP_Text>(true))
                    {
                        txt.font = tmpFont;
                    }
                }

                // Lưu thành Prefab
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGO, PrefabPath);
                Debug.Log($"<color=cyan>[BakeNewFantasyHUD] Đã tạo và lưu Prefab chuẩn tại: {PrefabPath}</color>");
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootGO);
            }
        }

        private static void BakeIntoScene(string scenePath, GameObject prefabAsset)
        {
            if (!File.Exists(scenePath)) return;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            // Xóa Canvas cũ hoặc GameObject rác
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

            // Instantiate Prefab GameUIRoot mới
            GameObject instance = null;
            if (prefabAsset != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, scene);
            }

            if (instance == null)
            {
                instance = new GameObject("GameUIRoot", typeof(RectTransform));
                var c = instance.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var cs = instance.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                instance.AddComponent<GraphicRaycaster>();

                GameObject hObj = new GameObject("GameplayHUD", typeof(RectTransform));
                hObj.transform.SetParent(instance.transform, false);
                var h = hObj.AddComponent<GameplayHUD>();
                h.EnsureUIHierarchy();
            }

            instance.name = "GameUIRoot";

            // Chuẩn hóa Player trong Scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("Player_Novice_Tier1");
            if (player == null) player = UnityEngine.Object.FindAnyObjectByType<PlayerController>()?.gameObject;

            if (player != null)
            {
                if (player.GetComponent<CharacterStats>() == null) player.AddComponent<CharacterStats>();
                if (player.GetComponent<ClassSystem>() == null) player.AddComponent<ClassSystem>();
                if (player.GetComponent<ActionRPG.Combat.PlayerSkillCombat2D>() == null) player.AddComponent<ActionRPG.Combat.PlayerSkillCombat2D>();
                if (player.GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>() == null) player.AddComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
                if (player.GetComponent<ActionRPG.Presentation.Views.WeaponSocketView>() == null) player.AddComponent<ActionRPG.Presentation.Views.WeaponSocketView>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            Debug.Log($"<color=green>[BakeNewFantasyHUD] Đã nạp và lưu thành công Canvas đồ họa mới vào Scene: {scenePath} (Saved={saved})</color>");
        }

        private static void StandardizePlayerPrefab()
        {
            string playerPrefabPath = "Assets/Prefabs/ForestPrototype/Player_Novice_Tier1.prefab";
            if (!File.Exists(playerPrefabPath)) return;

            try
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(playerPrefabPath))
                {
                    var pRoot = scope.prefabContentsRoot;
                    if (pRoot.GetComponent<CharacterStats>() == null) pRoot.AddComponent<CharacterStats>();
                    if (pRoot.GetComponent<ClassSystem>() == null) pRoot.AddComponent<ClassSystem>();
                    if (pRoot.GetComponent<ActionRPG.Combat.PlayerSkillCombat2D>() == null) pRoot.AddComponent<ActionRPG.Combat.PlayerSkillCombat2D>();
                    if (pRoot.GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>() == null) pRoot.AddComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
                    if (pRoot.GetComponent<ActionRPG.Presentation.Views.WeaponSocketView>() == null) pRoot.AddComponent<ActionRPG.Presentation.Views.WeaponSocketView>();
                }
                Debug.Log("<color=green>[BakeNewFantasyHUD] Đã chuẩn hóa Player_Novice_Tier1.prefab với đủ Combat2D và 4Dir Animator!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BakeNewFantasyHUD] Không thể cập nhật Prefab Player qua EditPrefabContentsScope: {ex.Message}");
            }
        }
    }
}
#endif
