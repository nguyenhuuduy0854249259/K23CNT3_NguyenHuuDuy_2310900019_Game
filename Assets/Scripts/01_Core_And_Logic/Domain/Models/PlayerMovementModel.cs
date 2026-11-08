using System;
using ActionRPG.Core.Domain.Interfaces;

namespace ActionRPG.Core.Domain.Models
{
    /// <summary>
    /// Pure C# Model quản lý trạng thái di chuyển, lướt né đòn (Dash) và hướng quay mặt.
    /// Hoàn toàn độc lập với Rigidbody2D, Time.deltaTime hay Unity Physics.
    /// </summary>
    public class PlayerMovementModel : IMovementModel
    {
        public float BaseSpeed { get; private set; }
        public float DashSpeed { get; private set; }
        public float DashDuration { get; private set; }
        public float DashCooldown { get; private set; }

        public bool IsMoving { get; private set; }
        public bool IsDashing { get; private set; }
        public int FacingDirectionX { get; private set; } = 1; // 1 = Phải, -1 = Trái

        private float currentDashTimer = 0f;
        private float currentDashCooldownTimer = 0f;
        private float dashDirX = 1f;
        private float dashDirY = 0f;

        // C# Events
        public event Action<int> OnFacingChanged;
        public event Action<bool> OnMovementStateChanged;
        public event Action OnDashStarted;
        public event Action OnDashEnded;

        public PlayerMovementModel(float baseSpeed = 4.5f, float dashSpeed = 10.5f, float dashDuration = 0.22f, float dashCooldown = 0.8f)
        {
            BaseSpeed = Math.Max(0.1f, baseSpeed);
            DashSpeed = Math.Max(0.1f, dashSpeed);
            DashDuration = Math.Max(0.05f, dashDuration);
            DashCooldown = Math.Max(0.1f, dashCooldown);
        }

        public void SetBaseSpeed(float newSpeed)
        {
            BaseSpeed = Math.Max(0.1f, newSpeed);
        }

        public (float vx, float vy) ComputeVelocity(float inputX, float inputY)
        {
            if (IsDashing)
            {
                return (dashDirX * DashSpeed, dashDirY * DashSpeed);
            }

            bool wasMoving = IsMoving;
            IsMoving = (Math.Abs(inputX) > 0.01f || Math.Abs(inputY) > 0.01f);

            if (wasMoving != IsMoving)
            {
                OnMovementStateChanged?.Invoke(IsMoving);
            }

            // Cập nhật hướng nhìn
            if (inputX > 0.05f && FacingDirectionX != 1)
            {
                FacingDirectionX = 1;
                OnFacingChanged?.Invoke(FacingDirectionX);
            }
            else if (inputX < -0.05f && FacingDirectionX != -1)
            {
                FacingDirectionX = -1;
                OnFacingChanged?.Invoke(FacingDirectionX);
            }

            // Chuẩn hóa vector di chuyển
            float length = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
            if (length > 1f)
            {
                inputX /= length;
                inputY /= length;
            }

            return (inputX * BaseSpeed, inputY * BaseSpeed);
        }

        public bool TryStartDash(float inputX, float inputY)
        {
            if (IsDashing || currentDashCooldownTimer > 0f) return false;

            IsDashing = true;
            currentDashTimer = DashDuration;
            currentDashCooldownTimer = DashCooldown;

            // Xác định hướng lướt: theo hướng di chuyển hiện tại hoặc hướng mặt
            float len = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
            if (len > 0.01f)
            {
                dashDirX = inputX / len;
                dashDirY = inputY / len;
            }
            else
            {
                dashDirX = FacingDirectionX;
                dashDirY = 0f;
            }

            OnDashStarted?.Invoke();
            return true;
        }

        public void UpdateTimers(float deltaTime)
        {
            if (currentDashCooldownTimer > 0f)
            {
                currentDashCooldownTimer -= deltaTime;
            }

            if (IsDashing)
            {
                currentDashTimer -= deltaTime;
                if (currentDashTimer <= 0f)
                {
                    IsDashing = false;
                    OnDashEnded?.Invoke();
                }
            }
        }
    }
}
