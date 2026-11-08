using UnityEngine;

namespace ActionRPG.Environment
{
    /// <summary>
    /// Phát animation lặp theo danh sách frame sprite (dùng cho thác nước, props động...).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        public Sprite[] frames;
        public float framesPerSecond = 5f;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0 || framesPerSecond <= 0f) return;
            int index = (int)(Time.time * framesPerSecond) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
