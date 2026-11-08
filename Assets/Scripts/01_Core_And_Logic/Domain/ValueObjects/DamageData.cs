using System;
using ActionRPG.Combat;

namespace ActionRPG.Core.Domain.ValueObjects
{
    /// <summary>
    /// Pure C# Value Object đại diện cho dữ liệu sát thương.
    /// Hoàn toàn độc lập với UnityEngine (dễ Unit Test, serialization).
    /// </summary>
    public readonly struct DamageData : IEquatable<DamageData>
    {
        public readonly float Amount;
        public readonly bool IsCritical;
        public readonly int ElementId;
        public readonly float KnockbackX;
        public readonly float KnockbackY;
        public readonly float KnockbackForce;
        public readonly DamageType Type;

        public DamageData(float amount, bool isCritical = false, int elementId = 0, float knockbackX = 0f, float knockbackY = 0f, float knockbackForce = 0f, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            IsCritical = isCritical;
            ElementId = elementId;
            KnockbackX = knockbackX;
            KnockbackY = knockbackY;
            KnockbackForce = knockbackForce;
            Type = type;
        }

        public bool Equals(DamageData other)
        {
            return Amount.Equals(other.Amount) &&
                   IsCritical == other.IsCritical &&
                   ElementId == other.ElementId &&
                   KnockbackX.Equals(other.KnockbackX) &&
                   KnockbackY.Equals(other.KnockbackY) &&
                   KnockbackForce.Equals(other.KnockbackForce) &&
                   Type == other.Type;
        }

        public override bool Equals(object obj) => obj is DamageData other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Amount.GetHashCode();
                hash = (hash * 397) ^ IsCritical.GetHashCode();
                hash = (hash * 397) ^ ElementId;
                hash = (hash * 397) ^ KnockbackForce.GetHashCode();
                hash = (hash * 397) ^ Type.GetHashCode();
                return hash;
            }
        }
    }
}
