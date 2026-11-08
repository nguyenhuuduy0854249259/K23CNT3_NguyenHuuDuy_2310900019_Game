using UnityEngine;
using ActionRPG.Items;
using ActionRPG.Core;
using ActionRPG.Classes;
using System.Collections;
using System.Collections.Generic;

namespace ActionRPG.Presentation.Views
{
    /// <summary>
    /// Controller cầm nắm & vung vũ khí (Kiếm / Gậy Phép / Cung Tên) hỗ trợ đầy đủ 4 hướng & 360 độ theo chuột.
    /// </summary>
    public class WeaponSocketView : MonoBehaviour
    {
        [Header("Components")]
        public SpriteRenderer weaponRenderer;
        public Transform weaponHolder; // Điểm cầm vũ khí

        [Header("Dependencies")]
        [SerializeField] private CharacterVisualView characterVisualView;

        private float defaultRotationAngle = 0f;
        private bool isAttacking = false;
        private Coroutine attackCoroutine;
        private System.Action<Vector3> pendingReleaseCallback;
        private Vector3 pendingReleaseWorldPos;

        private SpriteRenderer staffTipGlowRenderer;
        private SpriteRenderer staffTipCoreRenderer;
        private SpriteRenderer nockedArrowRenderer;

        private EquipmentData currentEquippedWeapon;
        private int currentFacingDir = 0; // 0=Down, 1=Up, 2=Right, 3=Left

        // Vũ khí mặc định theo class (Kiếm / Gậy / Cung)
        private EquipmentData classWeapon;
        private ClassBranch classWeaponBranch = ClassBranch.Novice;
        private static readonly Dictionary<ClassBranch, EquipmentData> classWeaponCache = new Dictionary<ClassBranch, EquipmentData>();
        private static readonly Dictionary<string, Sprite> proceduralWeaponSprites = new Dictionary<string, Sprite>();
        private static Sprite proceduralCircleSprite;
        private static Sprite proceduralArrowSprite;

        private void Awake()
        {
            if (characterVisualView == null)
                characterVisualView = GetComponentInParent<CharacterVisualView>();
            EnsureWeaponObjects();
            EnsureSubEffectRenderers();
            HideSubEffects();
            HideWeapon();
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentChanged += UpdateWeaponVisual;
            EventBus.OnClassApplied += HandleClassApplied;
            if (characterVisualView != null)
                characterVisualView.OnFlipX += UpdateFlip;
            HideSubEffects();
            HideWeapon();
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentChanged -= UpdateWeaponVisual;
            EventBus.OnClassApplied -= HandleClassApplied;
            if (characterVisualView != null)
                characterVisualView.OnFlipX -= UpdateFlip;

            FlushPendingCallback();
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (isAttacking) ResetHolderToRestPose();
            isAttacking = false;
            HideSubEffects();
            HideWeapon();
        }

        private void Start()
        {
            var cs = ActionRPG.Characters.ClassSystem.Instance;
            if (cs == null) cs = GetComponentInParent<ActionRPG.Characters.ClassSystem>();
            if (cs != null && cs.currentClass != null)
                ApplyClassWeapon(cs.currentClass.branch);
            else
                ApplyClassWeapon(ClassBranch.Novice);
        }

        private void LateUpdate()
        {
            if (!isAttacking) return;

            // Đảm bảo khi đang vung vũ khí / giương cung / giơ gậy phép thì vũ khí luôn hiện rõ trên lớp trên cùng
            int baseOrder = GetBaseSortingOrder();
            int layerId = GetBaseSortingLayerID();

            if (weaponRenderer != null)
            {
                if (weaponRenderer.sprite == null)
                {
                    weaponRenderer.sprite = GetProceduralWeaponSprite(WeaponFileKey(classWeaponBranch));
                }
                weaponRenderer.enabled = true;
                weaponRenderer.sortingLayerID = layerId;
                weaponRenderer.sortingOrder = baseOrder + 12;
            }

            if (nockedArrowRenderer != null && nockedArrowRenderer.enabled)
            {
                nockedArrowRenderer.sortingLayerID = layerId;
                nockedArrowRenderer.sortingOrder = baseOrder + 14;
            }

            if (staffTipGlowRenderer != null && staffTipGlowRenderer.enabled)
            {
                staffTipGlowRenderer.sortingLayerID = layerId;
                staffTipGlowRenderer.sortingOrder = baseOrder + 15;
            }

            if (staffTipCoreRenderer != null && staffTipCoreRenderer.enabled)
            {
                staffTipCoreRenderer.sortingLayerID = layerId;
                staffTipCoreRenderer.sortingOrder = baseOrder + 16;
            }
        }

        private int GetBaseSortingOrder()
        {
            SpriteRenderer parentSr = GetComponent<SpriteRenderer>();
            if (parentSr == null) parentSr = GetComponentInParent<SpriteRenderer>();
            int order = parentSr != null ? parentSr.sortingOrder : 5010;
            return Mathf.Max(order, 5010);
        }

        private int GetBaseSortingLayerID()
        {
            SpriteRenderer parentSr = GetComponent<SpriteRenderer>();
            if (parentSr == null) parentSr = GetComponentInParent<SpriteRenderer>();
            return parentSr != null ? parentSr.sortingLayerID : 0;
        }

        private void HideWeapon()
        {
            if (weaponRenderer != null) weaponRenderer.enabled = false;
        }

        private void ShowWeapon()
        {
            if (weaponRenderer != null)
            {
                if (weaponRenderer.sprite == null)
                {
                    weaponRenderer.sprite = GetProceduralWeaponSprite(WeaponFileKey(classWeaponBranch));
                }
                weaponRenderer.color = Color.white;
                weaponRenderer.sortingLayerID = GetBaseSortingLayerID();
                weaponRenderer.sortingOrder = GetBaseSortingOrder() + 12;
                weaponRenderer.enabled = true;
            }
        }

