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

        [MenuItem("Tools/Lumina/Upgrade UI Canvas Quality & Layout (Safe - No Map Touch)")]
        [MenuItem("Tools/Lumina/Bake New Fantasy HUD To Scenes & Prefabs")]
        public static void BakeAll()
        {
            Debug.Log("<color=cyan><b>[BakeNewFantasyHUD] BẮT ĐẦU NÂNG CẤP HỆ THỐNG UI CANVAS PIXEL-PERFECT (KHÔNG ĐỤNG ĐẾN MAP)...</b></color>");

            // 1. Cấu hình lại Texture Importer cho toàn bộ Sprite UI (Point Filter, Uncompressed, MipMaps Disabled, 9-Slice)
            ConfigureAllHUDSprites();

            // 2. Liên kết Icon vào ScriptableObjects kỹ năng và Class_Novice
            LinkIconsToScriptableObjects();

            // 2.5. Cập nhật font Times New Roman SDF (Atlas 2048x2048 + Fallback + ExtraPadding)
            TimesNewRomanFontSetup.ApplyTimesNewRomanToAll();

            // 3. Chuẩn hóa Player_Novice_Tier1 Prefab
            StandardizePlayerPrefab();

            // 4. Tạo 4 Prefab Ô Giao Diện Tái Sử Dụng (ClassSlot, ItemSlot, SkillSlot, EquipmentSlot)
            CreateReusableUISlotPrefabs();

            // 5. Tạo/Cập nhật Prefab GameUIRoot với toàn bộ bố cục Pixel-Perfect
            GameObject prefabAsset = CreateOrUpdateUIPrefab();

            // 6. Nâng cấp trực tiếp GameUIRoot trên Scene đang mở (Tuyệt đối không mở/ghi đè các Scene Map trên ổ cứng)
            UpgradeActiveSceneUICanvasOnly(prefabAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[BakeNewFantasyHUD] HOÀN TẤT 100%! Hệ thống UI Canvas Pixel-Perfect, 4 Slot Prefabs và Font Tiếng Việt đã được nâng cấp an toàn tuyệt đối (Không đụng đến Map)!</b></color>");
        }

        private static void ConfigureAllHUDSprites()
        {
            string hudPath = "Assets/Sprites/UI";
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
                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        changed = true;
                    }
                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
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
                CharacterStatusUI.EnsureUI(rootGO.transform);
                InventoryBagUI.EnsureUI(rootGO.transform);
                MinimapSystem2D.EnsureUI(rootGO.transform);

                var switcher = rootGO.AddComponent<ClassDemoSwitcherUI>();
                switcher.EnsureUIHierarchy();
                rootGO.AddComponent<PauseMenuUI>();
                rootGO.AddComponent<ActionRPG.UI.Character.CharacterUI>();
                rootGO.AddComponent<ClassSelectionUI>();
                rootGO.AddComponent<InventoryUI>();
                rootGO.AddComponent<ClassChangeUI>();

                // Mặc định ẩn các panel khi vào game (nhưng component script vẫn nằm trên rootGO và active)
                var charStatus = rootGO.GetComponent<CharacterStatusUI>();
                if (charStatus != null && charStatus.panelRoot != null) charStatus.panelRoot.SetActive(false);

                var invBag = rootGO.GetComponent<InventoryBagUI>();
                if (invBag != null && invBag.panelRoot != null) invBag.panelRoot.SetActive(false);

                if (switcher.panelRoot != null) switcher.panelRoot.SetActive(false);

                // Gán font Times New Roman + ExtraPadding + Sanitize tọa độ nguyên
                var tmpFont = TimesNewRomanFontSetup.EnsureTmpFontAssetCreated();
                if (tmpFont != null)
                {
                    foreach (var txt in rootGO.GetComponentsInChildren<TMP_Text>(true))
                    {
                        txt.font = tmpFont;
                        txt.extraPadding = true;
                        txt.transform.localScale = Vector3.one;
                    }
                }
                FontManager.SanitizeUIHierarchy(rootGO.transform);

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

        private static void CreateReusableUISlotPrefabs()
        {
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);
            Sprite slot9Slice = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png") ?? FontManager.GetOrCreateSlotNineSliceSprite();

            // 1. ClassSlot_Prefab.prefab (50px height, 9-Slice, Cyan highlight ready)
            CreateSinglePrefab("Assets/Prefabs/UI/ClassSlot_Prefab.prefab", "ClassSlot_Prefab", root =>
            {
                var rt = root.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(320f, 50f);
                var le = root.AddComponent<LayoutElement>();
                le.minHeight = 50f;
                le.preferredHeight = 50f;
                le.flexibleWidth = 1f;

                var img = root.AddComponent<Image>();
                img.sprite = slot9Slice;
                img.type = Image.Type.Sliced;
                img.color = FontManager.SlotEmptyBgColor;

                var outline = root.AddComponent<Outline>();
                outline.effectColor = FontManager.AetherCyanColor;
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                root.AddComponent<Button>();

                var txtGO = new GameObject("Text", typeof(RectTransform));
                txtGO.transform.SetParent(root.transform, false);
                var trt = txtGO.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(10f, 4f);
                trt.offsetMax = new Vector2(-10f, -4f);

                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "<b>Hiệp Sĩ Lumina</b>\n<size=11><color=#FFD54F>Tier 2 Lumina</color> | <color=#80D8FF>Cận Chiến</color></size>";
                tmp.fontSize = 13f;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.color = FontManager.IvoryTextColor;
                FontManager.ApplyFont(tmp);
            });

            // 2. ItemSlot_Prefab.prefab (52x52 px, 9-Slice, Icon preserveAspect, Quantity bottom-right with black outline)
            CreateSinglePrefab("Assets/Prefabs/UI/ItemSlot_Prefab.prefab", "ItemSlot_Prefab", root =>
            {
                var rt = root.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(52f, 52f);

                var img = root.AddComponent<Image>();
                img.sprite = slot9Slice;
                img.type = Image.Type.Sliced;
                img.color = FontManager.SlotEmptyBgColor;

                var outline = root.AddComponent<Outline>();
                outline.effectColor = new Color(0.45f, 0.38f, 0.22f, 0.55f);
                outline.effectDistance = new Vector2(1f, -1f);
                root.AddComponent<Button>();

                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(root.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = Vector2.zero;
                irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(6f, 6f);
                irt.offsetMax = new Vector2(-6f, -6f);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.color = Color.clear;
                iconImg.enabled = false;
                iconImg.raycastTarget = false;

                var qtyGO = new GameObject("Quantity", typeof(RectTransform));
                qtyGO.transform.SetParent(root.transform, false);
                var qrt = qtyGO.GetComponent<RectTransform>();
                qrt.anchorMin = Vector2.zero;
                qrt.anchorMax = Vector2.one;
                qrt.offsetMin = new Vector2(2f, 2f);
                qrt.offsetMax = new Vector2(-4f, -2f);
                var qTmp = qtyGO.AddComponent<TextMeshProUGUI>();
                qTmp.text = "";
                qTmp.fontSize = 11f;
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.alignment = TextAlignmentOptions.BottomRight;
                qTmp.color = FontManager.GoldTitleColor;
                FontManager.ApplyFont(qTmp);
                var qOut = qtyGO.AddComponent<Outline>();
                qOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
                qOut.effectDistance = new Vector2(1f, -1f);
            });

            // 3. SkillSlot_Prefab.prefab (56x56 px, SkillSlotUI attached, Icon, CooldownOverlay, Key, ManaCost, CDText)
            CreateSinglePrefab("Assets/Prefabs/UI/SkillSlot_Prefab.prefab", "SkillSlot_Prefab", root =>
            {
                var rt = root.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(56f, 56f);

                var img = root.AddComponent<Image>();
                img.sprite = slot9Slice;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                root.AddComponent<Button>();
                var slotUI = root.AddComponent<SkillSlotUI>();

                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(root.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = Vector2.zero;
                irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(6f, 6f);
                irt.offsetMax = new Vector2(-6f, -6f);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.color = Color.clear;
                iconImg.enabled = false;
                slotUI.iconImage = iconImg;

                var cdGO = new GameObject("CooldownOverlay", typeof(RectTransform));
                cdGO.transform.SetParent(root.transform, false);
                var cdrt = cdGO.GetComponent<RectTransform>();
                cdrt.anchorMin = Vector2.zero;
                cdrt.anchorMax = Vector2.one;
                cdrt.offsetMin = new Vector2(4f, 4f);
                cdrt.offsetMax = new Vector2(-4f, -4f);
                var cdImg = cdGO.AddComponent<Image>();
                cdImg.color = new Color(0f, 0f, 0f, 0.75f);
                cdImg.type = Image.Type.Filled;
                cdImg.fillMethod = Image.FillMethod.Radial360;
                cdImg.fillAmount = 0f;
                cdImg.enabled = false;
                slotUI.cooldownOverlay = cdImg;

                var keyGO = new GameObject("Key", typeof(RectTransform));
                keyGO.transform.SetParent(root.transform, false);
                var krt = keyGO.GetComponent<RectTransform>();
                krt.anchorMin = Vector2.zero;
                krt.anchorMax = Vector2.one;
                krt.offsetMin = new Vector2(5f, 5f);
                krt.offsetMax = new Vector2(-5f, -5f);
                var kTmp = keyGO.AddComponent<TextMeshProUGUI>();
                kTmp.text = "1";
                kTmp.fontSize = 12f;
                kTmp.fontStyle = FontStyles.Bold;
                kTmp.alignment = TextAlignmentOptions.TopLeft;
                kTmp.color = FontManager.GoldTitleColor;
                FontManager.ApplyFont(kTmp);
                slotUI.shortcutKeyText = kTmp;

                var manaGO = new GameObject("ManaCost", typeof(RectTransform));
                manaGO.transform.SetParent(root.transform, false);
                var mrt = manaGO.GetComponent<RectTransform>();
                mrt.anchorMin = Vector2.zero;
                mrt.anchorMax = Vector2.one;
                mrt.offsetMin = new Vector2(4f, 4f);
                mrt.offsetMax = new Vector2(-5f, -4f);
                var mTmp = manaGO.AddComponent<TextMeshProUGUI>();
                mTmp.text = "";
                mTmp.fontSize = 11f;
                mTmp.fontStyle = FontStyles.Bold;
                mTmp.alignment = TextAlignmentOptions.BottomRight;
                mTmp.color = FontManager.AetherCyanColor;
                FontManager.ApplyFont(mTmp);
                slotUI.manaCostText = mTmp;

                var cdTxtGO = new GameObject("CDText", typeof(RectTransform));
                cdTxtGO.transform.SetParent(root.transform, false);
                var ctrt = cdTxtGO.GetComponent<RectTransform>();
                ctrt.anchorMin = Vector2.zero;
                ctrt.anchorMax = Vector2.one;
                ctrt.offsetMin = Vector2.zero;
                ctrt.offsetMax = Vector2.zero;
                var cTmp = cdTxtGO.AddComponent<TextMeshProUGUI>();
                cTmp.text = "";
                cTmp.fontSize = 15f;
                cTmp.fontStyle = FontStyles.Bold;
                cTmp.alignment = TextAlignmentOptions.Center;
                cTmp.color = Color.white;
                cTmp.enabled = false;
                FontManager.ApplyFont(cTmp);
                slotUI.cdText = cTmp;
            });

            // 4. EquipmentSlot_Prefab.prefab (156x76 px, 44x44 Icon on Left + Word-Wrapped Stats on Right)
            CreateSinglePrefab("Assets/Prefabs/UI/EquipmentSlot_Prefab.prefab", "EquipmentSlot_Prefab", root =>
            {
                var rt = root.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(156f, 76f);

                var img = root.AddComponent<Image>();
                img.sprite = slot9Slice;
                img.type = Image.Type.Sliced;
                img.color = FontManager.SlotEmptyBgColor;

                var outline = root.AddComponent<Outline>();
                outline.effectColor = FontManager.BronzeGoldBorder;
                outline.effectDistance = new Vector2(1f, -1f);
                root.AddComponent<Button>();

                var iconBgGO = new GameObject("Icon_Frame", typeof(RectTransform));
                iconBgGO.transform.SetParent(root.transform, false);
                var ibRt = iconBgGO.GetComponent<RectTransform>();
                ibRt.anchorMin = new Vector2(0f, 0.5f);
                ibRt.anchorMax = new Vector2(0f, 0.5f);
                ibRt.pivot = new Vector2(0f, 0.5f);
                ibRt.anchoredPosition = new Vector2(8f, 0f);
                ibRt.sizeDelta = new Vector2(44f, 44f);
                var ibImg = iconBgGO.AddComponent<Image>();
                ibImg.sprite = slot9Slice;
                ibImg.type = Image.Type.Sliced;
                ibImg.color = new Color(0.05f, 0.06f, 0.09f, 0.95f);

                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(iconBgGO.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = Vector2.zero;
                irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(4f, 4f);
                irt.offsetMax = new Vector2(-4f, -4f);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.color = Color.clear;
                iconImg.enabled = false;

                var txtGO = new GameObject("SlotText", typeof(RectTransform));
                txtGO.transform.SetParent(root.transform, false);
                var trt = txtGO.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(58f, 6f);
                trt.offsetMax = new Vector2(-6f, -6f);
                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "<color=#9E9E9E>[Vũ Khí]\n(Trống)</color>";
                tmp.fontSize = 11f;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
                FontManager.ApplyFont(tmp);
            });
        }

        private static void CreateSinglePrefab(string assetPath, string name, Action<GameObject> builder)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            try
            {
                builder(go);
                FontManager.SanitizeUIHierarchy(go.transform);
                PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void UpgradeActiveSceneUICanvasOnly(GameObject prefabAsset)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            GameObject uiRoot = GameObject.Find("GameUIRoot");
            if (uiRoot == null)
            {
                if (prefabAsset != null)
                {
                    uiRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, scene);
                    uiRoot.name = "GameUIRoot";
                }
            }
            else
            {
                // Chuẩn hóa Canvas & CanvasScaler trên GameUIRoot hiện có mà KHÔNG xóa hay đụng chạm bất kỳ Map object nào
                var canvas = uiRoot.GetComponent<Canvas>() ?? uiRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = uiRoot.GetComponent<CanvasScaler>() ?? uiRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                if (uiRoot.GetComponent<GraphicRaycaster>() == null) uiRoot.AddComponent<GraphicRaycaster>();

                var hud = uiRoot.GetComponentInChildren<GameplayHUD>(true);
                if (hud != null) hud.EnsureUIHierarchy();

                CharacterStatusUI.EnsureUI(uiRoot.transform);
                InventoryBagUI.EnsureUI(uiRoot.transform);
                MinimapSystem2D.EnsureUI(uiRoot.transform);

                var switcher = uiRoot.GetComponentInChildren<ClassDemoSwitcherUI>(true) ?? uiRoot.AddComponent<ClassDemoSwitcherUI>();
                switcher.EnsureUIHierarchy();

                FontManager.SanitizeUIHierarchy(uiRoot.transform);
                EditorUtility.SetDirty(uiRoot);
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
