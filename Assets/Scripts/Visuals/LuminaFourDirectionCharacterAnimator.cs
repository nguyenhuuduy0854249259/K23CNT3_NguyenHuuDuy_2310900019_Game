using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Presentation.Views;
using ActionRPG.Classes;
using ActionRPG.Core;
using ActionRPG.Characters;

namespace ActionRPG.Visuals
{
    public enum CharacterDir4
    {
        Down = 0,
        Up = 1,
        Right = 2,
        Left = 3
    }

    public enum CharAnimAction
    {
        Idle,
        Walk,
        Attack,
        Cast
    }

    /// <summary>
    /// Điều khiển hoạt họa 4 hướng (Down, Up, Right, Left) cho 3 Class cơ bản (Warrior, Mage, Archer)
    /// Đảm bảo sprite hiển thị sắc nét, đồng bộ vũ khí quay 4 hướng không bị lệch tâm hay mất chi tiết.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LuminaFourDirectionCharacterAnimator : MonoBehaviour
    {
        [Header("Cấu Hình Class")]
        public string className = "novice"; // "novice", "warrior", "mage", "archer"

        [Header("Tốc Độ Khung Hình (FPS)")]
        public float walkFps = 8f;
        public float idleFps = 4f;

        [Header("Trạng Thái Hiện Tại")]
        public CharacterDir4 currentDirection = CharacterDir4.Down;
        public CharAnimAction currentAction = CharAnimAction.Idle;

        // Bộ khung hình theo 4 hướng [0=Down, 1=Up, 2=Right, 3=Left]
        private Sprite[][] idleFrames = new Sprite[4][];
        private Sprite[][] walkFrames = new Sprite[4][];
        private Sprite[][] attackFrames = new Sprite[4][];
        private Sprite[][] castFrames = new Sprite[4][];

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private WeaponSocketView weaponSocketView;
        private Coroutine activeActionRoutine;
        private bool isActionLocked = false;
        private bool useClassSheet = true;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            weaponSocketView = GetComponentInChildren<WeaponSocketView>();
            if (weaponSocketView == null) weaponSocketView = GetComponent<WeaponSocketView>();

            LoadClassSprites(className);

            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                Sprite fallback = null;
                if (idleFrames != null && idleFrames.Length > 0 && idleFrames[0] != null && idleFrames[0].Length > 0)
                {
                    fallback = idleFrames[0][0];
                }
                if (fallback != null)
                {
                    spriteRenderer.sprite = fallback;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.OnClassApplied += HandleClassApplied;
        }

        private void OnDisable()
        {
            EventBus.OnClassApplied -= HandleClassApplied;
        }

        private void HandleClassApplied(CharacterClassData data)
        {
            if (data != null) ApplyClassBranch(data.branch);
        }

        /// <summary>
        /// Đổi bộ sprite 4 hướng theo hệ phái: Novice/Warrior/Mage/Archer dùng sheet 4 hướng đồng bộ.
        /// </summary>
        public void ApplyClassBranch(ClassBranch branch)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            var legacy = GetComponent<DirectionalWalkAnimator>();

            string cls;
            switch (branch)
            {
                case ClassBranch.Warrior: cls = "warrior"; break;
                case ClassBranch.Mage: cls = "mage"; break;
                case ClassBranch.Archer: cls = "archer"; break;
                case ClassBranch.Novice:
                default:
                    cls = "novice";
                    break;
            }

            LoadClassSprites(cls);
            useClassSheet = true;
            if (legacy != null) legacy.enabled = false;
            enabled = true;

            isActionLocked = false;
            currentAction = CharAnimAction.Idle;
            currentFrameIndex = 0;
            frameTimer = 0f;
            ApplyDirection(currentDirection);
            UpdateSprite();
        }