        private void FlushPendingCallback()
        {
            if (pendingReleaseCallback != null)
            {
                var cb = pendingReleaseCallback;
                Vector3 pos = pendingReleaseWorldPos;
                pendingReleaseCallback = null;
                cb.Invoke(pos);
            }
        }

        /// <summary>Trả Weapon_Holder về vị trí cầm / góc nghỉ theo hướng hiện tại.</summary>
        private void ResetHolderToRestPose()
        {
            if (weaponHolder == null) return;
            Vector2 grip = GetGripOffsetForDirection(currentFacingDir);
            weaponHolder.localPosition = new Vector3(grip.x, grip.y, 0f);
            weaponHolder.localScale = Vector3.one;
            weaponHolder.localRotation = Quaternion.Euler(0, 0, defaultRotationAngle);
        }

        /// <summary>Dừng đòn cũ (nếu đang spam) và trả vũ khí về tư thế nghỉ trước khi chạy đòn mới.</summary>
        private void StopCurrentAttack()
        {
            FlushPendingCallback();
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
                ResetHolderToRestPose();
            }
            isAttacking = false;
        }

        /// <summary>Tự tạo Weapon_Holder / Weapon_Renderer nếu prefab người chơi chưa gán.</summary>
        private void EnsureWeaponObjects()
        {
            if (weaponHolder == null)
            {
                Transform existing = transform.Find("Weapon_Holder");
                if (existing != null)
                {
                    weaponHolder = existing;
                }
                else
                {
                    GameObject go = new GameObject("Weapon_Holder");
                    go.transform.SetParent(transform, false);
                    weaponHolder = go.transform;
                }
            }

            if (weaponRenderer == null && weaponHolder != null)
            {
                Transform wrTrans = weaponHolder.Find("Weapon_Renderer");
                if (wrTrans != null)
                {
                    weaponRenderer = wrTrans.GetComponent<SpriteRenderer>();
                    if (weaponRenderer == null)
                        weaponRenderer = wrTrans.gameObject.AddComponent<SpriteRenderer>();
                }
                else
                {
                    GameObject go = new GameObject("Weapon_Renderer");
                    go.transform.SetParent(weaponHolder, false);
                    weaponRenderer = go.AddComponent<SpriteRenderer>();
                }

                weaponRenderer.sortingLayerID = GetBaseSortingLayerID();
                weaponRenderer.sortingOrder = GetBaseSortingOrder() + 12;
            }
        }

        private void EnsureSubEffectRenderers()
        {
            EnsureWeaponObjects();
            if (weaponHolder == null) return;

            // tuyệt đối KHÔNG dùng toán tử ?? trên GetComponent vì Unity trả về fake-null gây MissingComponentException!
            if (staffTipGlowRenderer == null)
            {
                Transform t = weaponHolder.Find("StaffTip_Glow");
                if (t == null)
                {
                    GameObject go = new GameObject("StaffTip_Glow");
                    go.transform.SetParent(weaponHolder, false);
                    t = go.transform;
                }
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
                staffTipGlowRenderer = sr;
                staffTipGlowRenderer.sprite = GetProceduralCircleSprite();
                staffTipGlowRenderer.enabled = false;
            }

            if (staffTipCoreRenderer == null && staffTipGlowRenderer != null)
            {
                Transform t = staffTipGlowRenderer.transform.Find("StaffTip_Core");
                if (t == null)
                {
                    GameObject go = new GameObject("StaffTip_Core");
                    go.transform.SetParent(staffTipGlowRenderer.transform, false);
                    t = go.transform;
                }
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
                staffTipCoreRenderer = sr;
                staffTipCoreRenderer.sprite = GetProceduralCircleSprite();
                staffTipCoreRenderer.enabled = false;
            }

            if (nockedArrowRenderer == null)
            {
                Transform t = weaponHolder.Find("Bow_NockedArrow");
                if (t == null)
                {
                    GameObject go = new GameObject("Bow_NockedArrow");
                    go.transform.SetParent(weaponHolder, false);
                    t = go.transform;
                }
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
                nockedArrowRenderer = sr;
                nockedArrowRenderer.sprite = GetNockedArrowSprite();
                nockedArrowRenderer.enabled = false;
            }
        }

        private void HideSubEffects()
        {
            if (staffTipGlowRenderer != null) staffTipGlowRenderer.enabled = false;
            if (staffTipCoreRenderer != null) staffTipCoreRenderer.enabled = false;
            if (nockedArrowRenderer != null) nockedArrowRenderer.enabled = false;
        }

        /// <summary>
        /// Dừng mọi hoạt ảnh vũ khí và ẩn vũ khí ngay lập tức (dùng cho các kỹ năng không hiện vũ khí).
        /// </summary>
        public void StopAndHideWeapon()
        {
            StopCurrentAttack();
            HideSubEffects();
            HideWeapon();
        }

        private void HandleClassApplied(CharacterClassData data)
        {
            if (data != null) ApplyClassWeapon(data.branch);
        }

        /// <summary>Đổi vũ khí theo hệ phái: Warrior/Novice = Kiếm, Mage = Gậy, Archer = Cung.</summary>
        public void ApplyClassWeapon(ClassBranch branch)
        {
            EnsureWeaponObjects();
            classWeaponBranch = branch;
            classWeapon = GetOrBuildClassWeapon(branch);
            currentEquippedWeapon = classWeapon;
            if (!isAttacking)
            {
                ApplyWeaponDirection(currentFacingDir, GetBaseSortingOrder());
            }
            else if (weaponRenderer != null && classWeapon != null)
            {
                weaponRenderer.sprite = classWeapon.visualSprite;
            }
        }

