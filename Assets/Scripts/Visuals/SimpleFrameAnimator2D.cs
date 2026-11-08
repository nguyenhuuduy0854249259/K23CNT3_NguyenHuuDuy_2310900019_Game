using UnityEngine;

namespace ActionRPG.Visuals
{
    /// <summary>
    /// Component phát hoạt họa sprite dạng chuỗi khung hình (frame-by-frame) cho đạn (projectile) và hiệu ứng (VFX).
    /// Hỗ trợ lặp (loop) cho đạn bay và phát 1 lần rồi tự hủy (one-shot) cho hiệu ứng nổ.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleFrameAnimator2D : MonoBehaviour
    {
        [Header("Danh Sách Khung Hình")]
        public Sprite[] frames;

        [Header("Cấu Hình Hoạt Họa")]
        [Tooltip("Tốc độ khung hình / giây")]
        public float fps = 12f;

        [Tooltip("Nếu true: lặp lại liên tục; nếu false: phát một lần rồi dừng")]
        public bool loop = true;

        [Tooltip("Tự hủy GameObject khi chạy hết khung hình (chỉ có tác dụng khi loop = false)")]
        public bool destroyOnFinish = false;

        private SpriteRenderer spriteRenderer;
        private float timer = 0f;
        private int currentFrame = 0;
        private bool isFinished = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void OnEnable()
        {
            timer = 0f;
            currentFrame = 0;
            isFinished = false;
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (isFinished || frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(fps, 0.1f);

            if (timer >= frameDuration)
            {
                timer -= frameDuration;
                currentFrame++;

                if (currentFrame >= frames.Length)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        isFinished = true;
                        if (destroyOnFinish)
                        {
                            Destroy(gameObject);
                            return;
                        }
                        else
                        {
                            currentFrame = frames.Length - 1;
                        }
                    }
                }

                if (spriteRenderer != null && currentFrame < frames.Length)
                {
                    spriteRenderer.sprite = frames[currentFrame];
                }
            }
        }

        /// <summary>
        /// Khởi tạo hoạt họa linh hoạt qua code lúc runtime
        /// </summary>
        public void Initialize(Sprite[] animFrames, float animFps, bool isLoop, bool autoDestroy = false)
        {
            frames = animFrames;
            fps = animFps;
            loop = isLoop;
            destroyOnFinish = autoDestroy;
            timer = 0f;
            currentFrame = 0;
            isFinished = false;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[0];
            }
        }
    }
}
