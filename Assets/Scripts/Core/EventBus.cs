using System;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Core
{
    public static class EventBus
    {
        // Player & Stats
        public static Action<int> OnPlayerLevelUp;
        public static Action<int, int> OnPlayerExpChanged;
        public static Action<float, float> OnPlayerHpChanged;
        public static Action<float, float> OnPlayerMpChanged;
        public static Action<float, float, string> OnHungerChanged; // current, max, statusName
        public static Action<string> OnClassChanged; // className
        public static Action<ActionRPG.Classes.CharacterClassData> OnClassApplied; // Đồng bộ kỹ năng + ActionBar + vũ khí + animation

        // Combat & VFX & SFX
        public static Action<Vector3, float, bool, bool> OnShowDamageText; // pos, amount, isCrit, isHeal
        public static Action<string> OnShowUIMessage; // Thông báo dạng Toast/Banner nổi trên UI
        public static Action<string> OnPlaySound; // Phát âm thanh SFX (error, equip, pickup, potion...)
        public static Action<GameObject, int, int> OnEnemyKilled; // enemyObj, exp, gold

        // Pet
        public static Action OnPetSystemUnlocked; // Mở khóa tính năng Pet khi đạt Cấp độ quy định (Mặc định Cấp 10)
        public static Action<string> OnPetCaptured; // petName
        public static Action<float, float> OnPetHpChanged; // current, max
        public static Action<float, float> OnPetBondChanged; // current, max (100)
        public static Action<int, int> OnPetExpChanged; // current, max
        public static Action<int> OnPetLevelUp; // newLevel
        public static Action<string, float> OnPetSkillTriggered; // skillName, cooldown
        public static Action<object> OnPetSummoned; // PetInstance
        public static Action<object> OnPetDismissed; // PetInstance
        public static Action<string, bool> OnPetCaptureResult; // petName, isSuccess

        // Inventory & Quest
        public static Action OnInventoryUpdated;
        public static Action<string> OnQuestProgressUpdated;

        // Skills & Cooldowns
        public static Action<System.Collections.Generic.List<ActionRPG.Classes.SkillData>> OnSkillChanged; // Bắn danh sách kỹ năng khi đổi class hoặc nạp lại
        public static void RaiseSkillChanged(System.Collections.Generic.List<ActionRPG.Classes.SkillData> skills) => OnSkillChanged?.Invoke(skills);
        public static Action<string, float> OnSkillCooldown; // skillId, remainingSeconds
        public static Action<string> OnSkillRequested; // skillId requested by UI or hotkey
        public static void RaiseSkillRequested(string skillId) => OnSkillRequested?.Invoke(skillId);

        // Stats & Equipment
        public static Action OnStatsChanged;
        public static Action OnEquipmentChanged;

        // Boss & Encounter (Sprint 2)
        public static Action<string, float, float, ElementType> OnBossHpChanged; // bossName, currentHp, maxHp, element
        public static Action<string, int> OnBossPhaseChanged; // bossName, phase
        public static Action<string> OnBossSpawned; // bossName
        public static Action<string> OnBossDefeated; // bossName
    }
}