        private void Start()
        {
            var cs = ClassSystem.Instance;
            if (cs != null && cs.currentClass != null)
            {
                ApplyClassBranch(cs.currentClass.branch);
                return;
            }
            ApplyDirection(currentDirection);
            UpdateSprite();

            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                Sprite[] frames = GetCurrentFrames();
                if (frames != null && frames.Length > 0 && frames[0] != null)
                {
                    spriteRenderer.sprite = frames[0];
                }
            }
        }

        public bool HasFourDirectionSprites => useClassSheet && idleFrames[3] != null && idleFrames[3].Length > 0 && idleFrames[3][0] != null;

        public void SetFacingDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            currentDirection = DetermineDirectionFromVector(dir);
            ApplyDirection(currentDirection);
            UpdateSprite();
        }

        private void Update()
        {
            if (spriteRenderer != null && useClassSheet)
            {
                // Bắt buộc giữ flipX = false vì spritesheet 4 hướng đã có frame Left riêng biệt
                spriteRenderer.flipX = false;
            }

            if (isActionLocked) return;

            // Đọc hướng bấm phím trực tiếp để phản hồi tức thì cả 4 hướng (Trái, Phải, Lên, Xuống)
            float inputX = 0f;
            float inputY = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputY = -1f;
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputY = 1f;

            Vector2 inputVec = new Vector2(inputX, inputY);
            Vector2 vel = rb != null ? rb.linearVelocity : Vector2.zero;
            Vector2 moveVec = inputVec.sqrMagnitude > 0.01f ? inputVec : vel;
            bool isMoving = moveVec.sqrMagnitude > 0.04f;

            if (isMoving)
            {
                CharacterDir4 newDir = DetermineDirectionFromVector(moveVec);
                if (newDir != currentDirection || currentAction != CharAnimAction.Walk)
                {
                    currentDirection = newDir;
                    currentAction = CharAnimAction.Walk;
                    currentFrameIndex = 0;
                    frameTimer = 0f;
                    ApplyDirection(currentDirection);
                    UpdateSprite();
                }
            }
            else
            {
                if (currentAction != CharAnimAction.Idle)
                {
                    currentAction = CharAnimAction.Idle;
                    currentFrameIndex = 0;
                    frameTimer = 0f;
                    ApplyDirection(currentDirection);
                    UpdateSprite();
                }
            }

            // Tiến hành chuyển frame hoạt họa
            AdvanceAnimation();
        }

        private void LateUpdate()
        {
            // Chặn triệt để lỗi lật ngược mặt từ Trái sang Phải do các script cũ can thiệp vào flipX hoặc localScale.x
            if (useClassSheet && spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
                if (transform.localScale.x < 0f)
                {
                    Vector3 s = transform.localScale;
                    s.x = Mathf.Abs(s.x);
                    transform.localScale = s;
                }
            }
        }

        private void AdvanceAnimation()
        {
            Sprite[] frames = GetCurrentFrames();
            if (frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float fps = (currentAction == CharAnimAction.Walk) ? walkFps : idleFps;
            float interval = 1f / Mathf.Max(fps, 1f);

            if (frameTimer >= interval)
            {
                frameTimer -= interval;
                currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            Sprite[] frames = GetCurrentFrames();
            if (frames != null && frames.Length > 0)
            {
                int idx = Mathf.Clamp(currentFrameIndex, 0, frames.Length - 1);
                if (frames[idx] != null)
                {
                    spriteRenderer.sprite = frames[idx];
                }
            }
            if (useClassSheet && spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
        }

        private Sprite[] GetCurrentFrames()
        {
            int dirIdx = Mathf.Clamp((int)currentDirection, 0, 3);
            switch (currentAction)
            {
                case CharAnimAction.Walk: return walkFrames[dirIdx];
                case CharAnimAction.Attack: return attackFrames[dirIdx];
                case CharAnimAction.Cast: return castFrames[dirIdx];
                case CharAnimAction.Idle:
                default: return idleFrames[dirIdx];
            }
        }

        public CharacterDir4 DetermineDirectionFromVector(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return currentDirection;

            // Ưu tiên hướng dọc/ngang rõ rệt (Hỗ trợ đầy đủ 4 hướng: Down, Up, Right, Left)
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            {
                return (dir.x >= 0f) ? CharacterDir4.Right : CharacterDir4.Left;
            }
            else
            {
                return (dir.y >= 0f) ? CharacterDir4.Up : CharacterDir4.Down;
            }
        }

        public void ApplyDirection(CharacterDir4 dir)
        {
            currentDirection = dir;
            // Không lật flipX nếu đã có sprite 4 hướng riêng
            if (spriteRenderer != null) spriteRenderer.flipX = false;

            // Đồng bộ với WeaponSocketView
            if (weaponSocketView != null)
            {
                weaponSocketView.SetFacingDirection4((int)dir, spriteRenderer != null ? spriteRenderer.sortingOrder : 10);
            }
        }

        /// <summary>
        /// Kích hoạt hoạt ảnh tung chiêu (Cast) theo hướng nhắm chuột (4 hướng).
        /// Mặc định KHÔNG tự ý bật vũ khí để từng kỹ năng tự quyết định có hiện vũ khí hay không.
        /// </summary>
        public void TriggerCast(Vector2 aimDir, float duration = 0.35f, bool showWeaponCast = false)
        {
            if (useClassSheet)
            {
                if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
                activeActionRoutine = StartCoroutine(ActionRoutine(CharAnimAction.Cast, aimDir, duration));
            }

            if (weaponSocketView != null && showWeaponCast)
            {
                weaponSocketView.PlayCastAnimation(duration);
            }
        }

        /// <summary>
        /// Kích hoạt hoạt ảnh đánh thường / tấn công (Attack) theo hướng nhắm (4 hướng).
        /// </summary>
        public void TriggerAttack(Vector2 aimDir, float duration = 0.25f, bool showWeaponSlash = false)
        {
            if (useClassSheet)
            {
                if (activeActionRoutine != null) StopCoroutine(activeActionRoutine);
                activeActionRoutine = StartCoroutine(ActionRoutine(CharAnimAction.Attack, aimDir, duration));
            }

            if (weaponSocketView != null && showWeaponSlash)
            {
                weaponSocketView.PlayMeleeSlashAnimation(aimDir, duration);
            }
        }

        private IEnumerator ActionRoutine(CharAnimAction action, Vector2 aimDir, float duration)
        {
            isActionLocked = true;
            currentAction = action;
            if (aimDir.sqrMagnitude > 0.001f)
            {
                currentDirection = DetermineDirectionFromVector(aimDir);
            }
            ApplyDirection(currentDirection);

            Sprite[] frames = GetCurrentFrames();
            int totalFrames = (frames != null && frames.Length > 0) ? frames.Length : 1;
            float step = Mathf.Max(0.02f, duration / totalFrames);

            for (int i = 0; i < totalFrames; i++)
            {
                currentFrameIndex = i;
                UpdateSprite();
                yield return new WaitForSeconds(step);
            }

            isActionLocked = false;
            currentAction = CharAnimAction.Idle;
            currentFrameIndex = 0;
            frameTimer = 0f;
            ApplyDirection(currentDirection);
            UpdateSprite();
            activeActionRoutine = null;
        }

        /// <summary>
        /// Nạp các sprite từ spritesheet 4 hướng của class
        /// </summary>
        public void LoadClassSprites(string clsName)
        {
            className = clsName.ToLower();
            string path = $"player_{className}_4direction";
            Sprite[] allSprites = Resources.LoadAll<Sprite>(path);
            if (allSprites == null || allSprites.Length == 0)
            {
                allSprites = Resources.LoadAll<Sprite>($"Sprites/{path}");
            }

#if UNITY_EDITOR
            if (allSprites == null || allSprites.Length == 0)
            {
                var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath($"Assets/Sprites/{path}.png");
                var list = new List<Sprite>();
                if (objs != null)
                {
                    foreach (var o in objs)
                    {
                        if (o is Sprite s) list.Add(s);
                    }
                }
                allSprites = list.ToArray();
            }
#endif

            if (allSprites == null || allSprites.Length == 0)
            {
                Debug.LogWarning($"[Lumina4Dir] Không tìm thấy sprite cho {path}!");
                return;
            }

            var dict = new Dictionary<string, Sprite>();
            foreach (var s in allSprites)
            {
                dict[s.name] = s;
            }

            string[] dirs = { "down", "up", "right", "left" };
            for (int d = 0; d < 4; d++)
            {
                string dirStr = dirs[d];

                // Idle frames (2 frames)
                var idles = new List<Sprite>();
                for (int i = 0; i < 4; i++)
                {
                    string key = $"{className}_idle_{dirStr}_{i}";
                    if (dict.ContainsKey(key)) idles.Add(dict[key]);
                }
                idleFrames[d] = idles.Count > 0 ? idles.ToArray() : new Sprite[] { dict.ContainsKey($"{className}_idle_{dirStr}_0") ? dict[$"{className}_idle_{dirStr}_0"] : null };

                // Walk frames (6 frames)
                var walks = new List<Sprite>();
                for (int i = 0; i < 8; i++)
                {
                    string key = $"{className}_walk_{dirStr}_{i}";
                    if (dict.ContainsKey(key)) walks.Add(dict[key]);
                }
                walkFrames[d] = walks.Count > 0 ? walks.ToArray() : idleFrames[d];

                // Attack frames (3 frames)
                var attacks = new List<Sprite>();
                for (int i = 0; i < 4; i++)
                {
                    string key = $"{className}_attack_{dirStr}_{i}";
                    if (dict.ContainsKey(key)) attacks.Add(dict[key]);
                }
                attackFrames[d] = attacks.Count > 0 ? attacks.ToArray() : idleFrames[d];

                // Cast frames (2 frames)
                var casts = new List<Sprite>();
                for (int i = 0; i < 4; i++)
                {
                    string key = $"{className}_cast_{dirStr}_{i}";
                    if (dict.ContainsKey(key)) casts.Add(dict[key]);
                }
                castFrames[d] = casts.Count > 0 ? casts.ToArray() : idleFrames[d];
            }
        }
    }
}
