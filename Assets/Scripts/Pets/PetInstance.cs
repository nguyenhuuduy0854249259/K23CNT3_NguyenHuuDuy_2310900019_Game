using System;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Items;
using ActionRPG.Core;

namespace ActionRPG.Pets
{
    public enum PetHungerState
    {
        Starving,   // < 20%: Giảm 20% sát thương và tốc độ
        Hungry,     // 20% - 50%: Giảm 10% sát thương
        Satisfied,  // 50% - 80%: Chỉ số bình thường
        Full        // > 80%: Tăng 10% EXP nhận được, tăng gắn kết Bond nhanh hơn
    }

    /// <summary>
    /// Lưu trữ dữ liệu thực tế trong quá trình chơi (Runtime Data) của một Thú Cưng:
    /// Cấp độ, EXP, Máu hiện tại, Điểm thân mật (Bond: 0 - 100), Độ no đói, và trạng thái triệu hồi.
    /// </summary>
    [Serializable]
    public class PetInstance
    {
        public string instanceId;
        public PetData petData;
        public string customName;

        [Header("Tiến Trình & Trạng Thái")]
        public int level = 1;
        public int currentExp = 0;
        public float currentHp = 80f;
        [Range(0f, 100f)] public float bondLevel = 50f; // Điểm thân mật: 0 - 100
        [Range(0f, 100f)] public float fullness = 100f;  // Trạng thái no đói: 0 - 100
        public bool isSummoned = false;

        // Callback sự kiện runtime
        public Action<float, float> OnHpChanged;
        public Action<float, float> OnBondChanged;
        public Action<int, int> OnExpChanged;
        public Action<int> OnLevelUp;

        public PetInstance()
        {
            instanceId = Guid.NewGuid().ToString();
            level = 1;
            currentExp = 0;
            bondLevel = 50f;
            fullness = 100f;
            isSummoned = false;
        }

        public PetInstance(PetData data, int initialLevel = 1, float initialBond = 50f)
        {
            instanceId = Guid.NewGuid().ToString();
            petData = data;
            level = Mathf.Max(1, initialLevel);
            currentExp = 0;
            bondLevel = Mathf.Clamp(initialBond, 0f, 100f);
            fullness = 100f;
            isSummoned = false;

            if (petData != null)
            {
                currentHp = MaxHp;
            }
        }

