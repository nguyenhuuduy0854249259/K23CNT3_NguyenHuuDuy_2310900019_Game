using System;
using ActionRPG.Core.Domain.ValueObjects;

namespace ActionRPG.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface Pure C# cho các thực thể có thể nhận sát thương và có máu.
    /// Cho phép Mocking và viết Unit Test độc lập hoàn toàn với Unity GameObject.
    /// </summary>
    public interface IHealthModel
    {
        float CurrentHp { get; }
        float MaxHp { get; }
        bool IsDead { get; }

        event Action<float, float> OnHealthChanged;
        event Action<DamageData> OnDamageTaken;
        event Action OnDied;

        void TakeDamage(DamageData damage);
        void Heal(float amount);
    }
}