        private static string WeaponFileKey(ClassBranch branch)
        {
            switch (branch)
            {
                case ClassBranch.Warrior: return "weapon_warrior_sword";
                case ClassBranch.Mage: return "weapon_mage_staff";
                case ClassBranch.Archer: return "weapon_archer_bow";
                case ClassBranch.Novice:
                default:
                    return "weapon_warrior_sword";
            }
        }

        /// <summary>
        /// Nạp Sprite Vũ Khí Pixel Art sắc nét từ codeart2d (24x16 cho Kiếm, 16x24 thẳng đứng cho Gậy Phép, 16x22 cho Cung ở 32 PPU),
        /// tỷ lệ vừa vặn với thân nhân vật 14x24px, có viền đậm #1a1c2c.
        /// </summary>
        private static Sprite GetProceduralWeaponSprite(string key)
        {
            if (proceduralWeaponSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

            Vector2 pivot;
            if (key.Contains("staff"))
                pivot = new Vector2(0.50f, 8f / 24f); // Gậy phép thẳng đứng 16x24, tay cầm ở y_bottom = 8, đỉnh ngọc ở y_bottom = 19
            else if (key.Contains("bow"))
                pivot = new Vector2(0.50f, 0.50f);
            else
                pivot = new Vector2(0.25f, 0.50f);

            Sprite codeArtSpr = ActionRPG.Visuals.CodeArtSpriteBank.LoadSprite(key, pivot, 32f);
            if (codeArtSpr != null)
            {
                proceduralWeaponSprites[key] = codeArtSpr;
                return codeArtSpr;
            }

            // Fallback PixelSpec thu gọn sắc nét nếu chạy trong môi trường không có file đĩa
            int w = (key.Contains("bow") || key.Contains("staff")) ? 16 : 24;
            int h = key.Contains("staff") ? 24 : (key.Contains("bow") ? 22 : 16);
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, Color.clear);

            int midY = h / 2;
            Color outline = new Color(0.10f, 0.11f, 0.17f, 1f);

            if (key.Contains("bow"))
            {
                Color woodMain = new Color(0.85f, 0.63f, 0.40f, 1f);
                Color goldTrim = new Color(1f, 0.80f, 0.46f, 1f);
                Color bowString = new Color(0.58f, 0.69f, 0.76f, 1f);
                int stringX = 5;
                for (int y = 3; y <= 18; y++)
                {
                    tex.SetPixel(stringX, y, bowString);
                    float normY = (y - 3f) / 15f;
                    int bowX = stringX + Mathf.RoundToInt(Mathf.Sin(normY * Mathf.PI) * 5f);
                    tex.SetPixel(bowX - 1, y, outline);
                    tex.SetPixel(bowX, y, (Mathf.Abs(y - midY) <= 2 || y <= 4 || y >= 17) ? goldTrim : woodMain);
                    tex.SetPixel(bowX + 1, y, outline);
                }
            }
            else if (key.Contains("staff"))
            {
                int midX = w / 2;
                Color shaftWood = new Color(0.56f, 0.34f, 0.23f, 1f);
                Color goldHead = new Color(1f, 0.80f, 0.46f, 1f);
                Color crystalOrb = new Color(0.45f, 0.94f, 0.97f, 1f);
                for (int y = 4; y <= 16; y++)
                {
                    tex.SetPixel(midX - 1, y, outline);
                    tex.SetPixel(midX, y, (y % 4 == 0) ? goldHead : shaftWood);
                    tex.SetPixel(midX + 1, y, outline);
                }
                for (int dy = -2; dy <= 2; dy++)
                    for (int dx = -2; dx <= 2; dx++)
                        if (dx * dx + dy * dy <= 5)
                            tex.SetPixel(midX + dx, 19 + dy, (dx == 0 && dy == 0) ? Color.white : crystalOrb);
            }
            else
            {
                Color hiltColor = new Color(0.69f, 0.24f, 0.33f, 1f);
                Color goldGuard = new Color(1f, 0.80f, 0.46f, 1f);
                Color bladeEdge = new Color(0.96f, 0.96f, 0.96f, 1f);
                Color bladeCore = new Color(0.45f, 0.94f, 0.97f, 1f);
                for (int x = 2; x <= 6; x++)
                {
                    tex.SetPixel(x, midY - 1, outline);
                    tex.SetPixel(x, midY, (x <= 3) ? goldGuard : hiltColor);
                    tex.SetPixel(x, midY + 1, outline);
                }
                for (int y = midY - 3; y <= midY + 3; y++)
                {
                    tex.SetPixel(7, y, goldGuard);
                    tex.SetPixel(8, y, goldGuard);
                }
                for (int x = 9; x <= 20; x++)
                {
                    int halfW = (x < 18) ? 1 : 0;
                    for (int dy = -halfW - 1; dy <= halfW + 1; dy++)
                    {
                        if (Mathf.Abs(dy) == halfW + 1) tex.SetPixel(x, midY + dy, outline);
                        else tex.SetPixel(x, midY + dy, dy == 0 ? bladeCore : bladeEdge);
                    }
                }
            }

            tex.Apply();
            Sprite spr = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, 32f);
            proceduralWeaponSprites[key] = spr;
            return spr;
        }

        private static EquipmentData GetOrBuildClassWeapon(ClassBranch branch)
        {
            string key = WeaponFileKey(branch);
            if (classWeaponCache.TryGetValue(branch, out EquipmentData cached) && cached != null && cached.spriteDown != null)
                return cached;

            Sprite unifiedWeaponSprite = GetProceduralWeaponSprite(key);

            EquipmentData data = ScriptableObject.CreateInstance<EquipmentData>();
            data.name = key;
            data.spriteDown = unifiedWeaponSprite;
            data.spriteUp = unifiedWeaponSprite;
            data.spriteRight = unifiedWeaponSprite;
            data.spriteLeft = unifiedWeaponSprite;
            data.visualSprite = unifiedWeaponSprite;
            data.weaponGripOffset = Vector2.zero;
            data.weaponRotationAngle = 0f;

            classWeaponCache[branch] = data;
            return data;
        }

        private static readonly Dictionary<ClassBranch, Vector2[]> classBranchGripOffsets = new Dictionary<ClassBranch, Vector2[]>
        {
            {
                ClassBranch.Warrior, new Vector2[]
                {
                    new Vector2(0.12f, -0.06f),  // Down
                    new Vector2(-0.10f, 0.04f),  // Up
                    new Vector2(0.15f, -0.04f),  // Right
                    new Vector2(-0.15f, -0.04f)  // Left
                }
            },
            {
                ClassBranch.Mage, new Vector2[]
                {
                    new Vector2(0.12f, -0.03f),  // Down
                    new Vector2(-0.12f, 0.05f),  // Up
                    new Vector2(0.16f, 0.00f),   // Right
                    new Vector2(-0.16f, 0.00f)   // Left
                }
            },
            {
                ClassBranch.Archer, new Vector2[]
                {
                    new Vector2(0.11f, -0.04f),  // Down
                    new Vector2(-0.11f, 0.03f),  // Up
                    new Vector2(0.15f, -0.03f),  // Right
                    new Vector2(-0.15f, -0.03f)  // Left
                }
            },
            {
                ClassBranch.Novice, new Vector2[]
                {
                    new Vector2(0.12f, -0.06f),  // Down
                    new Vector2(-0.10f, 0.04f),  // Up
                    new Vector2(0.15f, -0.04f),  // Right
                    new Vector2(-0.15f, -0.04f)  // Left
                }
            }
        };

        private Vector2 GetGripOffsetForDirection(int dir)
        {
            int clampDir = Mathf.Clamp(dir, 0, 3);
            if (classBranchGripOffsets.TryGetValue(classWeaponBranch, out Vector2[] offsets))
            {
                return offsets[clampDir];
            }
            return new Vector2(0.14f, -0.05f);
        }

        public void UpdateWeaponVisual()
        {
            EnsureWeaponObjects();

            if (classWeapon != null)
            {
                currentEquippedWeapon = classWeapon;
                if (!isAttacking) ApplyWeaponDirection(currentFacingDir, GetBaseSortingOrder());
                return;
            }

            if (Inventory.Instance != null)
            {
                var equipped = Inventory.Instance.equippedItems;
                if (equipped != null && equipped.TryGetValue(EquipSlot.Weapon, out EquipmentData weaponData) && weaponData != null)
                {
                    currentEquippedWeapon = weaponData;
                    if (!isAttacking) ApplyWeaponDirection(currentFacingDir, GetBaseSortingOrder());
                    return;
                }
            }

            ApplyClassWeapon(classWeaponBranch);
        }

        /// <summary>
        /// Cập nhật hướng nhìn 4 chiều cho vũ khí (0: Down, 1: Up, 2: Right, 3: Left).
        /// Khi đang thực hiện đòn đánh (isAttacking == true), không ghi đè vị trí/xoay của Coroutine tấn công.
        /// </summary>
        public void SetFacingDirection4(int dir, int baseSortingOrder = 10)
        {
            currentFacingDir = Mathf.Clamp(dir, 0, 3);
            if (isAttacking) return;
            ApplyWeaponDirection(currentFacingDir, Mathf.Max(baseSortingOrder, GetBaseSortingOrder()));
        }

        private void ApplyWeaponDirection(int dir, int baseSortingOrder = 10)
        {
            if (currentEquippedWeapon == null || weaponRenderer == null) return;

            Sprite spr = currentEquippedWeapon.visualSprite;
            if (spr == null) spr = currentEquippedWeapon.spriteDown;
            if (spr == null) spr = GetProceduralWeaponSprite(WeaponFileKey(classWeaponBranch));

            weaponRenderer.sprite = spr;
            weaponRenderer.sortingLayerID = GetBaseSortingLayerID();
            weaponRenderer.sortingOrder = Mathf.Max(baseSortingOrder, 5010) + 12;

            if (!isAttacking)
            {
                weaponRenderer.enabled = false;
                if (weaponHolder != null)
                {
                    Vector2 grip = GetGripOffsetForDirection(dir);
                    weaponHolder.localPosition = new Vector3(grip.x, grip.y, 0f);
                    weaponHolder.localScale = Vector3.one;
                    defaultRotationAngle = currentEquippedWeapon.weaponRotationAngle;
                    weaponHolder.localRotation = Quaternion.Euler(0, 0, defaultRotationAngle);
                }
            }
        }

        private static Sprite GetProceduralCircleSprite()
        {
            if (proceduralCircleSprite != null) return proceduralCircleSprite;
            int size = 12;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= 2.4f)
                        tex.SetPixel(x, y, Color.white);
                    else if (d <= 4.3f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.88f));
                    else if (d <= 5.2f)
                        tex.SetPixel(x, y, new Color(0.10f, 0.11f, 0.17f, 0.95f));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            proceduralCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return proceduralCircleSprite;
        }

        private static Sprite GetNockedArrowSprite()
        {
            if (proceduralArrowSprite != null) return proceduralArrowSprite;

            Sprite codeArtArrow = ActionRPG.Visuals.CodeArtSpriteBank.LoadSprite("archer_arrow", new Vector2(0.5f, 0.5f), 32f);
            if (codeArtArrow != null)
            {
                proceduralArrowSprite = codeArtArrow;
                return proceduralArrowSprite;
            }

            int w = 16, h = 9;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, Color.clear);

            int midY = 4;
            Color outline = new Color(0.10f, 0.11f, 0.17f, 1f);
            Color shaftGold = new Color(1f, 0.80f, 0.46f, 1f);
            Color headSteel = Color.white;
            Color feather = new Color(0.22f, 0.72f, 0.39f, 1f);

            for (int x = 3; x <= 11; x++)
            {
                tex.SetPixel(x, midY + 1, outline);
                tex.SetPixel(x, midY, shaftGold);
                tex.SetPixel(x, midY - 1, outline);
            }
            tex.SetPixel(11, midY + 1, new Color(0.45f, 0.94f, 0.97f, 1f));
            tex.SetPixel(11, midY - 1, new Color(0.45f, 0.94f, 0.97f, 1f));
            tex.SetPixel(12, midY, headSteel);
            tex.SetPixel(13, midY, headSteel);
            tex.SetPixel(14, midY, outline);
            tex.SetPixel(2, midY + 1, feather);
            tex.SetPixel(3, midY + 1, feather);
            tex.SetPixel(2, midY - 1, feather);
            tex.SetPixel(3, midY - 1, feather);

            tex.Apply();
            proceduralArrowSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
            return proceduralArrowSprite;
        }

        private void UpdateFlip(bool isFlipped)
        {
            var fourDir = GetComponentInParent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
            if (fourDir != null && fourDir.enabled) return;
            SetFacingDirection4(isFlipped ? 3 : 2);
        }

        private int Direction4FromVector(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return currentFacingDir;
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                return dir.x >= 0f ? 2 : 3; // 2=Right, 3=Left
            return dir.y >= 0f ? 1 : 0;     // 1=Up, 0=Down
        }

        /// <summary>
        /// Đảm bảo vũ khí của hệ phái đã sẵn sàng và sprite được gán đầy đủ trước khi kích hoạt hoạt ảnh.
        /// </summary>
        public void EnsureWeaponVisualReady(ClassBranch targetBranch)
        {
            EnsureWeaponObjects();
            if (classWeapon == null || classWeaponBranch != targetBranch)
            {
                ApplyClassWeapon(targetBranch);
            }
            if (currentEquippedWeapon == null)
            {
                currentEquippedWeapon = classWeapon;
            }
            if (weaponRenderer != null)
            {
                string key = WeaponFileKey(targetBranch);
                weaponRenderer.sprite = GetProceduralWeaponSprite(key);
                weaponRenderer.sortingLayerID = GetBaseSortingLayerID();
                weaponRenderer.sortingOrder = GetBaseSortingOrder() + 12;
            }
        }

        /// <summary>
        /// Hoạt ảnh Chém Vòng Cung cho Phái Cận Chiến (Chiến Binh / Tân Thủ) hỗ trợ cả 4 hướng / 360 độ theo chuột.
        /// </summary>
        public void PlayMeleeSlashAnimation(float duration = 0.2f)
        {
            Vector2 dir = Vector2.right;
            switch (currentFacingDir)
            {
                case 0: dir = Vector2.down; break;
                case 1: dir = Vector2.up; break;
                case 2: dir = Vector2.right; break;
                case 3: dir = Vector2.left; break;
            }
            PlayMeleeSlashAnimation(dir, duration, 130f);
        }

        public void PlayMeleeSlashAnimation(Vector2 aimDir, float duration = 0.22f, float arcDegrees = 135f)
        {
            ClassBranch branch = (classWeaponBranch == ClassBranch.Warrior || classWeaponBranch == ClassBranch.Novice)
                ? classWeaponBranch
                : ClassBranch.Warrior;
            EnsureWeaponVisualReady(branch);
            EnsureSubEffectRenderers();
            if (weaponHolder == null || weaponRenderer == null) return;
            if (!isActiveAndEnabled) return;

            StopCurrentAttack();
            HideSubEffects();
            attackCoroutine = StartCoroutine(MeleeSlashDirectionalCoroutine(aimDir, duration, arcDegrees));
        }

        /// <summary>
        /// Hoạt ảnh Cắm Kiếm Xuống Đất (Địa Chấn - Tremor Slam):
        /// Hiện thanh kiếm giơ lên cao rồi cắm thẳng xuống mặt đất tạo chấn động.
        /// </summary>
        public void PlaySwordPlungeAnimation(Vector2 aimDir, float duration = 0.42f, System.Action<Vector3> onGroundImpact = null)
        {
            EnsureWeaponVisualReady(ClassBranch.Warrior);
            EnsureSubEffectRenderers();
            if (weaponHolder == null || weaponRenderer == null) return;
            if (!isActiveAndEnabled)
            {
                onGroundImpact?.Invoke(transform.position + (Vector3)(aimDir.normalized * 0.4f));
                return;
            }

            StopCurrentAttack();
            HideSubEffects();
            pendingReleaseCallback = onGroundImpact;
            pendingReleaseWorldPos = transform.position + (Vector3)(aimDir.normalized * 0.4f);
            attackCoroutine = StartCoroutine(SwordPlungeCoroutine(aimDir, duration));
        }

        /// <summary>
        /// Hoạt ảnh Giương Cung & Hiện Mũi Tên Kéo Cung cho Phái Xạ Thủ (Archer):
        /// 1. Hiện cây cung & hiện mũi tên trên cung hướng về con trỏ chuột.
        /// 2. Kéo lùi mũi tên và dây cung.
        /// 3. Thả dây bắn mũi tên lao về phía trước rồi ẩn cung.
        /// </summary>
        public void PlayBowShotAnimation(Vector2 aimDir, float drawDuration = 0.14f, System.Action onRelease = null)
        {
            PlayBowDrawWithArrowAnimation(aimDir, drawDuration, _ => onRelease?.Invoke(), Color.white);
        }

        public void PlayBowDrawWithArrowAnimation(Vector2 aimDir, float drawDuration = 0.15f, System.Action<Vector3> onReleaseWithPos = null, Color? arrowTint = null)
        {
            EnsureWeaponVisualReady(ClassBranch.Archer);
            EnsureSubEffectRenderers();
            Vector2 normDir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            Vector3 defaultReleasePos = transform.position + (Vector3)(normDir * 0.55f);

            if (weaponHolder == null || weaponRenderer == null || !isActiveAndEnabled)
            {
                onReleaseWithPos?.Invoke(defaultReleasePos);
                return;
            }

            StopCurrentAttack();
            HideSubEffects();
            pendingReleaseCallback = onReleaseWithPos;
            pendingReleaseWorldPos = defaultReleasePos;
            attackCoroutine = StartCoroutine(BowShotWithArrowCoroutine(normDir, drawDuration, arrowTint.HasValue ? arrowTint.Value : Color.white));
        }

        /// <summary>
        /// Hoạt ảnh Đưa Gậy Phép & Phát Sáng Đầu Gậy cho Phái Pháp Sư (Mage):
        /// 1. Hiện gậy phép hướng về phía con trỏ chuột.
        /// 2. Trên đầu gậy phép hiện quả cầu ánh sáng theo màu chỉ định (Trắng / Đỏ / Xanh / Vàng).
        /// 3. Phóng chiêu thức từ đầu gậy phép về hướng chuột rồi thu gậy ẩn đi.
        /// </summary>
        public void PlayStaffThrustAnimation(Vector2 aimDir, float thrustDuration = 0.15f, System.Action onThrust = null)
        {
            PlayStaffLightCastAnimation(aimDir, Color.white, thrustDuration, 0.1f, _ => onThrust?.Invoke());
        }

        public void PlayStaffLightCastAnimation(Vector2 aimDir, Color tipLightColor, float chargeDuration = 0.16f, float holdDuration = 0.12f, System.Action<Vector3> onCastRelease = null)
        {
            EnsureWeaponVisualReady(ClassBranch.Mage);
            EnsureSubEffectRenderers();
            Vector2 normDir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            Vector3 defaultTipPos = transform.position + (Vector3)(normDir * 0.85f);

            if (weaponHolder == null || weaponRenderer == null || !isActiveAndEnabled)
            {
                onCastRelease?.Invoke(defaultTipPos);
                return;
            }

            StopCurrentAttack();
            HideSubEffects();
            pendingReleaseCallback = onCastRelease;
            pendingReleaseWorldPos = defaultTipPos;
            attackCoroutine = StartCoroutine(StaffLightCastCoroutine(normDir, tipLightColor, chargeDuration, holdDuration));
        }

        public void PlayAttackSlashAnimation(float duration = 0.2f)
        {
            PlayMeleeSlashAnimation(duration);
        }

        public void PlayCastAnimation(float duration = 0.3f)
        {
            EnsureSubEffectRenderers();
            if (weaponHolder == null || weaponRenderer == null || !isActiveAndEnabled) return;

            StopCurrentAttack();
            HideSubEffects();
            attackCoroutine = StartCoroutine(CastCoroutine(duration));
        }

        private IEnumerator MeleeSlashDirectionalCoroutine(Vector2 aimDir, float duration, float arcDegrees)
        {
            Vector2 dir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            currentFacingDir = Direction4FromVector(dir);

            isAttacking = true;
            ShowWeapon();

            Vector2 grip = GetGripOffsetForDirection(currentFacingDir);
            Vector3 basePos = new Vector3(grip.x * 0.45f, grip.y * 0.45f, 0f) + (Vector3)(dir * 0.14f);
            weaponHolder.localPosition = basePos;
            weaponHolder.localScale = new Vector3(0.92f, 0.92f, 1f);

            // Sprite kiếm định hướng chuẩn 0 độ (+X), xoay quanh góc ngắm chuột
            float centerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float halfArc = arcDegrees * 0.5f;
            bool swingReverse = (dir.x < -0.05f);
            float startAngle = swingReverse ? (centerAngle - halfArc) : (centerAngle + halfArc);
            float endAngle = swingReverse ? (centerAngle + halfArc) : (centerAngle - halfArc);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                float eased = t * t * (3f - 2f * t);
                float currentAngle = Mathf.Lerp(startAngle, endAngle, eased);
                weaponHolder.rotation = Quaternion.Euler(0, 0, currentAngle);
                weaponHolder.localPosition = basePos + (Vector3)(dir * Mathf.Sin(t * Mathf.PI) * 0.10f);
                yield return null;
            }

            ResetHolderToRestPose();
            isAttacking = false;
            attackCoroutine = null;
            HideWeapon();
        }

        private IEnumerator SwordPlungeCoroutine(Vector2 aimDir, float duration)
        {
            Vector2 dir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.down;
            currentFacingDir = Direction4FromVector(dir);

            isAttacking = true;
            ShowWeapon();

            // Mũi kiếm hướng thẳng xuống đất (-90 độ so với trục +X)
            weaponHolder.localScale = new Vector3(0.98f, 0.98f, 1f);
            weaponHolder.rotation = Quaternion.Euler(0, 0, -90f);

            Vector3 highPos = new Vector3(dir.x * 0.16f, 0.46f, 0f);
            Vector3 groundPos = new Vector3(dir.x * 0.22f, -0.10f, 0f);

            float raiseTime = duration * 0.28f;
            float slamTime = duration * 0.27f;
            float holdTime = duration * 0.45f;

            float elapsed = 0f;
            while (elapsed < raiseTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, raiseTime));
                weaponHolder.localPosition = Vector3.Lerp(new Vector3(0f, 0.06f, 0f), highPos, t);
                weaponHolder.rotation = Quaternion.Euler(0, 0, -90f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < slamTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, slamTime));
                weaponHolder.localPosition = Vector3.Lerp(highPos, groundPos, t * t);
                weaponHolder.rotation = Quaternion.Euler(0, 0, -90f);
                yield return null;
            }

            // Khoảnh khắc mũi kiếm cắm xuống mặt đất -> Phát sóng chấn động hình nón
            pendingReleaseWorldPos = transform.position + groundPos + (Vector3)(dir * 0.25f);
            FlushPendingCallback();

            elapsed = 0f;
            while (elapsed < holdTime)
            {
                elapsed += Time.deltaTime;
                float shake = Mathf.Sin(elapsed * 55f) * 0.02f * (1f - elapsed / Mathf.Max(0.01f, holdTime));
                weaponHolder.localPosition = groundPos + new Vector3(shake, 0f, 0f);
                weaponHolder.rotation = Quaternion.Euler(0, 0, -90f);
                yield return null;
            }

            ResetHolderToRestPose();
            isAttacking = false;
            attackCoroutine = null;
            HideWeapon();
        }

        private IEnumerator BowShotWithArrowCoroutine(Vector2 dir, float drawDuration, Color arrowTint)
        {
            currentFacingDir = Direction4FromVector(dir);

            isAttacking = true;
            ShowWeapon();

            float aimAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Đưa cây cung ra phía trước thân nhân vật vừa vặn với tầm tay Xạ Thủ
            Vector3 bowBasePos = (Vector3)(dir * 0.20f);
            weaponHolder.localPosition = bowBasePos;
            weaponHolder.localScale = new Vector3(0.90f, 0.90f, 1f);
            weaponHolder.rotation = Quaternion.Euler(0, 0, aimAngle);

            // Bật hiển thị mũi tên nhỏ gọn sắc nét đặt trên dây cung
            if (nockedArrowRenderer != null)
            {
                nockedArrowRenderer.sprite = GetNockedArrowSprite();
                nockedArrowRenderer.color = arrowTint;
                nockedArrowRenderer.sortingLayerID = GetBaseSortingLayerID();
                nockedArrowRenderer.sortingOrder = GetBaseSortingOrder() + 14;
                nockedArrowRenderer.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
                nockedArrowRenderer.transform.localScale = new Vector3(0.82f, 0.82f, 1f);
                nockedArrowRenderer.enabled = true;
            }

            // Bước 1 & 2: Hiện mũi tên -> Kéo cung lùi nhẹ kèm uốn căng cánh cung
            float elapsed = 0f;
            float safeDraw = Mathf.Max(0.05f, drawDuration);
            while (elapsed < safeDraw)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDraw);
                weaponHolder.localPosition = bowBasePos - (Vector3)(dir * (0.05f * t));
                weaponHolder.localScale = Vector3.Lerp(new Vector3(0.90f, 0.90f, 1f), new Vector3(0.80f, 1.02f, 1f), t);
                weaponHolder.rotation = Quaternion.Euler(0, 0, aimAngle);

                if (nockedArrowRenderer != null)
                {
                    nockedArrowRenderer.transform.position = weaponHolder.position + (Vector3)(dir * Mathf.Lerp(0.10f, -0.04f, t));
                    nockedArrowRenderer.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
                }
                yield return null;
            }

            // Bước 3: Thả dây cung -> Ẩn mũi tên trên cung & phóng mũi tên bay về phía trước theo hướng chuột
            pendingReleaseWorldPos = weaponHolder.position + (Vector3)(dir * 0.18f);
            if (nockedArrowRenderer != null) nockedArrowRenderer.enabled = false;
            FlushPendingCallback();

            // Cây cung nảy nhẹ về vị trí cũ rồi ẩn đi
            float recoilDuration = 0.08f;
            float recoilElapsed = 0f;
            Vector3 drawnPos = weaponHolder.localPosition;
            Vector3 drawnScale = weaponHolder.localScale;

            while (recoilElapsed < recoilDuration)
            {
                recoilElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(recoilElapsed / recoilDuration);
                weaponHolder.localPosition = Vector3.Lerp(drawnPos, bowBasePos, t);
                weaponHolder.localScale = Vector3.Lerp(drawnScale, new Vector3(0.90f, 0.90f, 1f), t);
                weaponHolder.rotation = Quaternion.Euler(0, 0, aimAngle);
                yield return null;
            }

            ResetHolderToRestPose();
            isAttacking = false;
            attackCoroutine = null;
            HideSubEffects();
            HideWeapon();
        }

        private IEnumerator StaffLightCastCoroutine(Vector2 dir, Color tipLightColor, float chargeDuration, float holdDuration)
        {
            currentFacingDir = Direction4FromVector(dir);

            isAttacking = true;
            ShowWeapon();

            // Xác định hướng bên tay cầm gậy phép (Phải = +1, Trái = -1)
            float sideSign = (dir.x < -0.05f || (Mathf.Abs(dir.x) <= 0.05f && currentFacingDir == 3)) ? -1f : 1f;

            // Gậy phép xuất hiện ở tư thế THẲNG ĐỨNG (0 độ), đặt gọn bên thân Pháp Sư
            Vector3 restPos = new Vector3(sideSign * 0.16f, -0.02f + dir.y * 0.04f, 0f);
            Vector3 tiltedPos = restPos + new Vector3(sideSign * 0.04f, 0.02f, 0f) + (Vector3)(dir * 0.04f);
            float uprightAngle = 0f;
            // Khi tung chiêu: đầu gậy nghiêng nhẹ về phía trước (~22 độ theo hướng xuất chiêu)
            float forwardTiltAngle = -sideSign * 22f;

            weaponHolder.localPosition = restPos;
            weaponHolder.localScale = new Vector3(0.92f, 0.92f, 1f);
            weaponHolder.rotation = Quaternion.Euler(0f, 0f, uprightAngle);

            // Khoảng cách từ tay cầm (y_bottom = 8) lên tâm viên ngọc đỉnh gậy thẳng đứng (y_bottom = 19) ở 32 PPU * scale 0.92
            float tipLocalUpDist = (11f / 32f) * 0.92f;

            if (staffTipGlowRenderer != null && staffTipCoreRenderer != null)
            {
                int baseOrder = GetBaseSortingOrder();
                int layerId = GetBaseSortingLayerID();

                staffTipGlowRenderer.sortingLayerID = layerId;
                staffTipGlowRenderer.sortingOrder = baseOrder + 15;
                Color glowCol = tipLightColor;
                glowCol.a = 0.95f;
                staffTipGlowRenderer.color = glowCol;
                staffTipGlowRenderer.enabled = true;

                staffTipCoreRenderer.sortingLayerID = layerId;
                staffTipCoreRenderer.sortingOrder = baseOrder + 16;
                staffTipCoreRenderer.color = Color.Lerp(tipLightColor, Color.white, 0.78f);
                staffTipCoreRenderer.enabled = true;
            }

            float safeCharge = Mathf.Max(0.08f, chargeDuration);
            float tiltPhaseDuration = safeCharge * 0.42f;
            float gatherPhaseDuration = safeCharge * 0.58f;

            // Bước 1: Hiện gậy phép thẳng đứng -> Nghiêng nhẹ đầu gậy về phía trước
            float elapsed = 0f;
            while (elapsed < tiltPhaseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, tiltPhaseDuration));
                float eased = t * t * (3f - 2f * t);

                float curAngle = Mathf.Lerp(uprightAngle, forwardTiltAngle, eased);
                weaponHolder.localPosition = Vector3.Lerp(restPos, tiltedPos, eased);
                weaponHolder.rotation = Quaternion.Euler(0f, 0f, curAngle);

                Vector3 tipWorldPos = weaponHolder.position + weaponHolder.up * tipLocalUpDist;
                if (staffTipGlowRenderer != null)
                {
                    staffTipGlowRenderer.transform.position = tipWorldPos;
                    float s = Mathf.Lerp(0.18f, 0.42f, eased);
                    staffTipGlowRenderer.transform.localScale = new Vector3(s, s, 1f);
                }
                if (staffTipCoreRenderer != null)
                {
                    staffTipCoreRenderer.transform.localPosition = Vector3.zero;
                    staffTipCoreRenderer.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
                }
                yield return null;
            }

            // Bước 2: Giữ đầu gậy nghiêng nhẹ về phía trước & Tụ quang rực sáng trên đỉnh trượng
            elapsed = 0f;
            while (elapsed < gatherPhaseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, gatherPhaseDuration));
                float pulse = Mathf.Sin(t * Mathf.PI);

                weaponHolder.localPosition = tiltedPos;
                weaponHolder.rotation = Quaternion.Euler(0f, 0f, forwardTiltAngle);

                Vector3 tipWorldPos = weaponHolder.position + weaponHolder.up * tipLocalUpDist;
                if (staffTipGlowRenderer != null)
                {
                    staffTipGlowRenderer.transform.position = tipWorldPos;
                    float s = Mathf.Lerp(0.42f, 0.78f, t) + pulse * 0.06f;
                    staffTipGlowRenderer.transform.localScale = new Vector3(s, s, 1f);
                }
                if (staffTipCoreRenderer != null)
                {
                    staffTipCoreRenderer.transform.localPosition = Vector3.zero;
                    float coreS = Mathf.Lerp(0.44f, 0.58f, t);
                    staffTipCoreRenderer.transform.localScale = new Vector3(coreS, coreS, 1f);
                }
                yield return null;
            }

            // Bước 3: Phóng chiêu thức / Triệu hồi từ đúng đỉnh ngọc của đầu gậy phép đang nghiêng về trước
            pendingReleaseWorldPos = weaponHolder.position + weaponHolder.up * tipLocalUpDist;
            FlushPendingCallback();

            // Bước 4: Đưa gậy phép từ góc nghiêng trở lại thẳng đứng (0 độ) rồi ẩn đi
            elapsed = 0f;
            float safeHold = Mathf.Max(0.06f, holdDuration);
            while (elapsed < safeHold)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeHold);
                weaponHolder.localPosition = Vector3.Lerp(tiltedPos, restPos, t);
                weaponHolder.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(forwardTiltAngle, uprightAngle, t));
                if (staffTipGlowRenderer != null)
                {
                    staffTipGlowRenderer.transform.position = weaponHolder.position + weaponHolder.up * tipLocalUpDist;
                    float s = Mathf.Lerp(0.75f, 0.14f, t);
                    staffTipGlowRenderer.transform.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }

            ResetHolderToRestPose();
            isAttacking = false;
            attackCoroutine = null;
            HideSubEffects();
            HideWeapon();
        }

        private IEnumerator CastCoroutine(float duration)
        {
            isAttacking = true;
            ShowWeapon();
            float elapsed = 0f;
            float sideSign = (currentFacingDir == 3) ? -1f : 1f;
            float uprightAngle = 0f;
            float tiltedAngle = -sideSign * 20f;
            Vector3 startPos = weaponHolder.localPosition;
            Vector3 tiltedPos = startPos + new Vector3(sideSign * 0.04f, 0.04f, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 2f / Mathf.Max(0.01f, duration), 1f);
                weaponHolder.localPosition = Vector3.Lerp(startPos, tiltedPos, t);
                weaponHolder.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(uprightAngle, tiltedAngle, t));
                yield return null;
            }

            ResetHolderToRestPose();
            isAttacking = false;
            attackCoroutine = null;
            HideSubEffects();
            HideWeapon();
        }
    }
}
