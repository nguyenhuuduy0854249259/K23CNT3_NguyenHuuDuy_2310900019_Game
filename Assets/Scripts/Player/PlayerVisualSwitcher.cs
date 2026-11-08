using UnityEngine;
using ActionRPG.Classes;
using ActionRPG.Presentation.Views;

namespace ActionRPG.Player
{
    /// <summary>
    /// Component quản lý hoán đổi bộ hoạt ảnh (Animator Controller) của Player theo từng Class:
    /// - Cho phép đổi RuntimeAnimatorController động khi đổi Class (Novice, Warrior, Mage, Archer).
    /// - Tự động duy trì và khôi phục trạng thái di chuyển hiện tại (IsMoving, Speed, Attack trigger).
    /// - Tích hợp chặt chẽ với CharacterVisualView và kiến trúc Clean Architecture của dự án.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerVisualSwitcher : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterVisualView visualView;

        [Header("Class Animator Controllers")]
        [Tooltip("Bộ hoạt ảnh Tập Sự (Novice)")]
        [SerializeField] private RuntimeAnimatorController noviceController;

        [Tooltip("Bộ hoạt ảnh Chiến Binh (Warrior)")]
        [SerializeField] private RuntimeAnimatorController warriorController;

        [Tooltip("Bộ hoạt ảnh Pháp Sư (Mage)")]
        [SerializeField] private RuntimeAnimatorController mageController;

        [Tooltip("Bộ hoạt ảnh Xạ Thủ (Archer)")]
        [SerializeField] private RuntimeAnimatorController archerController;

        [Header("Trạng Thái Hiện Tại")]
        [SerializeField] private ClassBranch currentBranch = ClassBranch.Novice;

        // Hash parameters để tối ưu hiệu năng
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (visualView == null) visualView = GetComponent<CharacterVisualView>();

            // Áp dụng Controller mặc định nếu chưa gán
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                ApplyControllerForBranch(currentBranch, preserveState: false);
            }
        }

        /// <summary>
        /// Hoán đổi bộ hoạt ảnh theo ClassBranch (Novice, Warrior, Mage, Archer)
        /// </summary>
        public void SwitchClassVisual(ClassBranch newBranch)
        {
            currentBranch = newBranch;
            ApplyControllerForBranch(newBranch, preserveState: true);
        }

        /// <summary>
        /// Hoán đổi bộ hoạt ảnh theo tên chuỗi (không phân biệt hoa thường)
        /// </summary>
        public void SwitchClassVisual(string className)
        {
            if (string.IsNullOrEmpty(className)) return;

            string lower = className.Trim().ToLower();
            if (lower.Contains("warrior") || lower.Contains("chienbinh") || lower.Contains("knight"))
            {
                SwitchClassVisual(ClassBranch.Warrior);
            }
            else if (lower.Contains("mage") || lower.Contains("phapsu") || lower.Contains("wizard"))
            {
                SwitchClassVisual(ClassBranch.Mage);
            }
            else if (lower.Contains("archer") || lower.Contains("xathu") || lower.Contains("ranger"))
            {
                SwitchClassVisual(ClassBranch.Archer);
            }
            else
            {
                SwitchClassVisual(ClassBranch.Novice);
            }
        }

        /// <summary>
        /// Kích hoạt hoạt ảnh tấn công trên Animator hiện tại
        /// </summary>
        public void TriggerAttack()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger(AttackTriggerHash);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái di chuyển
        /// </summary>
        public void SetMovingState(bool isMoving, float speed = 0f)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(IsMovingHash, isMoving);
                animator.SetFloat(SpeedHash, speed > 0f ? speed : (isMoving ? 1f : 0f));
            }

            if (visualView != null)
            {
                visualView.SetMovingState(isMoving);
            }
        }

        private void ApplyControllerForBranch(ClassBranch branch, bool preserveState)
        {
            if (animator == null) return;

            // 1. Lưu lại trạng thái hoạt ảnh trước khi đổi controller
            bool wasMoving = false;
            float currentSpeed = 0f;

            if (preserveState && animator.runtimeAnimatorController != null)
            {
                try
                {
                    wasMoving = animator.GetBool(IsMovingHash);
                    currentSpeed = animator.GetFloat(SpeedHash);
                }
                catch
                {
                    // Animator chưa khởi tạo tham số
                }
            }

            // 2. Chọn Controller tương ứng
            RuntimeAnimatorController targetController = null;
            switch (branch)
            {
                case ClassBranch.Warrior:
                    targetController = warriorController;
                    break;
                case ClassBranch.Mage:
                    targetController = mageController;
                    break;
                case ClassBranch.Archer:
                    targetController = archerController;
                    break;
                case ClassBranch.Novice:
                default:
                    targetController = noviceController;
                    break;
            }

            if (targetController == null)
            {
                Debug.LogWarning($"[PlayerVisualSwitcher] Chưa gán Animator Controller cho nhánh '{branch}'.");
                return;
            }

            // 3. Hoán đổi Controller
            animator.runtimeAnimatorController = targetController;

            // 4. Khôi phục lại trạng thái hoạt ảnh
            if (preserveState)
            {
                animator.SetBool(IsMovingHash, wasMoving);
                animator.SetFloat(SpeedHash, currentSpeed);
            }

            Debug.Log($"[PlayerVisualSwitcher] Đã chuyển đổi visual sang Class: {branch} (Controller: {targetController.name}) ✅");
        }

        #region Public Setters / Inspectors
        public void SetNoviceController(RuntimeAnimatorController ctrl) => noviceController = ctrl;
        public void SetWarriorController(RuntimeAnimatorController ctrl) => warriorController = ctrl;
        public void SetMageController(RuntimeAnimatorController ctrl) => mageController = ctrl;
        public void SetArcherController(RuntimeAnimatorController ctrl) => archerController = ctrl;

        public ClassBranch CurrentBranch => currentBranch;
        public RuntimeAnimatorController CurrentController => animator != null ? animator.runtimeAnimatorController : null;
        #endregion

        #region Context Menu Testing
        [ContextMenu("Chuyển sang: Novice")]
        private void TestNovice() => SwitchClassVisual(ClassBranch.Novice);

        [ContextMenu("Chuyển sang: Warrior")]
        private void TestWarrior() => SwitchClassVisual(ClassBranch.Warrior);

        [ContextMenu("Chuyển sang: Mage")]
        private void TestMage() => SwitchClassVisual(ClassBranch.Mage);

        [ContextMenu("Chuyển sang: Archer")]
        private void TestArcher() => SwitchClassVisual(ClassBranch.Archer);
        #endregion
    }
}
