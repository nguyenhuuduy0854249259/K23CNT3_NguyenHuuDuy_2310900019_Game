using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.UI;

namespace ActionRPG.UI.Character
{
    /// <summary>
    /// Main controller for the Character Panel. Handles opening/closing, tab switching and
    /// propagates data to sub‑components (stats, equipment, skills, talent).
    /// </summary>
    public class CharacterUI : MonoBehaviour
    {
        public static CharacterUI Instance { get; private set; }

        [Header("Root Panels (assign in Inspector)")]
        public GameObject panelRoot;               // Whole character UI panel
        public Button closeButton;                 // X button
        public Button[] tabButtons;                // Buttons for each tab
        public GameObject[] tabContents;           // Panels that correspond to each tab, same order as tabButtons

        // Sub‑components (optional – you can assign via inspector or Find in Awake)
        public CharacterStatsUI statsUI;
        public EquipmentUI equipmentUI;
        public SkillsUI skillsUI;
        public TalentUI talentUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            // Hook tab buttons
            if (tabButtons != null)
            {
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    int index = i; // capture
                    if (tabButtons[i] != null)
                        tabButtons[i].onClick.AddListener(() => SwitchTab(index));
                }
            }

            // Auto-find subcomponents if unassigned
            if (statsUI == null) statsUI = GetComponentInChildren<CharacterStatsUI>(true);
            if (equipmentUI == null) equipmentUI = GetComponentInChildren<EquipmentUI>(true);
            if (skillsUI == null) skillsUI = GetComponentInChildren<SkillsUI>(true);
            if (talentUI == null) talentUI = GetComponentInChildren<TalentUI>(true);
        }

        private void Update()
        {
            // Nếu đã có CharacterStatusUI mới thì nhường phím tắt cho CharacterStatusUI xử lý để tránh bật/tắt kép
            if (CharacterStatusUI.Instance != null) return;

            // Phím U: Đóng / Mở Character UI
            if (Input.GetKeyDown(KeyCode.U))
            {
                Toggle();
            }
        }

        private void OnEnable()
        {
            // Refresh data whenever panel opens
            RefreshAll();
        }

        public void Toggle()
        {
            if (panelRoot != null)
            {
                if (panelRoot.activeSelf) Close();
                else Open();
            }
            else if (CharacterStatusUI.Instance != null)
            {
                CharacterStatusUI.Instance.Toggle();
            }
            else if (GetComponent<Canvas>() == null)
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

        public void Open()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            else if (CharacterStatusUI.Instance != null) CharacterStatusUI.Instance.Open();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else if (CharacterStatusUI.Instance != null) CharacterStatusUI.Instance.Close();
        }

        private void SwitchTab(int index)
        {
            for (int i = 0; i < tabContents.Length; i++)
            {
                bool active = i == index;
                if (tabContents[i] != null) tabContents[i].SetActive(active);
                if (tabButtons[i] != null) tabButtons[i].interactable = !active; // optional visual feedback
            }
        }

        /// <summary>
        /// Pull latest data from the player and feed it to UI sub‑components.
        /// </summary>
        public void RefreshAll()
        {
            if (statsUI != null) statsUI.Refresh();
            if (equipmentUI != null) equipmentUI.Refresh();
            if (skillsUI != null) skillsUI.Refresh();
            if (talentUI != null) talentUI.Refresh();
        }
    }
}
