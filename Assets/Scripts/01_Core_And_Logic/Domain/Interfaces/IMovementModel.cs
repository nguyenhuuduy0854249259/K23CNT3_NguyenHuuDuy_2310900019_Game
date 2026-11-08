using System;

namespace ActionRPG.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface Pure C# cho logic di chuyển và lướt (Dash).
    /// </summary>
    public interface IMovementModel
    {
        float BaseSpeed { get; }
        bool IsMoving { get; }
        bool IsDashing { get; }
        int FacingDirectionX { get; }

        event Action<int> OnFacingChanged;
        event Action<bool> OnMovementStateChanged;
        event Action OnDashStarted;
        event Action OnDashEnded;

        (float vx, float vy) ComputeVelocity(float inputX, float inputY);
        bool TryStartDash(float inputX, float inputY);
        void UpdateTimers(float deltaTime);
    }
}