        #region Thuộc Tính Tính Toán (Computed Stats)
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(customName)) return customName;
                if (petData != null) return petData.petName;
                return "Thú Cưng";
            }
        }

        public ElementType Element => petData != null ? petData.element : ElementType.None;
        public PetRole Role => petData != null ? petData.role : PetRole.DPS;
        public PetType PetType => petData != null ? petData.petType : PetType.Beast;

        public float MaxHp
        {
            get
            {
                if (petData == null) return 80f;
                float baseVal = petData.maxHp + (level - 1) * 14f;
                float bondMultiplier = 1f + (bondLevel / 100f) * 0.15f; // +15% HP ở max Bond
                return Mathf.Round(baseVal * bondMultiplier);
            }
        }

        public float AttackPower
        {
            get
            {
                if (petData == null) return 10f;
                float baseVal = petData.attackPower + (level - 1) * 2.8f;
                float bondMultiplier = 1f + (bondLevel / 100f) * 0.20f; // +20% ATK ở max Bond
                float hungerMultiplier = HungerState == PetHungerState.Starving ? 0.8f :
                                         HungerState == PetHungerState.Hungry ? 0.9f : 1.0f;
                return Mathf.Round(baseVal * bondMultiplier * hungerMultiplier * 10f) / 10f;
            }
        }

        public float Defense
        {
            get
            {
                if (petData == null) return 4f;
                float baseVal = petData.defense + (level - 1) * 1.2f;
                float bondMultiplier = 1f + (bondLevel / 100f) * 0.15f;
                return Mathf.Round(baseVal * bondMultiplier * 10f) / 10f;
            }
        }

        public float MoveSpeed
        {
            get
            {
                if (petData == null) return 5f;
                float speed = petData.moveSpeed;
                if (bondLevel >= 80f) speed *= 1.10f; // +10% Tốc chạy khi thân thiết
                if (HungerState == PetHungerState.Starving) speed *= 0.85f;
                return speed;
            }
        }

        public int ExpToNextLevel => level * 60;

        public PetHungerState HungerState
        {
            get
            {
                if (fullness < 20f) return PetHungerState.Starving;
                if (fullness < 50f) return PetHungerState.Hungry;
                if (fullness <= 80f) return PetHungerState.Satisfied;
                return PetHungerState.Full;
            }
        }
        #endregion

        #region Hành Vi & Quản Lý Dữ Liệu
        public void GainExp(int exp)
        {
            if (exp <= 0) return;

            // Thưởng thêm EXP nếu Pet no đủ và thân mật cao
            if (HungerState == PetHungerState.Full)
            {
                exp = Mathf.RoundToInt(exp * 1.15f);
            }

            currentExp += exp;
            while (currentExp >= ExpToNextLevel)
            {
                currentExp -= ExpToNextLevel;
                level++;
                currentHp = MaxHp; // Hồi đầy máu khi lên cấp
                ModifyBond(5f);    // Lên cấp tăng 5 điểm thân mật

                Debug.Log($"<color=green>[PetInstance] {DisplayName} đã thăng lên Cấp {level}! (Máu tối đa: {MaxHp}, ATK: {AttackPower})</color>");
                OnLevelUp?.Invoke(level);
                EventBus.OnPetLevelUp?.Invoke(level);
            }

            OnExpChanged?.Invoke(currentExp, ExpToNextLevel);
            EventBus.OnPetExpChanged?.Invoke(currentExp, ExpToNextLevel);
        }

        public void TakeDamage(float rawDamage)
        {
            float finalDamage = Mathf.Max(1f, rawDamage - Defense);
            currentHp = Mathf.Max(0f, currentHp - finalDamage);

            OnHpChanged?.Invoke(currentHp, MaxHp);
            EventBus.OnPetHpChanged?.Invoke(currentHp, MaxHp);

            // Bị đánh đau giảm nhẹ 0.5 bond nếu HP tụt quá thấp
            if (currentHp <= MaxHp * 0.15f)
            {
                ModifyBond(-0.5f);
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            currentHp = Mathf.Min(MaxHp, currentHp + amount);
            OnHpChanged?.Invoke(currentHp, MaxHp);
            EventBus.OnPetHpChanged?.Invoke(currentHp, MaxHp);
        }

        public void ModifyBond(float delta)
        {
            float oldBond = bondLevel;
            bondLevel = Mathf.Clamp(bondLevel + delta, 0f, 100f);

            if (!Mathf.Approximately(oldBond, bondLevel))
            {
                OnBondChanged?.Invoke(bondLevel, 100f);
                EventBus.OnPetBondChanged?.Invoke(bondLevel, 100f);
            }
        }

        public void ConsumeHunger(float amount)
        {
            fullness = Mathf.Clamp(fullness - amount, 0f, 100f);
            if (fullness <= 0f)
            {
                ModifyBond(-0.1f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Cho Pet ăn thức ăn để hồi no đói và tăng điểm thân mật Bond.
        /// Thức ăn ưa thích sẽ nhân đôi điểm gắn kết!
        /// </summary>
        public bool Feed(ItemData foodItem, string foodName = "")
        {
            string targetFoodName = foodItem != null ? foodItem.itemName : foodName;
            bool isFavorite = false;

            if (petData != null)
            {
                if (foodItem != null && petData.favoriteFoodItem != null && foodItem == petData.favoriteFoodItem)
                    isFavorite = true;
                else if (!string.IsNullOrEmpty(petData.favoriteFood) && targetFoodName.ToLower().Contains(petData.favoriteFood.ToLower()))
                    isFavorite = true;
            }

            float fullnessGain = 35f;
            float bondGain = isFavorite ? 15f : 6f;

            fullness = Mathf.Clamp(fullness + fullnessGain, 0f, 100f);
            ModifyBond(bondGain);
            Heal(MaxHp * 0.25f); // Ăn uống hồi 25% máu

            Debug.Log($"<color=yellow>[Pet Nuôi Dưỡng] Cho {DisplayName} ăn '{targetFoodName}'! {(isFavorite ? "⭐ MÓN ƯA THÍCH! +15 Bond" : "+6 Bond")}. Thân mật hiện tại: {bondLevel:F0}/100.</color>");
            return true;
        }

        public bool CanEvolve()
        {
            if (petData == null || petData.nextEvolution == null) return false;
            return level >= petData.evolutionRequiredLevel;
        }

        public bool Evolve()
        {
            if (!CanEvolve()) return false;

            PetData next = petData.nextEvolution;
            petData = next;
            currentHp = MaxHp;
            ModifyBond(20f); // Tiến hóa thành công tăng 20 bond

            Debug.Log($"<color=cyan>[Pet Tiến Hóa] Chúc mừng! {DisplayName} đã tiến hóa thành '{petData.petName}'!</color>");
            OnHpChanged?.Invoke(currentHp, MaxHp);
            EventBus.OnPetHpChanged?.Invoke(currentHp, MaxHp);
            return true;
        }
        #endregion
    }
}
