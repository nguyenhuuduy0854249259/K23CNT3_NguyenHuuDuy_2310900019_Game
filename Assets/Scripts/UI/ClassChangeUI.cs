using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Classes;
using ActionRPG.Characters;

namespace ActionRPG.UI
{
    /// <summary>
    /// ClassChangeUI: Cầu nối kiểm tra điều kiện chuyển nghề và mở ClassSelectionUI.
    /// Gọi TryOpenPromotion() khi người chơi nói chuyện với NPC Thầy nghề hoặc đủ cấp độ.
    /// </summary>
    public class ClassChangeUI : MonoBehaviour
    {
        public static ClassChangeUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>
        /// Kiểm tra và mở màn hình chuyển nghề nếu đủ điều kiện.
        /// </summary>
        public void TryOpenPromotion()
        {
            if (ClassSystem.Instance == null) return;

            List<CharacterClassData> options;
            if (ClassSystem.Instance.CanPromote(out options))
            {
                var current = ClassSystem.Instance.currentClass;
                // Tier 0 (Novice → nhánh cơ bản) dùng FirstClassSelection
                // Tier 1 (Nhánh cơ bản → Advanced) dùng AdvancedClassSelection
                bool isAdvanced = current != null && current.tier >= 1;

                var selectionUI = ClassSelectionUI.Instance ?? FindObjectOfType<ClassSelectionUI>(true);
                if (selectionUI == null)
                {
                    GameObject rootGO = GameObject.Find("GameUIRoot");
                    Canvas canvas = rootGO != null ? rootGO.GetComponent<Canvas>() : null;
                    if (canvas == null)
                    {
                        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                        foreach (var c in allCanvases)
                        {
                            if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                            {
                                canvas = c;
                                break;
                            }
                        }
                    }
                    if (canvas != null)
                    {
                        var go = new GameObject("ClassSelectionUI");
                        go.transform.SetParent(canvas.transform, false);
                        selectionUI = go.AddComponent<ClassSelectionUI>();
                    }
                }

                if (selectionUI != null)
                {
                    if (isAdvanced)
                        selectionUI.OpenAdvancedClassSelection(options, current.requiredLevelForPromotion);
                    else
                        selectionUI.OpenFirstClassSelection(options);
                }
                else
                {
                    Debug.LogWarning("[ClassChangeUI] Không tìm thấy Canvas để khởi tạo ClassSelectionUI!");
                }
            }
            else
            {
                var current = ClassSystem.Instance.currentClass;
                int required = current != null ? current.requiredLevelForPromotion : 15;
                Debug.Log($"[ClassChangeUI] Chưa đủ cấp để chuyển nghề. Cần Cấp {required}.");
            }
        }
    }
}
