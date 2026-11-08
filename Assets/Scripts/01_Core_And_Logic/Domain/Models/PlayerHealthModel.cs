using System;
using ActionRPG.Core.Domain.Interfaces;
using ActionRPG.Core.Domain.ValueObjects;
using ActionRPG.Core.Services;

namespace ActionRPG.Core.Domain.Models
{
    /// <summary>
    /// Pure C# Model quản lý sinh lực, quy tắc nhận đòn và trạng thái sống/chết.
    /// Không chứa bất kỳ liên kết nào tới MonoBehaviour, SpriteRenderer hay UI.
    /// </summary>
    public class PlayerHealthModel : IHealthModel
    {
        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }
        public float PhysicalDef { get; private set; }
        public float MagicDef { get; private set; }
        public float Defense => PhysicalDef;
        public bool IsDead => CurrentHp <= 0f;

        // Observer Pattern / C# Events
        public event Action<float, float> OnHealthChanged; // (currentHp, maxHp)
        public event Action<DamageData> OnDamageTaken;      // (damageData)
        public event Action OnDied;

        public PlayerHealthModel(float maxHp, float physicalDef = 0f, float magicDef = 0f)
        {
            MaxHp = Math.Max(1f, maxHp);
            PhysicalDef = Math.Max(0f, physicalDef);
            MagicDef = Math.Max(0f, magicDef > 0f ? magicDef : physicalDef);
            CurrentHp = MaxHp;
        }

        public void SetDefense(float newDefense)
        {
            PhysicalDef = Math.Max(0f, newDefense);
        }

        public void SetDefenses(float newPhysicalDef, float newMagicDef)
        {
            PhysicalDef = Math.Max(0f, newPhysicalDef);
            MagicDef = Math.Max(0f, newMagicDef);
        }

        public void SetMaxHp(float newMaxHp, bool healToFull = false)
        {
            MaxHp = Math.Max(1f, newMaxHp);
            if (healToFull || CurrentHp > MaxHp)
            {
                CurrentHp = MaxHp;
            }
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public void TakeDamage(DamageData damage)
        {
            if (IsDead || damage.Amount <= 0f) return;

            float finalDamage = DamageCalculationService.CalculateFinalDamage(
                damage.Amount,
                damage.Type,
                PhysicalDef,
                MagicDef,
                1.0f,
                damage.IsCritical
            );
            CurrentHp = Math.Max(0f, CurrentHp - finalDamage);

            // Gửi dữ liệu ra bên ngoài cho Presenter/Views tiêu thụ
            var processedDamage = new DamageData(
                finalDamage,
                damage.IsCritical,
                damage.ElementId,
                damage.KnockbackX,
                damage.KnockbackY,
                damage.KnockbackForce,
                damage.Type
            );

            OnDamageTaken?.Invoke(processedDamage);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
