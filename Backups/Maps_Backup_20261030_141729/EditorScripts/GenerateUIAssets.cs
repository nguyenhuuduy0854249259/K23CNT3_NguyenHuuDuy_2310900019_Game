#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ActionRPG.UI.Gameplay;
using ActionRPG.UI.Character;
using ActionRPG.Items;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ sinh giao diện tự động cho dự án Lumina:
    /// Menu: Tools -> Generate UI Layout (Canvas + Gameplay HUD + Character Panel).
    /// </summary>
    public class GenerateUIAssets : EditorWindow
    {
        private static GameObject CreateUIGameObject(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        [MenuItem("Tools/Generate UI Layout")]
        public static void GenerateUI()
        {
            // 1. Canvas Root
            GameObject canvasGO = GameObject.Find("GameUIRoot");
            if (canvasGO == null)
            {
                canvasGO = CreateUIGameObject("GameUIRoot");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Ensure EventSystem
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // 2. Gameplay HUD
            Transform existingHud = canvasGO.transform.Find("GameplayHUD");
            GameObject hudObj;
            if (existingHud != null) hudObj = existingHud.gameObject;
            else
            {
                hudObj = CreateUIGameObject("GameplayHUD", canvasGO.transform);
                var rt = hudObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }

            var gameplayHUD = hudObj.GetComponent<GameplayHUD>() ?? hudObj.AddComponent<GameplayHUD>();

            // Top-Left Player Info
            Transform tlTransform = hudObj.transform.Find("TopLeft_PlayerInfo");
            if (tlTransform == null)
            {
                GameObject tlObj = CreateUIGameObject("TopLeft_PlayerInfo", hudObj.transform);
                var tlRt = tlObj.GetComponent<RectTransform>();
                tlRt.anchorMin = new Vector2(0f, 1f);
                tlRt.anchorMax = new Vector2(0f, 1f);
                tlRt.pivot = new Vector2(0f, 1f);
                tlRt.anchoredPosition = new Vector2(25, -25);
                tlRt.sizeDelta = new Vector2(320, 120);

                var bg = tlObj.AddComponent<Image>();
                bg.color = new Color(0.08f, 0.09f, 0.13f, 0.88f);
                var outline = tlObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.2f, 0.6f, 0.9f, 0.5f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                // Name & Level
                var nameTxt = CreateTMPText("PlayerName", tlObj.transform, "Hiệp Sĩ Lumina", 16, TextAlignmentOptions.Left);
                nameTxt.rectTransform.anchoredPosition = new Vector2(15, -15);
                gameplayHUD.playerNameText = nameTxt;

                var lvlTxt = CreateTMPText("LevelText", tlObj.transform, "Lv. 1", 15, TextAlignmentOptions.Right);
                lvlTxt.rectTransform.anchoredPosition = new Vector2(240, -15);
                lvlTxt.color = new Color(1f, 0.85f, 0.2f);
                gameplayHUD.levelText = lvlTxt;

                // Bars: HP, MP, EXP
                gameplayHUD.hpBarFill = CreateBar("HP_Bar", tlObj.transform, new Vector2(15, -45), new Color(0.9f, 0.2f, 0.2f));
                gameplayHUD.hpText = CreateTMPText("HP_Text", gameplayHUD.hpBarFill.transform.parent, "100 / 100", 11, TextAlignmentOptions.Center);
                gameplayHUD.hpText.rectTransform.anchoredPosition = new Vector2(120, 0);

                gameplayHUD.mpBarFill = CreateBar("MP_Bar", tlObj.transform, new Vector2(15, -70), new Color(0.2f, 0.55f, 0.95f));
                gameplayHUD.mpText = CreateTMPText("MP_Text", gameplayHUD.mpBarFill.transform.parent, "50 / 50", 11, TextAlignmentOptions.Center);
                gameplayHUD.mpText.rectTransform.anchoredPosition = new Vector2(120, 0);

                gameplayHUD.expBarFill = CreateBar("EXP_Bar", tlObj.transform, new Vector2(15, -95), new Color(0.3f, 0.85f, 0.4f));
                gameplayHUD.expText = CreateTMPText("EXP_Text", gameplayHUD.expBarFill.transform.parent, "0 / 175", 10, TextAlignmentOptions.Center);
                gameplayHUD.expText.rectTransform.anchoredPosition = new Vector2(120, 0);

                // ===== Top-Left Pet Companion Widget =====
                GameObject petWidgetObj = CreateUIGameObject("TopLeft_PetHUDWidget", tlObj.transform);
                var petRt = petWidgetObj.GetComponent<RectTransform>();
                petRt.anchorMin = new Vector2(0f, 1f);
                petRt.anchorMax = new Vector2(0f, 1f);
                petRt.pivot = new Vector2(0f, 1f);
                petRt.anchoredPosition = new Vector2(15, -125);
                petRt.sizeDelta = new Vector2(250, 75);

                var petBg = petWidgetObj.AddComponent<Image>();
                petBg.color = new Color(0.08f, 0.10f, 0.15f, 0.88f);
                var petOutline = petWidgetObj.AddComponent<Outline>();
                petOutline.effectColor = new Color(1.0f, 0.5f, 0.2f, 0.5f);

                var petWidget = petWidgetObj.AddComponent<PetHUDWidget>();
                petWidget.widgetRoot = petWidgetObj;
                gameplayHUD.petWidget = petWidget;

                // Pet Avatar
                var petAvatarGO = CreateUIGameObject("PetAvatar", petWidgetObj.transform);
                var petAvatarRt = petAvatarGO.GetComponent<RectTransform>();
                petAvatarRt.anchorMin = new Vector2(0f, 0.5f);
                petAvatarRt.anchorMax = new Vector2(0f, 0.5f);
                petAvatarRt.pivot = new Vector2(0f, 0.5f);
                petAvatarRt.anchoredPosition = new Vector2(10, 0);
                petAvatarRt.sizeDelta = new Vector2(50, 50);
                var petAvImg = petAvatarGO.AddComponent<Image>();
                petAvImg.color = new Color(0.2f, 0.2f, 0.25f);
                petWidget.avatarImage = petAvImg;

                // Pet Name & Level
                var petName = CreateTMPText("PetName", petWidgetObj.transform, "Hồ Ly Lửa", 13, TextAlignmentOptions.Left);
                petName.rectTransform.anchoredPosition = new Vector2(70, -14);
                petWidget.petNameText = petName;

                var petLvl = CreateTMPText("PetLevel", petWidgetObj.transform, "Lv. 1", 12, TextAlignmentOptions.Right);
                petLvl.rectTransform.anchoredPosition = new Vector2(195, -14);
                petLvl.color = new Color(1f, 0.85f, 0.3f);
                petWidget.petLevelText = petLvl;

                // Pet HP Bar
                petWidget.hpBarFill = CreateBar("PetHP_Bar", petWidgetObj.transform, new Vector2(70, -36), new Color(0.95f, 0.3f, 0.2f), 170, 10);
                petWidget.hpText = CreateTMPText("PetHP_Text", petWidget.hpBarFill.transform.parent, "90 / 90", 9, TextAlignmentOptions.Center);
                petWidget.hpText.rectTransform.anchoredPosition = new Vector2(85, 0);

                // Pet Bond Bar
                petWidget.bondBarFill = CreateBar("PetBond_Bar", petWidgetObj.transform, new Vector2(70, -52), new Color(1.0f, 0.65f, 0.1f), 170, 8);
                petWidget.bondText = CreateTMPText("PetBond_Text", petWidget.bondBarFill.transform.parent, "Thân Mật: 60/100", 8, TextAlignmentOptions.Center);
                petWidget.bondText.rectTransform.anchoredPosition = new Vector2(85, 0);

                // Pet Element Badge
                var elemBadgeGO = CreateUIGameObject("ElementBadge", petWidgetObj.transform);
                var ebRt = elemBadgeGO.GetComponent<RectTransform>();
                ebRt.anchorMin = new Vector2(0f, 1f);
                ebRt.anchorMax = new Vector2(0f, 1f);
                ebRt.pivot = new Vector2(0f, 1f);
                ebRt.anchoredPosition = new Vector2(42, -5);
                ebRt.sizeDelta = new Vector2(18, 18);
                var ebImg = elemBadgeGO.AddComponent<Image>();
                ebImg.color = new Color(1f, 0.35f, 0.15f);
                petWidget.elementBadgeBg = ebImg;

                var elemTxt = CreateTMPText("ElemText", elemBadgeGO.transform, "Hỏa", 9, TextAlignmentOptions.Center);
                elemTxt.rectTransform.anchoredPosition = Vector2.zero;
                petWidget.elementNameText = elemTxt;

                // Hotkey Hint [T]
                var hint = CreateTMPText("HotkeyHint", petWidgetObj.transform, "[T] Thu Hồi", 9, TextAlignmentOptions.Right);
                hint.rectTransform.anchoredPosition = new Vector2(240, -62);
                hint.color = new Color(0.7f, 0.7f, 0.8f, 0.8f);
                petWidget.hotkeyHintText = hint;
            }

            // Bottom-Right Skill Bar (8 Slots)
            Transform brTransform = hudObj.transform.Find("BottomRight_SkillBar");
            if (brTransform == null)
            {
                GameObject brObj = CreateUIGameObject("BottomRight_SkillBar", hudObj.transform);
                var brRt = brObj.GetComponent<RectTransform>();
                brRt.anchorMin = new Vector2(1f, 0f);
                brRt.anchorMax = new Vector2(1f, 0f);
                brRt.pivot = new Vector2(1f, 0f);
                brRt.anchoredPosition = new Vector2(-25, 25);
                brRt.sizeDelta = new Vector2(500, 60);

                var layout = brObj.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 8;
                layout.childAlignment = TextAnchor.MiddleRight;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                var slots = new List<SkillSlotUI>();
                string[] hotkeys = { "1", "2", "3", "4", "5", "Q", "E", "R" };
                for (int i = 0; i < 8; i++)
                {
                    GameObject slotGO = CreateUIGameObject($"SkillSlot_{i + 1}", brObj.transform);
                    var sRt = slotGO.GetComponent<RectTransform>();
                    sRt.sizeDelta = new Vector2(54, 54);

                    var sImg = slotGO.AddComponent<Image>();
                    sImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
                    var sOutline = slotGO.AddComponent<Outline>();
                    sOutline.effectColor = new Color(0.3f, 0.7f, 1f, 0.6f);

                    var slotUI = slotGO.AddComponent<SkillSlotUI>();

                    // Icon
                    var iconGO = CreateUIGameObject("Icon", slotGO.transform);
                    var iconRt = iconGO.GetComponent<RectTransform>();
                    iconRt.anchorMin = Vector2.zero;
                    iconRt.anchorMax = Vector2.one;
                    iconRt.offsetMin = iconRt.offsetMax = new Vector2(2, -2);
                    var iconImg = iconGO.AddComponent<Image>();
                    iconImg.color = new Color(0.2f, 0.25f, 0.35f, 0.8f);
                    slotUI.iconImage = iconImg;

                    // Cooldown overlay
                    var cdGO = CreateUIGameObject("CooldownOverlay", slotGO.transform);
                    var cdRt = cdGO.GetComponent<RectTransform>();
                    cdRt.anchorMin = Vector2.zero;
                    cdRt.anchorMax = Vector2.one;
                    var cdImg = cdGO.AddComponent<Image>();
                    cdImg.color = new Color(0f, 0f, 0f, 0.65f);
                    cdImg.type = Image.Type.Filled;
                    cdImg.fillMethod = Image.FillMethod.Radial360;
                    cdImg.fillClockwise = false;
                    cdImg.fillAmount = 0f;
                    slotUI.cooldownOverlay = cdImg;

                    // Hotkey label
                    var keyTxt = CreateTMPText("Key", slotGO.transform, hotkeys[i], 11, TextAlignmentOptions.TopLeft);
                    keyTxt.rectTransform.anchorMin = Vector2.zero;
                    keyTxt.rectTransform.anchorMax = Vector2.one;
                    keyTxt.rectTransform.offsetMin = new Vector2(4, 4);
                    keyTxt.rectTransform.offsetMax = new Vector2(-4, -4);
                    keyTxt.color = new Color(1f, 0.9f, 0.3f);
                    slotUI.shortcutKeyText = keyTxt;

                    slots.Add(slotUI);
                }
                gameplayHUD.skillSlots = slots.ToArray();
            }

            // Bottom-Left Menu cluster
            Transform blTransform = hudObj.transform.Find("BottomLeft_MenuCluster");
            if (blTransform == null)
            {
                GameObject blObj = CreateUIGameObject("BottomLeft_MenuCluster", hudObj.transform);
                var blRt = blObj.GetComponent<RectTransform>();
                blRt.anchorMin = new Vector2(0f, 0f);
                blRt.anchorMax = new Vector2(0f, 0f);
                blRt.pivot = new Vector2(0f, 0f);
                blRt.anchoredPosition = new Vector2(25, 25);
                blRt.sizeDelta = new Vector2(560, 45);

                var layout = blObj.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 6;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                gameplayHUD.characterButton = CreateMenuButton("Btn_Character", blObj.transform, "Nhân Vật [U]");
                gameplayHUD.inventoryButton = CreateMenuButton("Btn_Inventory", blObj.transform, "Túi Đồ [I]");
                gameplayHUD.equipmentButton = CreateMenuButton("Btn_Equipment", blObj.transform, "Trang Bị");
                gameplayHUD.skillsButton = CreateMenuButton("Btn_Skills", blObj.transform, "Kỹ Năng");
                gameplayHUD.demoClassButton = CreateMenuButton("Btn_DemoClass", blObj.transform, "⚔ Đổi Class [F1]");
                gameplayHUD.menuButton = CreateMenuButton("Btn_Menu", blObj.transform, "Cài Đặt [Esc]");
            }

            // 3. Character UI Panel
            Transform existingChar = canvasGO.transform.Find("CharacterPanel");
            GameObject charObj;
            if (existingChar != null) charObj = existingChar.gameObject;
            else
            {
                charObj = CreateUIGameObject("CharacterPanel", canvasGO.transform);
                var cRt = charObj.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0.5f);
                cRt.anchorMax = new Vector2(0.5f, 0.5f);
                cRt.pivot = new Vector2(0.5f, 0.5f);
                cRt.sizeDelta = new Vector2(850, 580);

                var bg = charObj.AddComponent<Image>();
                bg.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);
                var outline = charObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.7f, 0.55f, 0.95f, 0.75f);
                outline.effectDistance = new Vector2(2, -2);
            }

            var charUI = charObj.GetComponent<CharacterUI>() ?? charObj.AddComponent<CharacterUI>();
            charUI.panelRoot = charObj;

            // Ensure Sub-panels & Components
            if (charObj.GetComponentInChildren<CharacterStatsUI>(true) == null)
            {
                var statsObj = CreateUIGameObject("TabContent_Stats", charObj.transform);
                var sRt = statsObj.GetComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(20, 20);
                sRt.offsetMax = new Vector2(-20, -70);
                charUI.statsUI = statsObj.AddComponent<CharacterStatsUI>();
            }

            if (charObj.GetComponentInChildren<EquipmentUI>(true) == null)
            {
                var equipObj = CreateUIGameObject("TabContent_Equipment", charObj.transform);
                var eRt = equipObj.GetComponent<RectTransform>();
                eRt.anchorMin = Vector2.zero;
                eRt.anchorMax = Vector2.one;
                eRt.offsetMin = new Vector2(20, 20);
                eRt.offsetMax = new Vector2(-20, -70);
                equipObj.SetActive(false);
                charUI.equipmentUI = equipObj.AddComponent<EquipmentUI>();
            }

            if (charObj.GetComponentInChildren<SkillsUI>(true) == null)
            {
                var skillObj = CreateUIGameObject("TabContent_Skills", charObj.transform);
                var skRt = skillObj.GetComponent<RectTransform>();
                skRt.anchorMin = Vector2.zero;
                skRt.anchorMax = Vector2.one;
                skRt.offsetMin = new Vector2(20, 20);
                skRt.offsetMax = new Vector2(-20, -70);
                skillObj.SetActive(false);
                charUI.skillsUI = skillObj.AddComponent<SkillsUI>();
            }

            if (charObj.GetComponentInChildren<TalentUI>(true) == null)
            {
                var talentObj = CreateUIGameObject("TabContent_Talent", charObj.transform);
                var tRt = talentObj.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = new Vector2(20, 20);
                tRt.offsetMax = new Vector2(-20, -70);
                talentObj.SetActive(false);
                charUI.talentUI = talentObj.AddComponent<TalentUI>();
            }

            // Close Button
            Transform closeBtnTrans = charObj.transform.Find("CloseButton");
            if (closeBtnTrans == null)
            {
                var closeGO = CreateUIGameObject("CloseButton", charObj.transform);
                var clRt = closeGO.GetComponent<RectTransform>();
                clRt.anchorMin = new Vector2(1f, 1f);
                clRt.anchorMax = new Vector2(1f, 1f);
                clRt.pivot = new Vector2(1f, 1f);
                clRt.anchoredPosition = new Vector2(-15, -15);
                clRt.sizeDelta = new Vector2(36, 36);

                var clImg = closeGO.AddComponent<Image>();
                clImg.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);
                var clBtn = closeGO.AddComponent<Button>();
                var txt = CreateTMPText("X", closeGO.transform, "X", 18, TextAlignmentOptions.Center);
                txt.rectTransform.anchorMin = Vector2.zero;
                txt.rectTransform.anchorMax = Vector2.one;
                charUI.closeButton = clBtn;
                clBtn.onClick.AddListener(() => charUI.Close());
            }

            // Character UI starts closed by default
            charObj.SetActive(false);

            // 4. Class Demo Switcher UI
            Transform existingSwitcher = canvasGO.transform.Find("ClassDemoSwitcher");
            if (existingSwitcher == null)
            {
                var switcherObj = CreateUIGameObject("ClassDemoSwitcher", canvasGO.transform);
                switcherObj.AddComponent<ActionRPG.UI.ClassDemoSwitcherUI>();
            }

            // 5. Pause Menu UI
            Transform existingPause = canvasGO.transform.Find("PauseMenu");
            if (existingPause == null)
            {
                var pauseObj = CreateUIGameObject("PauseMenu", canvasGO.transform);
                pauseObj.AddComponent<ActionRPG.UI.PauseMenuUI>();
            }

            EditorUtility.SetDirty(canvasGO);
            Debug.Log("<color=green>[Lumina UI] Đã tạo thành công bộ Canvas, GameplayHUD, Character UI, Class Demo Switcher [F1] và Pause Menu [Esc]!</color>");
        }

        private static Image CreateBar(string name, Transform parent, Vector2 pos, Color fillColor, float width = 240, float height = 18)
        {
            var barGO = CreateUIGameObject(name, parent);
            var rt = barGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);

            var bg = barGO.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

            var fillGO = CreateUIGameObject("Fill", barGO.transform);
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;

            return fillImg;
        }

        private static Button CreateMenuButton(string name, Transform parent, string label)
        {
            var btnGO = CreateUIGameObject(name, parent);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(85, 38);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.12f, 0.15f, 0.2f, 0.92f);
            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.6f, 0.85f, 0.5f);

            var btn = btnGO.AddComponent<Button>();
            var txt = CreateTMPText("Label", btnGO.transform, label, 12, TextAlignmentOptions.Center);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;

            return btn;
        }

        private static TMP_Text CreateTMPText(string name, Transform parent, string content, float size, TextAlignmentOptions align)
        {
            var go = CreateUIGameObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }
    }
}
#endif
