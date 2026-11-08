using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Điều khiển cơ chế tương tác đạo cụ tách lớp của Cư Dân Oakhaven (NPC_Villager_Wanderer):
    /// - Cung cấp đầy đủ 4 hàm cốt lõi:
    ///   1. DropBasket(): Tách Giỏ hoa quả (Prop_FruitBasket) khỏi Parent của NPC để đặt xuống mặt đất (World Space)
    ///      và chuyển hoạt ảnh thân NPC sang đi bộ tay không (Unarmed).
    ///   2. PickUpBasket(): Nhặt Giỏ hoa quả gắn trở lại vào Parent của NPC và chuyển sang hoạt ảnh 2 tay bê giỏ (Carrying).
    ///   3. TakeOffHat(): Tách Mũ rơm (Prop_StrawHat) khỏi Parent của NPC đặt xuống mặt đất (World Space),
    ///      để lộ mái tóc nâu hạt dẻ đầy đặn hoàn chỉnh (không bị hói).
    ///   4. PutOnHat(): Đội Mũ rơm trở lại lên đầu NPC (gắn lại vào Parent của NPC).
    /// </summary>
    [RequireComponent(typeof(DirectionalWalkAnimator))]
    public class VillagerPropInteract : MonoBehaviour
    {
        [Header("Tham Chiếu 2 Child Objects Đạo Cụ Tách Rời")]
        public NPCModularProp strawHatProp;     // Prop_StrawHat
        public NPCModularProp fruitBasketProp;  // Prop_FruitBasket

        [Header("Sprite Khi Đặt Xuống Mặt Đất (World Space 32x32, PPU = 16)")]
        public Sprite worldHatSprite;
        public Sprite worldBasketSprite;

        [Header("Bộ Hoạt Ảnh Đi Bộ Khi Đang Bê Giỏ (Carrying State - 4 Hướng)")]
        public Sprite carryIdleFront;
        public Sprite carryIdleBack;
        public Sprite carryIdleSide;
        public Sprite carryIdleLeft;
        public Sprite[] carryWalkFrontFrames;
        public Sprite[] carryWalkBackFrames;
        public Sprite[] carryWalkSideFrames;
        public Sprite[] carryWalkLeftFrames;

        [Header("Bộ Hoạt Ảnh Đi Bộ Khi Tay Không (Unarmed State - 4 Hướng)")]
        public Sprite unarmedIdleFront;
        public Sprite unarmedIdleBack;
        public Sprite unarmedIdleSide;
        public Sprite unarmedIdleLeft;
        public Sprite[] unarmedWalkFrontFrames;
        public Sprite[] unarmedWalkBackFrames;
        public Sprite[] unarmedWalkSideFrames;
        public Sprite[] unarmedWalkLeftFrames;

        [Header("Vị Trí Đặt Đạo Cụ Xuống Đất (World Offset So Với Chân NPC)")]
        public Vector3 basketGroundOffset = new Vector3(0.32f, -0.14f, 0f);
        public Vector3 hatGroundOffset = new Vector3(-0.32f, -0.16f, 0f);

        [Header("Trạng Thái Hiện Tại")]
        public bool isWearingHat = true;
        public bool isCarryingBasket = true;

        [Header("Chu Trình Tự Động Đặt / Nhặt Đạo Cụ Khi Đi Dạo")]
        public bool enableAutonomousRoutine = true;
        public float routineInterval = 10.0f;

        private DirectionalWalkAnimator walkAnimator;
        private NPCModularCharacter2D modularCharacter;
        private SpriteRenderer bodySr;
        private float routineTimer = 0f;

        private void Awake()
        {
            walkAnimator = GetComponent<DirectionalWalkAnimator>();
            modularCharacter = GetComponent<NPCModularCharacter2D>();
            bodySr = GetComponent<SpriteRenderer>();

            AutoBindChildProps();
            CacheDefaultAnimationsIfUnset();
            routineTimer = Random.Range(routineInterval * 0.6f, routineInterval);
        }

        private void Start()
        {
            ApplyBodyAnimationSet(isCarryingBasket);
        }

        private void Update()
        {
            if (!enableAutonomousRoutine) return;

            routineTimer -= Time.deltaTime;
            if (routineTimer <= 0f)
            {
                routineTimer = routineInterval;
                if (isCarryingBasket || isWearingHat)
                {
                    DropBasket();
                    TakeOffHat();
                }
                else
                {
                    PickUpBasket();
                    PutOnHat();
                }
            }
        }

        private void AutoBindChildProps()
        {
            if (strawHatProp == null)
            {
                Transform hatTr = transform.Find("Prop_StrawHat");
                if (hatTr != null)
                {
                    strawHatProp = hatTr.GetComponent<NPCModularProp>();
                }
            }

            if (fruitBasketProp == null)
            {
                Transform basketTr = transform.Find("Prop_FruitBasket");
                if (basketTr != null)
                {
                    fruitBasketProp = basketTr.GetComponent<NPCModularProp>();
                }
            }
        }

        private void CacheDefaultAnimationsIfUnset()
        {
            if (walkAnimator == null) return;

            if (carryIdleFront == null) carryIdleFront = walkAnimator.idleFront;
            if (carryIdleBack == null) carryIdleBack = walkAnimator.idleBack;
            if (carryIdleSide == null) carryIdleSide = walkAnimator.idleSide;
            if (carryIdleLeft == null) carryIdleLeft = walkAnimator.idleLeft;

            if (carryWalkFrontFrames == null || carryWalkFrontFrames.Length == 0)
                carryWalkFrontFrames = walkAnimator.walkFrontFrames;
            if (carryWalkBackFrames == null || carryWalkBackFrames.Length == 0)
                carryWalkBackFrames = walkAnimator.walkBackFrames;
            if (carryWalkSideFrames == null || carryWalkSideFrames.Length == 0)
                carryWalkSideFrames = walkAnimator.walkSideFrames;
            if (carryWalkLeftFrames == null || carryWalkLeftFrames.Length == 0)
                carryWalkLeftFrames = walkAnimator.walkLeftFrames;
        }

        /// <summary>
        /// Tách Giỏ hoa quả (Prop_FruitBasket) khỏi Parent của NPC để đặt xuống mặt đất (World Space),
        /// đồng thời chuyển hoạt ảnh thân nhân vật sang trạng thái đi bộ tay không (Unarmed).
        /// </summary>
        [ContextMenu("Drop Basket (Đặt Giỏ Hoa Quả Xuống Đất)")]
        public void DropBasket()
        {
            AutoBindChildProps();
            if (fruitBasketProp == null || !isCarryingBasket) return;

            Transform basketTr = fruitBasketProp.transform;
            Vector3 worldDropPos = transform.position + basketGroundOffset;

            // Tách khỏi Parent của NPC ra World Space
            basketTr.SetParent(null, true);
            basketTr.position = worldDropPos;
            basketTr.rotation = Quaternion.identity;
            basketTr.localScale = transform.localScale;

            // Khóa cập nhật theo hướng NPC khi đang nằm dưới đất
            fruitBasketProp.enabled = false;

            SpriteRenderer basketSr = fruitBasketProp.GetComponent<SpriteRenderer>();
            if (basketSr != null)
            {
                basketSr.flipX = false;
                basketSr.color = Color.white;
                if (worldBasketSprite != null)
                {
                    basketSr.sprite = worldBasketSprite;
                }
                else if (fruitBasketProp.spriteFront != null)
                {
                    basketSr.sprite = fruitBasketProp.spriteFront;
                }

                int baseOrder = bodySr != null ? bodySr.sortingOrder : 5000;
                basketSr.sortingOrder = baseOrder - 1;
            }

            isCarryingBasket = false;
            ApplyBodyAnimationSet(false);
        }

        /// <summary>
        /// Nhặt Giỏ hoa quả (Prop_FruitBasket) từ mặt đất (World Space) gắn trở lại vào Parent của NPC,
        /// đồng thời chuyển hoạt ảnh thân nhân vật sang trạng thái 2 tay bê giỏ (Carrying).
        /// </summary>
        [ContextMenu("Pick Up Basket (Nhặt Giỏ Hoa Quả Lên Tay)")]
        public void PickUpBasket()
        {
            AutoBindChildProps();
            if (fruitBasketProp == null || isCarryingBasket) return;

            Transform basketTr = fruitBasketProp.transform;

            // Gắn trở lại làm Child Object của NPC
            basketTr.SetParent(transform, false);
            basketTr.localPosition = Vector3.zero;
            basketTr.localRotation = Quaternion.identity;
            basketTr.localScale = Vector3.one;

            fruitBasketProp.enabled = true;
            fruitBasketProp.gameObject.SetActive(true);

            isCarryingBasket = true;
            ApplyBodyAnimationSet(true);

            if (modularCharacter != null)
            {
                modularCharacter.RefreshProps();
            }
        }

        /// <summary>
        /// Tháo Mũ rơm (Prop_StrawHat) tách khỏi Parent của NPC để đặt xuống mặt đất (World Space),
        /// để lộ mái tóc nâu hạt dẻ đầy đặn hoàn chỉnh trên đầu NPC.
        /// </summary>
        [ContextMenu("Take Off Hat (Tháo Mũ Rơm Đặt Xuống Đất)")]
        public void TakeOffHat()
        {
            AutoBindChildProps();
            if (strawHatProp == null || !isWearingHat) return;

            Transform hatTr = strawHatProp.transform;
            Vector3 worldDropPos = transform.position + hatGroundOffset;

            // Tách khỏi Parent của NPC ra World Space
            hatTr.SetParent(null, true);
            hatTr.position = worldDropPos;
            hatTr.rotation = Quaternion.identity;
            hatTr.localScale = transform.localScale;

            strawHatProp.enabled = false;

            SpriteRenderer hatSr = strawHatProp.GetComponent<SpriteRenderer>();
            if (hatSr != null)
            {
                hatSr.flipX = false;
                hatSr.color = Color.white;
                if (worldHatSprite != null)
                {
                    hatSr.sprite = worldHatSprite;
                }
                else if (strawHatProp.spriteFront != null)
                {
                    hatSr.sprite = strawHatProp.spriteFront;
                }

                int baseOrder = bodySr != null ? bodySr.sortingOrder : 5000;
                hatSr.sortingOrder = baseOrder - 1;
            }

            isWearingHat = false;
        }

        /// <summary>
        /// Nhặt và đội lại Mũ rơm (Prop_StrawHat) từ mặt đất gắn trở lại vào Parent trên đầu NPC.
        /// </summary>
        [ContextMenu("Put On Hat (Đội Mũ Rơm Lên Đầu)")]
        public void PutOnHat()
        {
            AutoBindChildProps();
            if (strawHatProp == null || isWearingHat) return;

            Transform hatTr = strawHatProp.transform;

            // Gắn trở lại làm Child Object trên đầu NPC
            hatTr.SetParent(transform, false);
            hatTr.localPosition = Vector3.zero;
            hatTr.localRotation = Quaternion.identity;
            hatTr.localScale = Vector3.one;

            strawHatProp.enabled = true;
            strawHatProp.gameObject.SetActive(true);

            isWearingHat = true;

            if (modularCharacter != null)
            {
                modularCharacter.RefreshProps();
            }
        }

        /// <summary>
        /// Chuyển đổi linh hoạt giữa bộ Animation đi bộ bê đồ (Carrying) và bộ Animation đi bộ tay không (Unarmed).
        /// </summary>
        private void ApplyBodyAnimationSet(bool carrying)
        {
            if (walkAnimator == null) return;

            if (carrying)
            {
                if (carryIdleFront != null) walkAnimator.idleFront = carryIdleFront;
                if (carryIdleBack != null) walkAnimator.idleBack = carryIdleBack;
                if (carryIdleSide != null) walkAnimator.idleSide = carryIdleSide;
                if (carryIdleLeft != null) walkAnimator.idleLeft = carryIdleLeft;

                if (carryWalkFrontFrames != null && carryWalkFrontFrames.Length > 0)
                    walkAnimator.walkFrontFrames = carryWalkFrontFrames;
                if (carryWalkBackFrames != null && carryWalkBackFrames.Length > 0)
                    walkAnimator.walkBackFrames = carryWalkBackFrames;
                if (carryWalkSideFrames != null && carryWalkSideFrames.Length > 0)
                    walkAnimator.walkSideFrames = carryWalkSideFrames;
                if (carryWalkLeftFrames != null && carryWalkLeftFrames.Length > 0)
                    walkAnimator.walkLeftFrames = carryWalkLeftFrames;
            }
            else
            {
                if (unarmedIdleFront != null) walkAnimator.idleFront = unarmedIdleFront;
                if (unarmedIdleBack != null) walkAnimator.idleBack = unarmedIdleBack;
                if (unarmedIdleSide != null) walkAnimator.idleSide = unarmedIdleSide;
                if (unarmedIdleLeft != null) walkAnimator.idleLeft = unarmedIdleLeft;

                if (unarmedWalkFrontFrames != null && unarmedWalkFrontFrames.Length > 0)
                    walkAnimator.walkFrontFrames = unarmedWalkFrontFrames;
                if (unarmedWalkBackFrames != null && unarmedWalkBackFrames.Length > 0)
                    walkAnimator.walkBackFrames = unarmedWalkBackFrames;
                if (unarmedWalkSideFrames != null && unarmedWalkSideFrames.Length > 0)
                    walkAnimator.walkSideFrames = unarmedWalkSideFrames;
                if (unarmedWalkLeftFrames != null && unarmedWalkLeftFrames.Length > 0)
                    walkAnimator.walkLeftFrames = unarmedWalkLeftFrames;
            }

            if (bodySr != null && !walkAnimator.isMoving)
            {
                switch (walkAnimator.currentFacing)
                {
                    case FacingDirection2D.Front:
                        if (walkAnimator.idleFront != null) bodySr.sprite = walkAnimator.idleFront;
                        break;
                    case FacingDirection2D.Back:
                        if (walkAnimator.idleBack != null) bodySr.sprite = walkAnimator.idleBack;
                        break;
                    case FacingDirection2D.Side:
                        if (walkAnimator.isFacingLeft && walkAnimator.idleLeft != null)
                            bodySr.sprite = walkAnimator.idleLeft;
                        else if (walkAnimator.idleSide != null)
                            bodySr.sprite = walkAnimator.idleSide;
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            if (strawHatProp != null && strawHatProp.transform.parent == null)
            {
                Destroy(strawHatProp.gameObject);
            }
            if (fruitBasketProp != null && fruitBasketProp.transform.parent == null)
            {
                Destroy(fruitBasketProp.gameObject);
            }
        }
    }
}
