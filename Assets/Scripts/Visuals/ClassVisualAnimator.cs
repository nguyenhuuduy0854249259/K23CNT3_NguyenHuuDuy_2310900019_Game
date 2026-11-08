using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Classes;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Visuals
{
    [Serializable]
    public class ClassAnimationProfile
    {
        public SpecialMechanic mechanic;
        public string className;
        public Texture2D spriteSheetTexture;
        public Sprite[] idleFrames;
        public Sprite[] walkFrames;
        public Sprite[] attackFrames;
        public Sprite[] castFrames;
    }

    /// <summary>
    /// Tự động cập nhật bộ animation (Idle, Walk, Attack, Cast) cho SpriteAnimator2D
    /// tương ứng với Class và cơ chế đặc trưng mà người chơi đang chọn.
    /// </summary>
    [RequireComponent(typeof(SpriteAnimator2D))]
    public class ClassVisualAnimator : MonoBehaviour
    {
        [Header("Danh Sách Bộ Hoạt Họa Từng Class")]
        public List<ClassAnimationProfile> classProfiles = new List<ClassAnimationProfile>();

        private SpriteAnimator2D animator;
        private ClassSystem classSystem;

        private void Awake()
        {
            animator = GetComponent<SpriteAnimator2D>();
            classSystem = GetComponent<ClassSystem>();
        }

        private void OnEnable()
        {
            EventBus.OnClassChanged += HandleClassChanged;
        }

        private void OnDisable()
        {
            EventBus.OnClassChanged -= HandleClassChanged;
        }

        private void Start()
        {
            if (classSystem != null && classSystem.currentClass != null)
            {
                ApplyClassAnimation(classSystem.currentClass.specialMechanic);
            }
        }

        private void HandleClassChanged(string className)
        {
            if (classSystem != null && classSystem.currentClass != null)
            {
                ApplyClassAnimation(classSystem.currentClass.specialMechanic);
            }
        }

        /// <summary>
        /// Nạp bộ animation frames tương ứng với cơ chế class vào SpriteAnimator2D.
        /// </summary>
        public bool ApplyClassAnimation(SpecialMechanic mechanic)
        {
            ClassAnimationProfile profile = classProfiles.Find(p => p.mechanic == mechanic);
            if (profile == null) return false;

            if (profile.idleFrames != null && profile.idleFrames.Length > 0)
                animator.idleFrames = profile.idleFrames;

            if (profile.walkFrames != null && profile.walkFrames.Length > 0)
                animator.walkFrames = profile.walkFrames;

            if (profile.attackFrames != null && profile.attackFrames.Length > 0)
                animator.attackFrames = profile.attackFrames;

            if (profile.castFrames != null && profile.castFrames.Length > 0)
                animator.castFrames = profile.castFrames;

            animator.PlayAnimation(AnimationState.Idle);
            Debug.Log($"<color=green>[ClassVisualAnimator] Đã cập nhật Animation cho: {mechanic} ({profile.className})</color>");
            return true;
        }
    }
}
