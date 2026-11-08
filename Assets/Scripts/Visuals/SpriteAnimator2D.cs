using System;
using UnityEngine;

namespace ActionRPG.Visuals
{
    public enum AnimationState
    {
        Idle,
        Walk,
        Attack,
        Cast,
        Hurt,
        Die
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator2D : MonoBehaviour
    {
        [Header("Frames Hoạt Họa (Sprite Arrays)")]
        public Sprite[] idleFrames;
        public Sprite[] walkFrames;
        public Sprite[] attackFrames;
        public Sprite[] castFrames;
        public Sprite[] hurtFrames;

        [Header("Tốc Độ Khung Hình (FPS)")]
        public float fps = 8f;

        public AnimationState currentState = AnimationState.Idle;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Animator unityAnimator;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;
        private bool isLockedAnimation = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            unityAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Nếu GameObject đã có Unity Animator Controller thì nhường quyền điều khiển cho Animator
            if (unityAnimator != null && unityAnimator.runtimeAnimatorController != null)
            {
                return;
            }

            // Tự động chuyển đổi giữa Idle và Walk nếu không bị khóa bởi đòn đánh
            if (!isLockedAnimation && rb != null)
            {
                if (rb.linearVelocity.sqrMagnitude > 0.05f)
                {
                    if (currentState != AnimationState.Walk)
                    {
                        PlayAnimation(AnimationState.Walk);
                    }
                }
                else
                {
                    if (currentState != AnimationState.Idle)
                    {
                        PlayAnimation(AnimationState.Idle);
                    }
                }
            }

            // Cập nhật khung hình
            frameTimer += Time.deltaTime;
            float frameInterval = 1f / fps;

            if (frameTimer >= frameInterval)
            {
                frameTimer -= frameInterval;
                AdvanceFrame();
            }
        }

        public void PlayAnimation(AnimationState newState, bool lockAnim = false)
        {
            currentState = newState;
            isLockedAnimation = lockAnim;
            currentFrameIndex = 0;
            frameTimer = 0f;
            UpdateSprite();
        }

        public void TriggerAttackAnimation(Action onComplete = null)
        {
            PlayAnimation(AnimationState.Attack, true);
        }

        private void AdvanceFrame()
        {
            Sprite[] currentFrames = GetFramesForState(currentState);
            if (currentFrames == null || currentFrames.Length == 0) return;

            currentFrameIndex++;
            if (currentFrameIndex >= currentFrames.Length)
            {
                if (isLockedAnimation)
                {
                    isLockedAnimation = false;
                    PlayAnimation(AnimationState.Idle);
                    return;
                }
                else
                {
                    currentFrameIndex = 0; // Lặp lại (Loop)
                }
            }

            UpdateSprite();
        }

        private void UpdateSprite()
        {
            Sprite[] frames = GetFramesForState(currentState);
            if (frames != null && currentFrameIndex < frames.Length && frames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = frames[currentFrameIndex];
            }
        }

        private Sprite[] GetFramesForState(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Idle: return idleFrames;
                case AnimationState.Walk: return walkFrames;
                case AnimationState.Attack: return attackFrames;
                case AnimationState.Cast: return castFrames;
                case AnimationState.Hurt: return hurtFrames;
                default: return idleFrames;
            }
        }
    }
}
